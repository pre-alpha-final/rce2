# RCE2 Integration Guide for Future AI Agents

This repository contains a small .NET host app plus an `Rce2` folder that is treated as **vendor code**.

## Non-Negotiable Rule

- Do **not** edit anything inside `Rce2/`.
- Assume `Rce2/` is copied in as-is and should remain untouched.
- Integrate by configuring host code (`Program.cs`, hosted services, app settings), not by changing library internals.

## Minimal Integration Steps (Into Another Project)

1. Copy `Rce2/` folder into your project unchanged.
2. Ensure package dependencies exist in your `.csproj`:
   - `Microsoft.Extensions.Hosting`
   - `Microsoft.Extensions.Http`
   - `Newtonsoft.Json`
   - `PubSub`
3. Register services in startup (`Program.cs`):
   - `builder.Services.AddHttpClient();`
   - `builder.Services.AddSingleton<Rce2Service>();`
   - register a hosted service (or equivalent lifecycle component) that initializes and handles messages.
4. During app start, configure and initialize using the `Rce2Service` fluent builder chain:
   - `SetBrokerAddress("https://...")`
     - Base URL of the broker API used for feed polling and outbound posts.
   - `SetAgentId(Guid...)`
     - Unique identity for this agent instance on the broker.
     - Prefer a stable ID in real environments; use `Guid.NewGuid()` only for ephemeral/testing agents.
   - `SetAgentKey("...")`
     - Optional auth key; empty string disables auth header.
   - `SetAgentName("...")`
     - Human-readable name returned in `whois` replies.
   - `SetInputDefinitions(new Dictionary<string, string> { ... })`
     - Declares contacts this agent can receive (`contact -> type`), for example `{ "echo-test", Rce2Types.String }`.
   - `SetOutputDefinitions(new Dictionary<string, string> { ... })`
     - Declares contacts this agent can send (`contact -> type`).
     - Any `Send(contact, payload)` call must use a contact present here.
     - Input and output contact names do not need to match; they are independent maps.
     - This boilerplate uses the same name (`echo-test`) for both only because it is an echo-style example.
   - `Init()`
     - Starts the internal background feed loop. Call this after all values are set.
   - Builder pattern notes:
     - Every `Set...` method mutates the same `Rce2Service` instance and returns it, so calls are chainable.
     - The chain order among `Set...` methods is flexible, but all required values should be set before `Init()`.
     - `SetInputDefinitions` and `SetOutputDefinitions` replace prior dictionaries (they do not merge entries).
5. Subscribe to `Hub.Default` for `Rce2Message` and route by `Contact`.
6. On shutdown, unsubscribe (`Hub.Default.Unsubscribe(this)`).

Focused setup example (builder pattern only):

```csharp
_rce2Service
    .SetBrokerAddress("https://localhost:7113")
    .SetAgentId(Guid.NewGuid())
    .SetAgentKey(string.Empty)
    .SetAgentName("Boilerplate")
    .SetInputDefinitions(new()
    {
        { "echo-test", Rce2Types.String }
    })
    .SetOutputDefinitions(new()
    {
        { "echo-test", Rce2Types.String }
    })
    .Init();
```

## Protocol Behavior (Observed from Library)

- `Init()` starts an internal background loop that continuously calls `GET /api/agent/{agentId}`.
- Each polled inbound message is handled as follows:
  - if `Type == "whois"`, the service auto-responds with agent metadata (`Id`, `Name`, `Ins`, `Outs`)
  - otherwise, the message is published to `Hub.Default` for application handlers
- `Send(contact, payload)` posts to `POST /api/agent/{agentId}`.
- When `AgentKey` is non-empty, requests include a Basic auth header using the base64-encoded raw key string.

## Safe Implementation Pattern for Agents

- Keep configuration outside code when possible (env vars / config file):
  - broker address
  - agent id/name/key
  - declared contacts and types
- Use stable, deterministic `AgentId` in real environments (avoid `Guid.NewGuid()` unless ephemeral behavior is intended).
- Validate payload shape before dereferencing `["data"]`.
- Keep routing aligned with your definitions:
  - incoming `Contact` values you handle should exist in `SetInputDefinitions`
  - contacts you pass to `Send(contact, ...)` should exist in `SetOutputDefinitions`
  - input and output contact names may be different by design

## Boilerplate Example Mapping

Current example registers one contact:

- contact: `echo-test`
- type: `string`
- behavior: when an `echo-test` message arrives, the app waits 1 second and sends the same value back.

Use this as a template to add more contacts and handlers.
