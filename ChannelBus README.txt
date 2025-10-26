ChannelBus
A lightweight loosely coupled communication system for Unity

Overview
ChannelBus is a global communication system inspired by Second Life llRegionSay, a proven method used for over 20 years to let game objects communicate without references or wiring. The system uses numeric channels and string messages, allowing scripts and GameObjects to exchange data freely, similar to how modern distributed systems use channels and protocols.

The core of the system is a static class called ChannelBus, which manages all listeners and message broadcasts. It is simple, powerful, and fully customizable, easy to expand to use generic types or adapt to different needs while keeping the same clean concept.

Features
• Static class ChannelBus for global communication
• Loosely coupled architecture, no references or dependencies
• Numeric channel and string message system
• GC free optimized message sending
• Safe duplicate subscription check
• Works in any genre or project size
• Expandable for generic types or custom protocols
• Perfect for pure C# classes with no need for the Unity Inspector
• Ideal for AI signaling, gameplay triggers, UI, and tool systems

Example Usage

SwitchController.cs
Sends a message when the button is clicked
ChannelBus.Send(345, "TOGGLE");

SmartLightBulb.cs
Listens for messages on the same channel and reacts to them
ChannelBus.Listen(345, OnSwitchCommandReceived);

This setup lets objects communicate directly by channel, not by reference, just like walkie talkies tuned to the same frequency.

How to Integrate

1. Copy the ChannelBus.cs file into your project, preferably under a folder like Scripts/Core.
2. Add listener scripts, for example SmartLightBulb, that subscribe to a channel.
3. Use sender scripts, for example SwitchController, to broadcast messages on the same channel.
4. Run your scene, communication will happen instantly without any references.

Credits
Created by Marius Shama
Inspired by the communication model of Second Life
Enhanced with modern Unity C# practices
