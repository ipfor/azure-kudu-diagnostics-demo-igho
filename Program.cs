using KuduDiagnosticsDemo.Simulators;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// High CPU spike: fixed 30 seconds, one worker per core.
app.MapPost("/api/high-cpu", () =>
{
    var duration = TimeSpan.FromSeconds(30);
    var end = DateTime.UtcNow + duration;
    var workers = Environment.ProcessorCount;

    var tasks = new List<Task>(workers);

    for (var i = 0; i < workers; i++)
    {
        tasks.Add(Task.Run(() =>
        {
            while (DateTime.UtcNow < end)
            {
                for (var j = 0; j < 10_000_000; j++)
                {
                    _ = MathF.Sqrt(j * 123.456f);
                }
            }
        }));
    }

    Task.WaitAll(tasks.ToArray());

    return Results.Ok(new
    {
        message = "High CPU spike completed.",
        workers,
        durationSeconds = duration.TotalSeconds
    });
});

// 30 second memory leak.
app.MapPost("/api/memory-leak", () =>
{
    var duration = TimeSpan.FromSeconds(30);
    var total = ProblemSimulator.StartTimedLeak(duration);

    return Results.Ok(new
    {
        message = "Started 30 second memory leak.",
        durationSeconds = duration.TotalSeconds,
        approxTotalAllocatedMb = total
    });
});

// One-off memory bomb: allocate ~512 MB immediately.
app.MapPost("/api/memory-bomb", () =>
{
    var total = ProblemSimulator.AllocateMemoryBomb();

    return Results.Ok(new
    {
        message = "Allocated a one-off 512 MB memory bomb.",
        approxTotalAllocatedMb = total
    });
});

// Thread pool starvation demo.
app.MapPost("/api/threadpool-starvation", () =>
{
    ProblemSimulator.StartThreadPoolStarvation();

    return Results.Ok(new
    {
        message = "Started background work that will hold on to thread pool threads.",
        taskCount = 300,
        sleepMilliseconds = 60000
    });
});

app.MapGet("/api/health", () => Results.Ok(new { status = "OK" }));

await app.RunAsync();