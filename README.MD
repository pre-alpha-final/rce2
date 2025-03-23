# Rce2 - Remote Controlled Electronics v2
An evolution over the original Rce design.
<p align="center"><img src="/docs/main.jpg" width="600"/></p>

# In brief
A solution to connect and control any device supporting HTTP connections. PC, laptop, mobile, JS enabled refrigerator or a raspberry pi/arduino driving anything.

# Not in brief
The design goal is to enable creation of simple software driving various devices. Or more precisely providing a messaging layer that allows said software to remain simple.
"Agents" as they are known in the system need only to support outgoing HTTP requests. One URL - GET it (long polling) to receive your message feed, POST it to send out your outputs, that is it. 
The communication is handled via a message broker that agents use to exchange messages between each other.  
Protocol-wise agents are completely separated (they don't see each other). So how do they communicate? Each agent has a list of inputs and a list of outputs. The broker creates bindings between them. If you've seen Unreal Engine design flow, that is basically the same concept. In other words, agents, from their point of view, send their outputs into the void and the broker, based on the bindings, makes sure that the messages end up in correct message feeds.

# Feature summary
- Message broker
	- interface allowing easy system setup
	- accessible via web interface - from anywhere, "no installation"
	- regarding the quotes on "no installation" - broker is pwa enabled so it can be dropped as an app on e.g. mobile - no more mobile browser hassle
	- allows simulating agents' inputs and outputs
- Agents
	- quick and easy to write - a simple agent can be created in under 5 minutes, it takes longer to figure out what it is supposed to be doing than to actually code it
	- can be created on any hardware in any language as long as HTTP connections are supported
	- need only outgoing HTTP requests - will work behind firewall, NAT etc.
	- separated from the rest of the system - input/output based
	- no more library - original rce had a library to support communication, this is no longer needed, just GET your message feed and POST your own messages

# Examples
## Fibonacci sequence
A self reference example - bind the agent to itself, simulate one output and the agent will start feeding itself.
<p align="center"><img src="/docs/fibonacci.jpg" width="600"/></p>

## Distributed processing
Two processors getting and processing data from a single dispatcher.
<p align="center"><img src="/docs/processing.jpg" width="600"/></p>

## Any "device"
When I say any "device" that supports http connections, I do mean it.
<p align="center"><img src="/docs/minecraft_reality.gif" width="600"/></p>

# Security considerations
The concept of an "agent" came from an early draft, where agent and broker responsibilities were, sort of, reversed. The broker was stupid, just a message proxy, and the whole binding responsibility was with the agents. The network was shaped based on the agents' agendas (hence the name). This was obviously a security nightmare, as a single compromised agent compromised the entire network. In a system where devices can be God knows where this was not acceptable.
In current implementation agents have no knowledge of the rest of the network. They get messages delivered to their message feed and send their own messages to the ether. Only on broker level you see the entire network and agent bindings.
Additionally there is a key scheme available for the agents, where every agent gets a key assigned and is required to use it for all communication. This way if an agent is compromised it doesn't affect other agents on the network.

License
----
MIT