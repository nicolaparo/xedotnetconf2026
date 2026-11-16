# Demo di C# 15 ed EF Core 11

[English](readme.md)

Scopri le novità di C# 15 ed Entity Framework Core 11 con brevi esempi eseguibili da XE .NET Conf 2026. Confronta le nuove funzionalità con C# 14 ed EF Core 10, leggi i commenti negli esempi e prova a modificare il codice.

## Per iniziare

Installa [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) e gli [SDK .NET 10 e .NET 11](https://dotnet.microsoft.com/download). Gli esempi sono stati eseguiti con SDK 10.0.401 e 11.0.100-rc.3, ed EF Core 10.0.14 e 11.0.0-rc.3. La configurazione .NET 11 consente SDK prerelease e seleziona la versione major più recente installata: controlla l'SDK mostrato dal runner. I pacchetti NuGet vengono ripristinati alla prima esecuzione.

```powershell
git clone https://github.com/nicolaparo/xedotnetconf2026.git
cd xedotnetconf2026
./run.ps1 -List
./run.ps1 01-union -Target net11
./run.ps1 01-union -Variant old
./run.ps1 01-union -Pause
```

Per impostazione predefinita il runner esegue la variante `new` su entrambi gli SDK; `-Target net10` o `-Target net11` ne seleziona uno solo. Un esempio `new` può intenzionalmente non compilare o comportarsi diversamente su .NET 10. La variante `old`, quando presente, mostra l'approccio precedente. Alcuni esempi hanno solo la variante `new`. Il runner copia ogni file nella directory generata `net10/work` o `net11/work` per selezionare SDK e versioni dei pacchetti; modifica i file originali in `scenarios/`, non le copie. Usa `./run.ps1 -Clean` per rimuovere le directory generate.

## Scegli uno scenario

| Scenari | Argomenti |
| --- | --- |
| 01–04 | Union types, union manuali, serializzazione JSON e mapping EF Core |
| 05–08 | `with` nelle collection, break/continue con label, gerarchie chiuse e indexer nelle extension |
| 09–14 | Split query, `FullJoin`, `MaxBy`, complex type con ereditarietà, property chaining e query JSON path |
| 15 | Memory safety e flag di compilazione preview |
| 16–17 | Ricerca vettoriale, full-text e ibrida |

La maggior parte degli esempi non richiede un server database: usa SQLite in memoria oppure stampa l'SQL generato. Gli scenari 16 e 17 richiedono i servizi descritti sotto. Lo scenario 15 confronta i flag del compilatore anziché le versioni degli SDK; i dettagli sono nel sorgente.

## Configurazione della ricerca vettoriale e ibrida

Per gli scenari 16 e 17 esegui [Ollama](https://ollama.com/) in locale e scarica il modello per gli embedding:

```powershell
ollama pull qwen3-embedding:0.6b
```

Assicurati che il servizio Ollama sia attivo su `http://localhost:11434` (se necessario avvialo con `ollama serve`). Gli esempi usano embedding a 1024 dimensioni prodotti da questo modello. Imposta `XE_OLLAMA` se il server si trova altrove.

Serve anche SQL Server 2025, con un account autorizzato a creare e cancellare database e ad abilitare le funzionalità preview a livello di database. Per esempio, con Docker installato, avvia un'istanza locale usa e getta per lo scenario 16:

```powershell
docker run --name xe-sql2025 -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Xe!Demo2026pass" -p 14335:1433 -d mcr.microsoft.com/mssql/server:2025-latest
```

Attendi che SQL Server sia pronto prima di eseguire la demo. La password è destinata solo a un'istanza locale usa e getta. Lo scenario 17 richiede anche il componente **Full-Text Search** di SQL Server, incluso il supporto per la lingua italiana; installalo e abilitalo sull'istanza (vedi la [guida Microsoft per Linux](https://learn.microsoft.com/sql/linux/sql-server-linux-setup-full-text-search)) prima di eseguire lo scenario.

```powershell
./run.ps1 16 -Target net11
./run.ps1 17 -Target net11
```

Le connessioni predefinite usano `localhost,14335`, l'utente `sa`, la password sopra e i database `XeVector` (16) e `XeFullText` (17). Per usare un'altra istanza, imposta una stringa di connessione completa in PowerShell, per esempio:

```powershell
$env:XE_SQL = "Server=localhost,14335;Database=DisposableDemo;User Id=sa;Password=<your-password>;TrustServerCertificate=True"
$env:XE_OLLAMA = "http://localhost:11434"
```

**Attenzione:** ogni esecuzione dello scenario 16 o 17 cancella e ricrea il database indicato in `XE_SQL` (oppure il database predefinito). Usa solo un database usa e getta, mai un database esistente con dati da conservare. `./warmup.ps1` esegue tutti gli scenari, incluse queste demo distruttive.

Lo scenario 16 esegue la ricerca per distanza esatta, ma stampa soltanto l'SQL di `VectorSearch` di EF Core 11: nella configurazione provata, SQL Server 2025 CU10 ha rifiutato la sintassi generata.
