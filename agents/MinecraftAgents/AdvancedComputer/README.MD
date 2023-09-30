# Rce2 - Minecraft Advanced Computer Agent
Agent for minecraft's advanced computer.

# Requirements
For versions these are the ones I ran it on. Could work with older.
- Minecraft 1.20.1
- Mods
	- cc-tweaked - cc-tweaked-1.20.1-forge-1.108.1.jar

# Tech info
Only difference from standard rce2 usage is that there are actually 2 scripts running. Since lua is single threaded the long-pooling message loop would block sending out commands (like lever state changes). Not an issue though, since both scripts share the same id, so the system doesn't even know they run in two instances.
Why not two background processes on single advanced computer, or why not cc-tweaked's parallel? Because they don't work. Context switching breaks currently running http requests.

# Example
Connected to PI's agent to control lighs via a relay.
<p align="center"><img src="/docs/minecraft_reality.gif" width="600"/></p>
