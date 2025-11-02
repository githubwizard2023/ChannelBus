using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Simple, safe, predictable ChannelBus:
/// - Everything (Send/Register/Unregister) is enqueued first
/// - One processing loop does operations in order
/// - Queue limit prevents runaway spam
/// </summary>
public static class ChannelBus
{
    private static readonly Dictionary<int, List<Action<string>>> channelToListeners =
        new Dictionary<int, List<Action<string>>>();

    private enum OperationType { SendMessage, AddListener, RemoveListener }

    private struct Operation
    {
        public OperationType Type;
        public int Channel;
        public string Message;
        public Action<string> Listener;
    }

    // Safety limit to prevent memory overload / infinite loops
    private const int MaxQueuedOperations = 1000;

    private static readonly Queue<Operation> operationQueue =
        new Queue<Operation>(16);

    private static bool isProcessingOperations = false;


    /// <summary> Register a listener to a channel </summary>
    public static void Listen(int channel, Action<string> listener)
    {
        if (listener == null) return;

        EnqueueOperation(new Operation
        {
            Type = OperationType.AddListener,
            Channel = channel,
            Listener = listener
        });
    }

    /// <summary> Remove a listener from a channel </summary>
    public static void ListenRemove(int channel, Action<string> listener)
    {
        if (listener == null) return;

        EnqueueOperation(new Operation
        {
            Type = OperationType.RemoveListener,
            Channel = channel,
            Listener = listener
        });
    }

    /// <summary> Send a message on a channel </summary>
    public static void Send(int channel, string message = null)
    {
        if (string.IsNullOrEmpty(message)) return;

        EnqueueOperation(new Operation
        {
            Type = OperationType.SendMessage,
            Channel = channel,
            Message = message
        });
    }


    /// <summary> Central enqueue handler with overflow protection </summary>
    private static void EnqueueOperation(Operation operation)
    {
        if (operationQueue.Count >= MaxQueuedOperations)
        {
            Debug.WriteLine($"[ChannelBus] Queue FULL — operation dropped! " +
                            $"(limit {MaxQueuedOperations})");
            return;
        }

        operationQueue.Enqueue(operation);
        ProcessOperationsIfIdle();
    }


    /// <summary> Starts processing only if not already active </summary>
    private static void ProcessOperationsIfIdle()
    {
        if (isProcessingOperations) return;
        isProcessingOperations = true;
        ProcessOperationQueue();
        isProcessingOperations = false;
    }


    /// <summary>
    /// Processes operations in FIFO order
    /// Any API calls inside callbacks only enqueue new operations → safe
    /// </summary>
    private static void ProcessOperationQueue()
    {
        while (operationQueue.Count > 0)
        {
            var currentOperation = operationQueue.Dequeue();

            switch (currentOperation.Type)
            {
                case OperationType.AddListener:
                    {
                        if (!channelToListeners.TryGetValue(currentOperation.Channel,
                            out var listeners))
                        {
                            listeners = new List<Action<string>>();
                            channelToListeners[currentOperation.Channel] = listeners;
                        }

                        if (!listeners.Contains(currentOperation.Listener))
                            listeners.Add(currentOperation.Listener);

                        break;
                    }

                case OperationType.RemoveListener:
                    {
                        if (!channelToListeners.TryGetValue(currentOperation.Channel,
                            out var listeners)) break;

                        listeners.Remove(currentOperation.Listener);

                        if (listeners.Count == 0)
                            channelToListeners.Remove(currentOperation.Channel);

                        break;
                    }

                case OperationType.SendMessage:
                    {
                        if (!channelToListeners.TryGetValue(currentOperation.Channel,
                            out var listeners) || listeners.Count == 0)
                            break;

                        // Snapshot so iteration won't break if listeners change
                        var listenersCopy = new List<Action<string>>(listeners);

                        foreach (var listener in listenersCopy)
                            listener?.Invoke(currentOperation.Message);

                        break;
                    }
            }
        }
    }
}