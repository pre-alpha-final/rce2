# TabletUIAgent — the remote-control surface agent

A guide to the one agent in this repo that exists to **drive other agents**. Read the
repository-root [`AGENTS.md`](../../AGENTS.md) and the C# template guide
([`RCE2_INTEGRATION_GUIDE_FOR_AGENTS.md`](../_CSharpBuilderBoilerplate/CSharpBuilderBoilerplate/RCE2_INTEGRATION_GUIDE_FOR_AGENTS.md))
first — this document only covers what makes TabletUIAgent different.

## What it is

TabletUIAgent is a **Blazor WebAssembly PWA** (`net8.0`) — a touch-friendly control
panel you open in a browser and install onto a tablet or phone home screen. It renders
a grid of cards; tapping one opens a "screen" of big clickable buttons; tapping a button
emits an RCE2 output. That's the whole product: a remote with a screen.

It is a normal RCE2 agent and uses the same vendored
[`Rce2/`](TabletUIAgent/Rce2) library as every other C# agent — long-polls
`GET /api/agent/{id}`, answers `whois`, and POSTs outputs via `Rce2Service.Send`. The
WASM runtime makes the calls from the browser, but the wire protocol is identical to a
console agent. **Do not edit anything under `Rce2/`** (same rule as everywhere else).

## Why it's conceptually different

Every other agent is isolated *and* self-contained: it does its own job (press keys,
sync files, watch a page) and its in/out contacts only make sense in terms of that job.
TabletUIAgent is **mechanically just as isolated** — it has no knowledge of any other
agent, sees no one, and only emits messages into the broker — but its contacts are
**deliberately shaped to match specific partner agents**.

The coupling is intentional and lives entirely in the **broker bindings**, not in code:

```
TabletUIAgent (out)            broker bindings            partner agent (in)
  yt-back-10  ─┐
  yt-back     ─┴───────────────────────────────────────►  YtAgent "back"   (→ taps J)
  yt-pause-resume ───────────────────────────────────────►  YtAgent "pause"  (→ taps K)
  yt-forward   ─┐
  yt-forward-10 ┴───────────────────────────────────────►  YtAgent "forward"(→ taps L)
```

So TabletUIAgent is an isolated agent that is **usability-coupled** to a small set of
partners. Its buttons are useless until someone wires its outputs to those partners'
inputs in the broker UI; once wired, the tablet becomes a physical-feeling remote for
them. Contact names need not match across a binding — the broker reconciles them — which
is why the tablet can expose five distinct buttons (`yt-back`, `yt-back-10`, …) that fan
into YtAgent's three inputs, carrying a repeat count in the `number` payload (1 vs 10).

## The YouTube example, end to end

1. **Screen** — [`Pages/Youtube.razor`](TabletUIAgent/Pages/Youtube.razor) renders a
   media-controls bar (`« ‹ ⏯ › »`). Each button calls
   `Rce2Service.Send("yt-…", n)`.
2. **Outputs** — those five contacts are declared in
   [`Program.cs`](TabletUIAgent/Program.cs) `SetOutputDefinitions` (four `number`, one
   `void` for pause/resume).
3. **Partner** — [`YtAgent`](../YtAgent/YtAgent/Program.cs) is a Windows console agent
   whose inputs `back` / `pause` / `forward` translate to `J` / `K` / `L` keystrokes
   (YouTube's seek/pause hotkeys) via `WinApi`. `number` payloads drive
   `PressNTimes`, so "skip 10" presses `L` ten times.
4. **Binding** — in the broker UI you wire each tablet output to the matching YtAgent
   input. Nothing in either codebase references the other; the broker is the only place
   the two meet.

Net effect: tap the tablet, the broker routes the message, the PC running YtAgent seeks
the focused YouTube video.

## How the UI is structured

- [`Pages/Index.razor`](TabletUIAgent/Pages/Index.razor) — the single-page shell. Holds
  a `SubPage` string; renders `<MainMenu>` when empty, otherwise the selected screen.
  The `<<<` back link clears `SubPage`.
- [`Pages/MainMenu.razor`](TabletUIAgent/Pages/MainMenu.razor) — the card grid. Each
  card raises `OnClickCard` with a screen id (`"youtube"`, `"sendit"`, …). The
  placeholder cards are intentional empty slots for future control surfaces.
- **Screen components** — one per partner/feature. `Youtube.razor` is send-only.
  [`Pages/SendIt.razor`](TabletUIAgent/Pages/SendIt.razor) is the bidirectional debug
  screen: it `Send`s `sendit-output` and subscribes on `Hub.Default` to display inbound
  `sendit-input` (the agent's one declared input). Use it to probe a binding by hand.

A screen that needs to *receive* (like SendIt) subscribes to `Hub.Default` in its
constructor and **must** `Hub.Default.Unsubscribe(this)` in `Dispose` (`@implements
IDisposable`) — otherwise stale subscriptions accumulate as you navigate between screens.

## Adding a new control surface

1. Create `Pages/MyThing.razor`; inject `Rce2Service`; add buttons that call
   `Send("my-contact", payload)`.
2. Declare every `my-contact` in `Program.cs` `SetOutputDefinitions` (and any inputs in
   `SetInputDefinitions`). The `whois` reply is built from these dictionaries.
3. Add a card to `MainMenu.razor` whose `OnClickCardHandler` passes a new screen id.
4. Add an `else if (SubPage == "mything")` branch in `Index.razor`.
5. Run the partner agent, then **bind** the new outputs to its inputs in the broker UI.

Keep `Send` contacts inside the declared `outs` and only handle inbound contacts present
in `ins` — same discipline as any agent.

## Build, run, deploy

```sh
cd agents/TabletUIAgent
dotnet run --project TabletUIAgent        # dev server; open the printed URL
```

The broker address is hardcoded to `https://localhost:7113` in
[`Program.cs`](TabletUIAgent/Program.cs) (`SetBrokerAddress`); point it at your broker
for real use. Because the browser calls the broker directly, the broker must be
reachable from the tablet and CORS-permissive for that origin.

For a real device, publish the static PWA and host it (publish profiles for FTP /
Web Deploy / Zip Deploy ship under `Properties/PublishProfiles/`), then "Add to Home
Screen" — `manifest.json` sets `display: standalone` so it launches chromeless like a
native remote.

## Gotchas specific to this agent

- **`SetAgentId(Guid.NewGuid())` stays in committed code** (public-repo collision rule —
  see `AGENTS.md`). Pin a static id only at deploy time.
- **A fresh load is a fresh agent.** Each browser session generates a new `Guid`, so it
  registers as a *new* agent on the broker and your existing bindings won't apply until
  you re-bind (or pin a static id for the deployed instance).
- **Buttons do nothing without bindings.** A working tablet, a running partner, and the
  broker wiring are three separate prerequisites; if a tap has no effect, check the
  binding first — the agent itself can't tell whether anyone is listening.
