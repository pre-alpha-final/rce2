# AGENTS.md

Guidance for AI coding agents working in this repository. (For the *runtime* concept of "agents" in RCE2 — devices/programs that talk to the broker — see the Architecture section below; this file is about you, the coding assistant.)

## What this project is

**RCE2 (Remote Controlled Electronics v2)** is a messaging system for connecting and controlling any device that can make outgoing HTTP requests (PCs, phones, Raspberry Pi/Arduino, browsers, even Minecraft computers).

It has two parts:

- **`broker/`** — the central message broker. An ASP.NET Core 6 hosted app: a Blazor WebAssembly **Client** (web UI, PWA-enabled) + an ASP.NET Core **Server** (REST API), with a shared model project. This is the only component that sees the whole network.
- **`agents/`** — a large collection of example agents in many languages (C#, JS, TypeScript/React, Lua, PowerShell, Minecraft Lua, etc.). Each agent is an independent program; agents never see each other.

## Core architecture (read before changing protocol code)

- Agents are **input/output based** and isolated. They don't know about each other. The broker holds **bindings** (out-contact of agent A → in-contact of agent B) and routes messages accordingly — conceptually like Unreal Engine blueprint wiring.
- An agent needs **only outgoing HTTP**: `GET /api/agent/{agentId}` to long-poll its message feed, `POST /api/agent/{agentId}` to emit outputs. That's the entire agent-side protocol. This works behind NAT/firewalls and needs no client library.
- **`whois` handshake**: when the broker doesn't know an agent, it queues a `whois` message. The agent replies by POSTing its metadata (`id`, `name`, `ins`, `outs`). `ins`/`outs` are `{ contactName: type }` maps.
- **Message shape** (`Broker.Shared.Model.Rce2Message`): `{ Type, Contact, Payload }`. `Payload` is a JSON token, conventionally `{ "data": <value> }`. Types are the string constants in `Broker.Shared.Model.Rce2Types` (`void`, `string`, `string-list`, `number`, `number-list`, `bool`, `bool-list`, `custom`, plus the infrastructure `whois`).
- **Auth (per-agent)**: per-agent key sent as an HTTP Basic `Authorization` header containing the base64-encoded raw key. Broker-side enforcement is driven by the `_agentKeys` map in `AgentKeyService`: an agent with a registered key **must** present the matching key, while an agent with no entry may connect unauthenticated. The map is persisted to `agentKeys.txt` and managed via `GET`/`POST /api/broker/agentKeys` (a full read / full replace of the `{ agentId: key }` map) and the "Agent keys" button in the broker UI. The broker UI/management API (`/api/broker`) is `[Authorize]` and uses `BrokerKey` from `appsettings.json`.
- Key broker endpoints live in [`AgentController.cs`](broker/Broker/Server/Controllers/AgentController.cs) (agent feed + output routing) and [`BrokerController.cs`](broker/Broker/Server/Controllers/BrokerController.cs) (bindings, simulate in/out, broker UI feed). Services are wired in [`Program.cs`](broker/Broker/Server/Program.cs).

## Build & run

**Broker** (requires the .NET 6 SDK; uses Visual Studio / `dotnet`):

```sh
cd broker
dotnet build Broker.sln
dotnet run --project Broker/Server
```

Dev URLs: `https://localhost:7113` (and `http://localhost:5113`). The Blazor client is served by the server (`MapFallbackToFile("index.html")`), so run only the Server project. Example agents default to `https://localhost:7113` as the broker address.

**Agents** — vary by language; each directory is self-contained:
- C# agents: `dotnet build` / `dotnet run` in the agent's folder.
- `ReactAgent/`: `npm install` then `npm run dev` (Vite).
- `JSAgent/`: open `index.html` (or serve statically).
- `LuaAgent/`, `PowershellAgent/`: run the script directly (`PSAgent.ps1`, `lua_agent.lua`).

## Writing or modifying agents

- A new agent only needs to: long-poll `GET /api/agent/{guid}`, answer `whois` with its `ins`/`outs`, and `POST` outputs. Use any existing agent in `agents/` as a template for the target language.
- **Do not edit anything inside an agent's `Rce2/` folder.** It is vendored library code (the `Rce2Service` fluent builder + infra) copied in as-is. Integrate via host code (`Program.cs`, hosted services, config), not by changing library internals. See [`RCE2_INTEGRATION_GUIDE_FOR_AGENTS.md`](agents/_CSharpBuilderBoilerplate/CSharpBuilderBoilerplate/RCE2_INTEGRATION_GUIDE_FOR_AGENTS.md) and the [`_CSharpBuilderBoilerplate`](agents/_CSharpBuilderBoilerplate) project for the canonical C# template.
- C# agent setup uses the `Rce2Service` builder chain: `SetBrokerAddress` → `SetAgentId` → `SetAgentKey` → `SetAgentName` → `SetInputDefinitions` → `SetOutputDefinitions` → `Init()`. Subscribe to `PubSub.Hub.Default` for incoming `Rce2Message` and route by `Contact`.
- Keep `Send(contact, ...)` contacts inside the declared `outs`; handle only contacts present in `ins`. In/out contact names are independent maps and need not match.
- **Keep `AgentId` as `Guid.NewGuid()` in committed code.** This is a public repo — a hardcoded static id would make two developers' unmodified checkouts collide on the same agent on the broker. Static ids are a production-only change applied at deploy time.

## Conventions & gotchas

- Platform here is **Windows** with PowerShell; paths in this repo use Windows separators. `.sln`/`.csproj` files target VS 2022.
- C# projects use `Newtonsoft.Json` (not System.Text.Json), nullable + implicit usings enabled, and the `PubSub` package for the in-process message hub.
- Build artifacts (`bin/`, `obj/`, `.vs/`, `node_modules/`) are committed in some agent folders historically — **ignore them**; never treat files under those paths as source. Make changes only to source files.

## Where things live

| Path | What |
|------|------|
| `broker/Broker/Server/` | REST API, controllers, services, feed/binding repositories |
| `agents/_CSharpBuilderBoilerplate/` | Canonical C# agent template + integration guide |
| `agents/<Name>Agent/` | Example agents across languages/platforms |
| `docs/` | Diagrams and screenshots used by the README |
| `README.MD` | Project overview, concept, examples, security rationale |
