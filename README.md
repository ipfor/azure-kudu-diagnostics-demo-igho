# Azure Kudu Diagnostics Demo

This repository contains a .NET 10 sample application designed to reproduce common production issues inside an Azure App Service so you can practise real-world diagnostics using Kudu and Visual Studio.

The full walkthrough article that explains every scenario in depth is here:  
[Azure Kudu Diagnostics: CPU Spikes, Memory Leaks, Thread Pool Starvation and Full Dump Analysis](https://jkrussell.dev/blog/azure-kudu-diagnostics-cpu-memory-threadpool-dumps/)

The code in this repository matches the examples used throughout that guide.

---

## Scenarios

The app exposes simple HTTP endpoints that intentionally create four types of problematic behaviour.

### High CPU

**Endpoint:** `POST /api/high-cpu`

Creates a 30 second CPU heavy workload using one task per logical core. Helps you reproduce:

- High CPU incidents  
- Kudu profiling `.diagsession` captures  
- Visual Studio CPU Usage hot path analysis  

### Managed memory leak

**Endpoint:** `POST /api/memory-leak`

Allocates 10 MB chunks every 0.5 seconds and stores them in a static list so they remain rooted.

Useful for:

- Demonstrating true managed leaks  
- Large, clearly visible heap growth  
- Static root inspections  
- Full dump heap analysis in Visual Studio  

### Memory bomb

**Endpoint:** `POST /api/memory-bomb`

Allocates a large amount of memory in one shot but does not retain references. This creates a temporary spike without a long term leak.

Useful for:

- Seeing dead objects in the heap  
- Demonstrating the difference between a leak and a spike  
- Observing how committed memory behaves after GC  

### Thread pool starvation

**Endpoint:** `POST /api/threadpool-starvation`

Queues 200 long running tasks that block worker threads for 60 seconds.

Useful for:

- Showing thread pool exhaustion without high CPU  
- Capturing thread lists and stacks in dumps  
- Explaining why low throughput does not always mean high CPU  

### Health check

**Endpoint:** `GET /api/health`

Simple connectivity test used by the demo UI.

---

## Requirements

- .NET 10 SDK  
- Visual Studio 2022 (or later) with ASP.NET and web development workload  
- Azure App Service (Windows) if you want to replicate the Kudu diagnostics steps

---

## Running locally

Clone and run the app:

```bash
git clone https://github.com/your-org/azure-kudu-diagnostics-demo.git
cd azure-kudu-diagnostics-demo
dotnet run
```

Then browse to:

```
https://localhost:5001/
```

This opens a small UI with buttons to trigger each scenario and view API responses.

---

## Deploying to Azure App Service

1. In Visual Studio, right click the project and choose Publish.  
2. Select Azure, then Azure App Service (Windows).  
3. Create or choose an existing App Service.  
4. Deploy using the generated `.pubxml` profile.

After deployment you can:

- Trigger scenarios from the hosted UI  
- Use Kudu Process Explorer to inspect the worker  
- Profile high CPU  
- Capture full memory dumps  
- Explore heap and threads in Visual Studio  

The linked article walks through each of these steps with screenshots and explanations.

---

## Project structure

```
KuduDiagnosticsDemo/
  Program.cs                 - Endpoint registrations and app startup
  Simulators/
    ProblemSimulator.cs      - CPU, memory leak, memory bomb and thread pool logic
  wwwroot/
    index.html               - Minimal UI for triggering scenarios
  Properties/
    PublishProfiles/         - Azure App Service publish profiles
```

---

## Safety notes

This demo intentionally stresses the host environment. It will:

- Use all available CPU  
- Allocate into the gigabyte range  
- Block many thread pool threads  

Only deploy it to test environments or temporary App Services.

---

## Further reading

For the full diagnostic walkthrough including CPU profiling, memory dump analysis, thread inspection, and Kudu techniques, read the companion article:

https://jkrussell.dev/blog/azure-kudu-diagnostics-cpu-memory-threadpool-dumps/

---

## Licence

See the `LICENSE` file for details.