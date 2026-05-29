# RCE2 — C# Agent Integration Guide

How to build an RCE2 agent in C# using the `Rce2/` library bundled in this boilerplate. The full, runnable reference is this project itself (`Program.cs` + `App.cs`); this document explains how the pieces fit so you can adapt it.

> For the broader picture — what RCE2 is, the broker, the wire protocol, other-language agents — see the repository-root `AGENTS.md` and `README.MD`.

## The one rule: don't touch `Rce2/`

The `Rce2/` folder is **vendored library code**. Copy it into your project as-is and leave it alone. You integrate by writing *host* code (`Program.cs`, your `IHostedService`, config) — never by editing the library. Everything below assumes `Rce2/` is unchanged.

## How an agent works

An agent is just an HTTP client. `Rce2Service` runs a background loop that:

1. long-polls `GET {broker}/api/agent/{agentId}` for its message feed,
2. answers any `whois` message automatically by POSTing the agent's identity (`Id`, `Name`, `Ins`, `Outs`),
3. publishes every other inbound message onto the in-process `PubSub.Hub.Default` bus.

You subscribe to that bus, react to messages whose `Contact` you care about, and call `Send(contact, payload)` to emit outputs (which POST to the same `/api/agent/{agentId}` endpoint). The agent never sees other agents — the broker routes outputs to inputs according to bindings configured in the broker UI.

## Setup in four moves

### 1. Project dependencies

The boilerplate targets `net8.0`. Required packages (see `CSharpBuilderBoilerplate.csproj`):

```xml
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="PubSub" Version="4.0.2" />
```

`Rce2Service` deserializes with Newtonsoft.Json and uses `IHttpClientFactory`, so `Newtonsoft.Json` and `Microsoft.Extensions.Http` are not optional.

### 2. Register services (`Program.cs`)

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<App>();   // your agent logic
builder.Services.AddHttpClient();           // required by Rce2Service
builder.Services.AddSingleton<Rce2Service>();

await builder.Build().RunAsync();
```

### 3. Configure and start (in your hosted service)

`Rce2Service` is a fluent builder — every `Set...` mutates the same instance and returns it, so the order among setters doesn't matter as long as everything is set before `Init()`. `Init()` starts the background feed loop; call it last.

```csharp
_rce2Service
    .SetBrokerAddress("https://localhost:7113")
    .SetAgentId(Guid.NewGuid())
    .SetAgentKey(string.Empty)
    .SetAgentName("Boilerplate")
    .SetInputDefinitions(new() { { "echo-test", Rce2Types.String } })
    .SetOutputDefinitions(new() { { "echo-test", Rce2Types.String } })
    .Init();
```

| Setter | Meaning |
|--------|---------|
| `SetBrokerAddress` | Base URL of the broker. The library appends `/api/agent/{id}`. |
| `SetAgentId` | This agent's identity on the broker. **In this repo keep `Guid.NewGuid()`** (see warning below). Swap in a static Guid only when deploying to production. |
| `SetAgentKey` | Optional auth key. Empty string ⇒ no auth header. Non-empty ⇒ sent as HTTP Basic (`Authorization: Basic base64(key)`). |
| `SetAgentName` | Human-readable name shown in the broker UI / `whois` reply. |
| `SetInputDefinitions` | `{ contact → type }` map of messages this agent can **receive**. |
| `SetOutputDefinitions` | `{ contact → type }` map of messages this agent can **send**. |

> **Keep `SetAgentId(Guid.NewGuid())` in committed code.** This is a public repo. If you hardcode a static Guid here, two people running unmodified checkouts will register the *same* agent id on the broker and collide. A fresh random id per run keeps developer instances isolated. Hardcoded static ids are a **production-only** change, applied at deploy time — not something to commit into the boilerplate or example agents.

Notes:
- `SetInputDefinitions` / `SetOutputDefinitions` **replace** the dictionary; they don't merge.
- Input and output contact names are independent — they need not match. This boilerplate reuses `echo-test` for both only because it echoes.
- `type` values come from `Rce2Types` (see below).

### 4. Handle messages

Subscribe on `Hub.Default`, filter by `Contact`, read the payload from `Payload["data"]`, and `Send` outputs. Unsubscribe on shutdown.

```csharp
public async Task StartAsync(CancellationToken ct)
{
    _rce2Service./* ...builder chain... */.Init();

    Hub.Default.Subscribe<Rce2Message>(this, async e =>
    {
        if (e.Contact != "echo-test") return;

        var value = e.Payload["data"]?.ToObject<string>();
        await Task.Delay(1000);
        await _rce2Service.Send("echo-test", value!);
    });
}

