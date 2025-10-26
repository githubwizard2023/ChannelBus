using System;
using System.Collections.Generic;

/// <summary>
/// A high-efficiency global communication system inspired by llRegionSay.
/// It retains the original safety feature of checking for duplicate subscriptions 
/// while using a GC-free iteration approach in the Send method.
/// </summary>
public static class ChannelBus
{
    // The core dictionary mapping channel numbers to listener functions.
    private static readonly Dictionary<int, List<Action<string>>> channelListeners =
        new Dictionary<int, List<Action<string>>>();

    // Optimization: A pre-allocated, static list used ONLY for safe iteration in Send.
    // This eliminates the GC pressure from creating a new array/list on every Send call.
    private static readonly List<Action<string>> tempListeners = new List<Action<string>>(10);

    /// <summary>
    /// Subscribes a callback function to a specific numeric channel.
    /// The callback will be executed whenever a message is sent to that channel.
    /// Includes the original check to prevent duplicate subscriptions.
    /// </summary>
    public static void Listen(int channel, Action<string> callback)
    {
        if (callback == null) return;

        // Retrieve the listener list for the channel, or create it if it doesn't exist.
        if (!channelListeners.TryGetValue(channel, out List<Action<string>> listeners))
        {
            listeners = new List<Action<string>>(1);
            channelListeners[channel] = listeners;
        }

        // ORIGINAL SAFETY CHECK: Only add the callback if it is not already in the list.
        // NOTE: This adds an O(n) search overhead, but prevents duplicates.
        if (!listeners.Contains(callback))
        {
            listeners.Add(callback);
        }
    }

    /// <summary>
    /// Unsubscribes a callback function from a specific numeric channel.
    /// If no listeners remain on that channel, the channel entry is removed.
    /// </summary>
    public static void ListenRemove(int channel, Action<string> callback)
    {
        if (callback == null) return;

        if (channelListeners.TryGetValue(channel, out List<Action<string>> listeners))
        {
            // List.Remove is O(n), which is standard for unsubscription.
            listeners.Remove(callback);

            if (listeners.Count == 0)
            {
                // Clean up the dictionary to conserve memory.
                channelListeners.Remove(channel);
            }
        }
    }

    /// <summary>
    /// Sends a message to all listeners registered to a specific channel.
    /// This method is optimized to use a pre-allocated static list for iteration 
    /// to eliminate Garbage Collection pressure.
    /// </summary>
    public static void Send(int channel, string message = null)
    {
        if (!channelListeners.TryGetValue(channel, out List<Action<string>> listeners))
        {
            return;
        }

        // 1. Copy the listeners to the pre-allocated temporary list.
        // This is the GC-free replacement for listeners.ToArray() and ensures iteration safety.
        tempListeners.AddRange(listeners);

        // 2. Iterate over the temporary list.
        for (int i = 0; i < tempListeners.Count; i++)
        {
            // Invoke the action (function).
            tempListeners[i]?.Invoke(message);
        }

        // 3. IMPORTANT: Clear the temporary list immediately for reuse on the next Send call.
        tempListeners.Clear();
    }
}