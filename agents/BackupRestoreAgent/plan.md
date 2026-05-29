# Plan: BackupRestoreAgent

## Context

The RCE2 system is a broker-based agent network where each agent is a small standalone
program that long-polls a broker, declares typed inputs/outputs, and reacts to messages.
The repo's `agents/_CSharpBuilderBoilerplate` is the canonical C# starting point: a
`net8.0` console app that vendors the `Rce2/` library and integrates via a fluent
`Rce2Service` builder (`SetBrokerAddress` / `SetAgentId` / `SetInputDefinitions` /
`SetOutputDefinitions` / `Init`), subscribes to `PubSub.Hub.Default`, and emits outputs
with `Send(contact, payload)` (see `RCE2_INTEGRATION_GUIDE_FOR_AGENTS.md`).

We need a new agent, **BackupRestoreAgent**, that lets the network back up and restore a
folder on disk as timestamped 7-zip archives. It declares three inputs (`set-path`,
`backup`, `restore`) and one string output (`out`) used as a debug/result channel.

### Confirmed decisions
- **7-zip mechanism:** shell out to the installed `7z.exe` (auto-detect common install
  paths; overridable via config). Produces true `.7z` archives.
- **Backup storage:** a `backups/` subfolder next to the executable (alongside the config file).
- **Restore scope:** newest archive whose name encodes the current `set-path`'s folder name.

## New project layout

Create under `agents/BackupRestoreAgent/`, mirroring boilerplate structure:

```
agents/BackupRestoreAgent/
  BackupRestoreAgent.sln
  BackupRestoreAgent/
    BackupRestoreAgent.csproj      # net8.0, Exe, same 4 PackageReferences as boilerplate
    Program.cs                     # host wiring (copied, renamed namespace)
    App.cs                         # agent logic (the real work)
    Config/
      AgentConfig.cs               # POCO: SetPath, SevenZipPath
      ConfigRepository.cs          # load/save JSON in exe folder
    Services/
      BackupService.cs             # 7z invocation, backup/restore/list logic
    Rce2/                          # VERBATIM copy from boilerplate — do not modify
      Rce2Service.cs
      Extensions/Rce2StringListExtensions.cs
      Infrastructure/Rce2Message.cs
      Infrastructure/Rce2Sync.cs
      Infrastructure/Rce2Types.cs
      Infrastructure/Rce2Agent.cs
```

The `Rce2/` folder is vendored library code — copy as-is, namespace stays `Rce2`. Only the
host files (`Program.cs`, `App.cs`, new `Config/`, `Services/`) use namespace
`BackupRestoreAgent`.

## Contracts (input/output definitions)

```csharp
.SetInputDefinitions(new()
{
    { "set-path", Rce2Types.String },
    { "backup",   Rce2Types.Void },   // trigger
    { "restore",  Rce2Types.Void },   // trigger
})
.SetOutputDefinitions(new()
{
    { "out", Rce2Types.String },
})
```

`backup`/`restore` are triggers — their payload is ignored; only `Contact` matters.
`set-path` carries the path string at `Payload["data"]`.

## File responsibilities

### `Config/AgentConfig.cs`
POCO: `string? SetPath`, `string? SevenZipPath` (optional override).

### `Config/ConfigRepository.cs`
- Config path = `Path.Combine(AppContext.BaseDirectory, "config.json")` (executable's folder).
- `Load()` → deserialize or return new empty config if missing.
- `Save(AgentConfig)` → serialize with Newtonsoft.Json.
- Mirrors the load/save pattern used in `agents/FileSyncAgent/.../ConfigRepository`.

### `Services/BackupService.cs`
- `ResolveSevenZip()`: use config override if set, else probe `%ProgramFiles%\7-Zip\7z.exe`,
  `%ProgramFiles(x86)%\7-Zip\7z.exe`, then bare `7z` on PATH. Throw a clear message if none found.
- Backups dir = `Path.Combine(AppContext.BaseDirectory, "backups")` (created if absent).
- Archive naming: `{sanitizedFolderName}__{yyyyMMdd-HHmmss}.7z` where `sanitizedFolderName`
  comes from the leaf folder name of `set-path`. The `__` + timestamp suffix is what restore
  parses; the prefix scopes archives to a given path.
- `Backup(string sourcePath)`:
  - validate `sourcePath` is set and the directory exists;
  - run `7z a "<archive>" "<sourcePath>\*"` (or archive the folder itself — capture stdout/exit code);
  - on non-zero exit, surface stderr/stdout in the result string;
  - return a human-readable result line (archive name + status).
- `FindLatestBackup(string sourcePath)`: enumerate `backups/`, filter by `{sanitizedFolderName}__`
  prefix, parse the trailing timestamp, return the newest (or null).
- `Restore(string sourcePath)`:
  - require a matching backup to exist (else return "no backup found", do nothing — **safety gate**);
  - delete the existing `sourcePath` directory recursively **only after** the backup is confirmed;
  - run `7z x "<archive>" -o"<restoreParent>" -y` to extract;
  - return result string.
- All process invocation via `System.Diagnostics.Process` with `RedirectStandardOutput/Error`,
  capturing both for the `out` channel.

### `App.cs` (IHostedService)
- Inject `Rce2Service` + `ConfigRepository` + `BackupService`.
- `StartAsync`: build the fluent chain (broker `https://localhost:7113`, **stable** `AgentId`
  via a hardcoded `Guid` constant, name `"BackupRestore"`, the contracts above), call `Init()`.
- Subscribe `Hub.Default.Subscribe<Rce2Message>`:
  - `set-path` → read `Payload?["data"]?.ToObject<string>()`, store via config Save, `Send("out", "...")`.
  - `backup` → load config; if no `SetPath`, send error to `out`; else `BackupService.Backup`, send result.
  - `restore` → load config; if no `SetPath`, error; else `BackupService.Restore`, send result.
  - wrap each handler in try/catch and report exceptions through `out` (debug channel).
- `StopAsync`: `Hub.Default.Unsubscribe(this)`.

### `Program.cs`
Copy boilerplate; additionally register `ConfigRepository` and `BackupService` as singletons.

### `BackupRestoreAgent.csproj` / `.sln`
Same SDK/target/packages as boilerplate csproj; fresh project GUID in the `.sln`.

## Verification

Build the solution and confirm it compiles cleanly:
`dotnet build agents/BackupRestoreAgent/BackupRestoreAgent.sln`.

The user will handle functional/end-to-end testing.