public Task StopAsync(CancellationToken ct)
{
    Hub.Default.Unsubscribe(this);
    return Task.CompletedTask;
}
```

This is the entire boilerplate behavior: receive an `echo-test` string, wait a second, send it back.

> **`echo-test` is not special — it's this example's made-up contact name.** Contact names are yours to choose; pick whatever describes your agent's inputs and outputs (`setColor`, `temperature`, `fire`, ...). The only reserved name in the protocol is `whois`, which is infrastructure: the broker sends it whenever it needs your identity, `Rce2Service` answers it for you, and **every** agent participates in it regardless of which contacts it declares. Don't declare `whois` in your input/output definitions and don't handle it yourself.

## Messages, payloads, and types

A message is `{ Type, Contact, Payload }` (`Rce2Message`). On the wire, `Send("echo-test", "hi")` produces:

```json
{ "type": "string", "contact": "echo-test", "payload": { "data": "hi" } }
```

- `Send` sets `Type` to the contact's declared output type automatically and wraps your payload as `{ "data": <payload> }`.
- So inbound, your value is always at `Payload["data"]` — use `.ToObject<T>()` to read it. **Validate before dereferencing**: `Payload?["data"]?.ToObject<T>()` rather than assuming shape.

`Rce2Types` constants:

| Constant | Wire value |
|----------|-----------|
| `WhoIs` | `whois` (infrastructure; handled for you) |
| `Void` | `void` |
| `String` / `StringList` | `string` / `string-list` |
| `Number` / `NumberList` | `number` / `number-list` |
| `Boolean` / `BooleanList` | `bool` / `bool-list` |
| `Custom` | `custom` |

### Key/value over `string-list`

`Rce2StringListExtensions.GetValue` treats a `List<string>` as alternating `[key, value, key, value, ...]` pairs — a lightweight way to pass structured data without a `custom` type:

```csharp
// list = ["color", "red", "size", "10"]
var color = list.GetValue("color"); // "red"
```

## Request/response over a fire-and-forget bus

The bus is one-way: you send, and replies arrive later as separate messages. When you need to *wait* for a correlated reply, use `Rce2Sync<T>`. It subscribes to the hub, runs your validator against each inbound message's `Payload["data"]`, and releases once one matches (or the timeout elapses).

```csharp
using var sync = new Rce2Sync<string>(
    validate: (data, msg) => Task.FromResult(msg.Contact == "pong"),
    timeout: 3000);

await _rce2Service.Send("ping", "hello");
await sync.WaitAsync();

var reply = sync.Result; // null if it timed out
```

Always dispose it (the `using` above) so it unsubscribes from the hub.

## Checklist for a correct agent

- Every contact you pass to `Send(...)` exists in `SetOutputDefinitions`.
- Every inbound `Contact` you act on exists in `SetInputDefinitions`.
- Payloads are read defensively from `Payload?["data"]`.
- `AgentId` stays `Guid.NewGuid()` in committed code; a static id is a production-only, deploy-time change (avoids collisions between developers' checkouts).
- You `Hub.Default.Unsubscribe(this)` on shutdown.
- Broker address / id / key / contacts live in config (env or appsettings), not hardcoded, for real deployments.
- You did not modify anything under `Rce2/`.
