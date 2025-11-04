using UnityEngine;

/// <summary>
/// Central registry for all ChannelBus message channels.
///
/// Why?
/// - Avoids "magic numbers" scattered in the code
/// - Prevents typos when using channel IDs (one source of truth)
/// - Makes channels easy to find, rename, and document
///
/// How to use:
/// - When sending a message:
///     ChannelBus.Send(Channels.COMMUNICATION_CHANNEL, "HELLO");
///
/// - When listening to a channel:
///     ChannelBus.Listen(Channels.COMMUNICATION_CHANNEL, OnMessageReceived);
///
/// - When removing a listener:
///     ChannelBus.ListenRemove(Channels.COMMUNICATION_CHANNEL, OnMessageReceived);
///
/// Tips:
/// - Group channels by purpose (UI, Audio, EnemyAI, Admin…)
/// - Add new channels here instead of writing numbers in scripts
/// - Use descriptive names so everyone understands the intent
///
/// Example channel groups:
///     COMMUNICATION_CHANNEL   = main game messages
///     ADMIN_CHANNEL           = debug / runtime developer commands
///
/// This class is STATIC:
/// - Access from anywhere without creating an instance
/// - Acts like a global dictionary of channel definitions
///
/// Think of this as the official "telephone book" for your ChannelBus 😉
/// </summary>
public static class Channels
{
    public const int COMMUNICATION_CHANNEL = 1001;
    public const int ADMIN_CHANNEL = -23646;
}
