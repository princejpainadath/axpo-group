## AxpoGroup.PowerPosition

### 📖 Overview

AxpoGroup.PowerPosition is a .NET 10 solution designed to process and export power trading data. It includes services for retrieving trades, aggregating positions, exporting reports, and running scheduled background workers.

---

### 🧩 Architecture

The solution follows a **Clean Architecture** pattern with clear separation of concerns:

- **Application**  
  Contains core services such as `PowerReportService` for trade aggregation and reporting.

- **Domain**  
  Defines domain entities.

- **Infrastructure**  
  Provides data access implementations like `PowerTradeRepository` and reporting/export services such as `CsvReportWriter`.

- **Worker**  
  Hosts the `ScheduledExtractWorker` background service that runs scheduled extracts with retry logic.

- **Tests**  
  Each project has a corresponding `*.Tests` project for unit testing.

---

### 📂 Solution Structure

```text
/src
   /AxpoGroup.PowerPosition.slnx
   /AxpoGroup.PowerPosition.Application
   /AxpoGroup.PowerPosition.Application.Tests
   /AxpoGroup.PowerPosition.Domain
   /AxpoGroup.PowerPosition.Infrastructure
   /AxpoGroup.PowerPosition.Infrastructure.Tests
   /AxpoGroup.PowerPosition.Worker
   /AxpoGroup.PowerPosition.Worker.Tests
/.gitignore
/README.md
```

---

### ⚙️ Configuration

The solution uses strongly typed options classes:

- **CsvExportOptions**
  - `OutputDirectory`: Directory where CSV files are written.
  - `FileNamePattern`: Pattern for naming files (supports `{timestamp}`).
  - `TimestampFormat`: Format string for timestamps in file names.

- **ScheduledExtractOptions**
  - `ExtractIntervalMinutes`: Interval between scheduled extracts.
  - `MaxRetries`: Maximum retry attempts for failed extracts.
  - `RetryDelaySeconds`: Delay between retries.

Configuration is typically provided via `appsettings.json`.

---

### 🚀 Running the Worker

The `ScheduledExtractWorker` runs as a hosted background service:

1. Ensure configuration values are set in `appsettings.json`.
2. Build and run the Worker project:
   ```bash
   dotnet run --project src/AxpoGroup.PowerPosition.Worker
   ```
3. Logs will indicate when extracts start, succeed, or fail.

---

### 🧪 Testing

Unit tests are implemented using **xUnit** and **Moq** (for mocking dependencies).  
The solution includes dedicated test projects:

- AxpoGroup.PowerPosition.Application.Tests
- AxpoGroup.PowerPosition.Infrastructure.Tests
- AxpoGroup.PowerPosition.Worker.Tests

Run tests with:

```bash
dotnet test
```

---

### 📊 Reports

Reports are exported as CSV files, and each file is generated in the folder specified by the `OutputDirectory` property in the `appsettings.json` configuration.

```csv
Local Time,Volume
23:00,150
00:00,200
```

---
