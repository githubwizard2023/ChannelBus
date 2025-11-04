using UnityEngine;

/// <summary>
/// Central registry of common command keywords sent through ChannelBus.
///
/// Why?
/// - Prevents typos when sending or comparing text commands
/// - Keeps commands consistent across the entire project
/// - Provides a single discoverable place to add new global commands
///
/// How to use:
/// - When sending a command:
///     ChannelBus.Send(Channels.COMMUNICATION_CHANNEL, Commands.TURN_ON);
///
/// - When reacting to commands:
///     void Listen(string message)
///     {
///         switch (message)
///         {
///             case Commands.TURN_ON:
///                 // Perform action here
///                 break;
///             case Commands.TURN_OFF:
///                 // Perform action here
///                 break;
///         }
///     }
///
/// Tips:
/// - Use verbs in ALL CAPS for clarity and consistency
/// - Extend this class with additional game actions as needed
/// - This acts like a shared "command dictionary" for the whole project
///
/// Think of this class as:
///   "The Universal Remote Control Buttons" for your ChannelBus 😉
///
/// Implementation notes:
/// - Strings must remain `public const` to be usable everywhere
/// - private fields aren’t accessible — so keep commands public
/// </summary>
public static class Commands
{
    // Public constants so other scripts can reference them
    public const string TURN_ON = "TURN_ON";   // Start / enable
    public const string TURN_OFF = "TURN_OFF";  // Stop / disable
    public const string TOGGLE = "TOGGLE";    // Flip current state
}
