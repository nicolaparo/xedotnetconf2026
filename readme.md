# C# 15 and EF Core 11 demos

[Italiano](readme.it.md)

Explore what's new in C# 15 and Entity Framework Core 11 with short, runnable examples from XE .NET Conf 2026. Compare the new features with C# 14 and EF Core 10, read the comments in each example, and try changing the code yourself.

## Get started

Install [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) and the [.NET 10 and .NET 11 SDKs](https://dotnet.microsoft.com/download). The examples were run with SDK 10.0.401 and 11.0.100-rc.3, and EF Core 10.0.14 and 11.0.0-rc.3. The .NET 11 configuration allows prerelease SDKs and selects the latest installed major version, so check the SDK displayed by the runner. NuGet packages are restored on the first run.

```powershell
git clone https://github.com/nicolaparo/xedotnetconf2026.git
cd xedotnetconf2026
./run.ps1 -List
./run.ps1 01-union -Target net11
./run.ps1 01-union -Variant old
./run.ps1 01-union -Pause
```

By default the runner executes the `new` variant on both SDKs; `-Target net10` or `-Target net11` selects one. A `new` example may intentionally fail to compile or behave differently on .NET 10. The `old` variant, where present, shows the previous approach. Some examples only have a `new` variant. The runner copies each file into a generated `net10/work` or `net11/work` directory to select the SDK and package versions; edit the original files under `scenarios/`, not the copies. Use `./run.ps1 -Clean` to remove those generated directories.

## Choose a scenario

| Scenarios | Topics |
| --- | --- |
| 01–04 | Union types, manual unions, JSON serialization and EF Core mapping |
| 05–08 | Collection `with`, labeled break/continue, closed hierarchies and extension indexers |
| 09–14 | Split queries, `FullJoin`, `MaxBy`, complex types with inheritance, property chaining and JSON path queries |
| 15 | Memory safety and preview compiler flags |
| 16–17 | Vector search, full-text search and hybrid search |

Most examples require no database server: they use in-memory SQLite or print generated SQL. Scenarios 16 and 17 require the services below. Scenario 15 compares compiler flags rather than SDK versions; see its source for details.

## Vector and hybrid search setup

For scenarios 16 and 17, run [Ollama](https://ollama.com/) locally and download the embedding model:

```powershell
ollama pull qwen3-embedding:0.6b
```

Ensure the Ollama service is running at `http://localhost:11434` (start it with `ollama serve` if needed). The examples use 1024-dimensional embeddings from this model. Set `XE_OLLAMA` if the server is elsewhere.

You also need SQL Server 2025, with a login allowed to create and drop databases and enable database-scoped preview features. For example, with Docker installed, start a disposable local instance for scenario 16:

```powershell
docker run --name xe-sql2025 -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Xe!Demo2026pass" -p 14335:1433 -d mcr.microsoft.com/mssql/server:2025-latest
```

Wait until SQL Server is ready before running the demo. This password is only for a local disposable instance. Scenario 17 also requires SQL Server's **Full-Text Search** component, including Italian language support; install and enable it on the instance (see [Microsoft's Linux installation guide](https://learn.microsoft.com/sql/linux/sql-server-linux-setup-full-text-search)) before running that scenario.

```powershell
./run.ps1 16 -Target net11
./run.ps1 17 -Target net11
```

The default connections use `localhost,14335`, user `sa`, the password above, and databases `XeVector` (16) and `XeFullText` (17). To use another instance, set a complete connection string in PowerShell, for example:

```powershell
$env:XE_SQL = "Server=localhost,14335;Database=DisposableDemo;User Id=sa;Password=<your-password>;TrustServerCertificate=True"
$env:XE_OLLAMA = "http://localhost:11434"
```

**Warning:** each run of scenario 16 or 17 drops and recreates the database named in `XE_SQL` (or its default database). Use only a disposable database, never an existing database with data you need. `./warmup.ps1` runs all scenarios, including these destructive demos.

Scenario 16 executes exact distance search, but only prints the SQL for EF Core 11 `VectorSearch`: SQL Server 2025 CU10 rejected the generated syntax in the tested setup.
