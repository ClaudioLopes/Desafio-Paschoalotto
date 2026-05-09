# ServicoMonitorPasta

A .NET 10 Windows Service that monitors a system folder and logs every file creation and deletion to a `.log` file.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Requirements](#requirements)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [How to Run](#how-to-run)
- [Deploy as a Windows Service](#deploy-as-a-windows-service)
- [Internationalization](#internationalization)
- [Tests](#tests)
- [Architecture](#architecture)

---

## Overview

**ServicoMonitorPasta** is a Windows Service that uses `FileSystemWatcher` to monitor a configurable folder in real time. Every file creation or deletion event is recorded in a `.log` file with a precise timestamp.

**Sample log output:**
```
[2026-05-09 14:32:01.123] [CREATED ] C:\MonitoredFolder\report.pdf
[2026-05-09 14:33:45.456] [DELETED ] C:\MonitoredFolder\temp.txt
```

---

## Requirements

- Windows 10 / Windows Server 2016 or later
- .NET 10 SDK
- Visual Studio 2022 (for development)

---

## Project Structure

```
Desafio-Paschoalotto/
├── ServicoMonitorPasta/
│   ├── Program.cs                  # Entry point and host configuration
│   ├── ServicoMonitoramento.cs     # Core service logic
│   ├── OpcoesMonitor.cs            # Configuration class
│   ├── appsettings.json            # Externalized settings
│   ├── Mensagens.resx              # Resources in Portuguese (default)
│   ├── Mensagens.en.resx           # Resources in English
│   └── Mensagens.es.resx           # Resources in Spanish
│
└── ServicoMonitorPasta.Testes/
    ├── OpcoesMonitorTestes.cs       # Configuration class tests
    ├── MensagensTestes.cs           # Internationalization tests
    └── ServicoMonitoramentoTestes.cs # Service behavior tests
```

---

## Configuration

Edit `appsettings.json` to adjust the paths:

```json
{
  "FileMonitor": {
    "CaminhoPasta": "C:\\MonitoredFolder",
    "CaminhoLog": "C:\\Logs\\file-monitor.log",
    "IncluirSubpastas": false
  }
}
```

| Property | Description | Default |
|---|---|---|
| `CaminhoPasta` | Folder to monitor | `C:\MonitoredFolder` |
| `CaminhoLog` | Full path to the log file | `C:\Logs\file-monitor.log` |
| `IncluirSubpastas` | Monitor subfolders recursively | `false` |

> Folders are created automatically by the service if they do not exist.

---

## How to Run

### Development mode (Visual Studio)

1. Open `Desafio-Paschoalotto.sln` in Visual Studio 2022
2. Set `ServicoMonitorPasta` as the startup project
3. Press **F5**
4. Create or delete files in `C:\MonitoredFolder`
5. Check the log at `C:\Logs\file-monitor.log`

### Development mode (terminal)

```bash
cd ServicoMonitorPasta
dotnet run
```

---

## Deploy as a Windows Service

**1. Publish the executable:**
```bash
dotnet publish -c Release -r win-x64 --self-contained -o C:\Services\FileMonitor
```

**2. Install the service (as Administrator):**
```bash
sc create ServicoMonitorPasta binPath="C:\Services\FileMonitor\ServicoMonitorPasta.exe"
sc description ServicoMonitorPasta "Monitors a folder and logs file events"
sc config ServicoMonitorPasta start=auto
```

**3. Start:**
```bash
sc start ServicoMonitorPasta
```

**4. Check status:**
```bash
sc query ServicoMonitorPasta
```

**5. Stop and remove:**
```bash
sc stop ServicoMonitorPasta
sc delete ServicoMonitorPasta
```

---

## Internationalization

The service supports three languages via `ResourceManager`. The language is automatically selected based on the operating system culture.

| Culture | File | Language |
|---|---|---|
| `pt` (default) | `Mensagens.resx` | Portuguese |
| `en` | `Mensagens.en.resx` | English |
| `es` | `Mensagens.es.resx` | Spanish |

To force a specific language during development, temporarily add to `Program.cs`:

```csharp
Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
```

---

## Tests

The project includes **22 unit tests** using **xUnit** and **Moq**.

### Run all tests

```bash
cd ServicoMonitorPasta.Testes
dotnet test
```

### Expected result

```
total: 22 | failed: 0 | passed: 22 | skipped: 0
```

### Test coverage

| Test File | Tests | What it covers |
|---|---|---|
| `OpcoesMonitorTestes.cs` | 5 | Default values and property changes |
| `MensagensTestes.cs` | 12 | Resources in all 3 languages (PT, EN, ES) |
| `ServicoMonitoramentoTestes.cs` | 5 | Folder creation, event logging, lifecycle |

---

## Architecture

### Event flow

```
FileSystemWatcher
      │
      ▼
OnArquivoCriado / OnArquivoDeletado
      │
      ▼
ConcurrentQueue<string>  ◄── thread-safe
      │
      ▼
SemaphoreSlim (signal)
      │
      ▼
ExecuteAsync (main loop)
      │
      ▼
GravarLogAsync (with automatic retry)
      │
      ▼
file-monitor.log
```

### Technical decisions

**`ConcurrentQueue` + `SemaphoreSlim`** — `FileSystemWatcher` events arrive on OS threads. The queue ensures thread-safety without manual locks, and the semaphore wakes the write loop only when there are events, avoiding polling.

**Batch writing** — the loop drains the entire queue at once before opening the file, reducing I/O operations in high-volume event scenarios.

**Automatic retry** — on `IOException` (log file in use), the service retries up to 3 times with a growing 200ms backoff.

**Externalized configuration** — all operational parameters are in `appsettings.json`, requiring no recompilation to change paths.
