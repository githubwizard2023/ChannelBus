using System;
using System.Collections.Generic;
/// <summary>
/// Safe ChannelBus with admin runtime controls and queue monitoring:
/// - All requests are enqueued → processed safely in-order
/// - Prevents recursion crashes or listener changes during dispatch
/// - Live queue metrics: Enqueued, Processed, Dropped, Peak usage
/// - Admin channel (-23646) commands for debugging:
///     SHOW_REQUESTS, HIDE_REQUESTS, STOP_ALL,
///     SHOW_METRICS, RESET_METRICS
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

    private const int ADMIN_CHANNEL = -23646;
    private const int MaxQueuedOperations = 1000;

    private static readonly Queue<Operation> operationQueue = new Queue<Operation>(16);

    private static bool isProcessingOperations = false;

    private static bool logRequests = false;
    private static bool stopAll = false;

    // Metrics tracking
    private static int totalEnqueued = 0;
    private static int totalProcessed = 0;
    private static int totalDropped = 0;
    private static int peakQueueDepth = 0;

#if UNITY_EDITOR
    private static void Log(string message)
    {
        int currentCount = operationQueue.Count;
        UnityEngine.Debug.Log($"[ChannelBus] {message}  (queue: {currentCount}/{MaxQueuedOperations})");
    }
#endif

    static ChannelBus()
    {
        Listen(ADMIN_CHANNEL, AdminCommandHandler);
    }

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

    private static void EnqueueOperation(Operation operation)
    {
        if (operationQueue.Count >= MaxQueuedOperations)
        {
            totalDropped++;
#if UNITY_EDITOR
            Log($"Queue FULL — Operation Dropped! limit={MaxQueuedOperations}");
#endif
            return;
        }

        // Enqueue first so the count reflects the true current size
        operationQueue.Enqueue(operation);
        totalEnqueued++;
        peakQueueDepth = Math.Max(peakQueueDepth, operationQueue.Count);

#if UNITY_EDITOR
        if (logRequests)
        {
            switch (operation.Type)
            {
                case OperationType.AddListener:
                    Log($"ENQUEUED AddListener  Channel = {operation.Channel}");
                    break;

                case OperationType.RemoveListener:
                    Log($"ENQUEUED RemoveListener  Channel = {operation.Channel}");
                    break;

                case OperationType.SendMessage:
                    string shortMessage = operation.Message;
                    if (shortMessage != null && shortMessage.Length > 64)
                        shortMessage = shortMessage.Substring(0, 64) + "…";
                    Log($"ENQUEUED SendMessage  Channel = {operation.Channel}, Message = \"{shortMessage}\"");
                    break;
            }
        }
#endif

        ProcessOperationsIfIdle();
    }

    private static void ProcessOperationsIfIdle()
    {
        if (isProcessingOperations) return;
        isProcessingOperations = true;
        ProcessOperationQueue();
        isProcessingOperations = false;
    }

    private static void ProcessOperationQueue()
    {
        while (operationQueue.Count > 0)
        {
            // Optional: show queue size as we drain (Editor + SHOW_REQUESTS only)
#if UNITY_EDITOR
            if (logRequests)
                Log("PROCESSING next operation…");
#endif
            var operation = operationQueue.Dequeue();
            totalProcessed++;

            bool isAdminMsg =
                operation.Type == OperationType.SendMessage &&
                operation.Channel == ADMIN_CHANNEL;

            if (stopAll && !isAdminMsg)
            {
#if UNITY_EDITOR
                if (logRequests)
                    Log($"STOPPED Operation  Type = {operation.Type}, Channel = {operation.Channel}");
#endif
                continue;
            }

            switch (operation.Type)
            {
                case OperationType.AddListener:
                    {
                        if (!channelToListeners.TryGetValue(operation.Channel, out var listeners))
                        {
                            listeners = new List<Action<string>>();
                            channelToListeners[operation.Channel] = listeners;
                        }

                        if (!listeners.Contains(operation.Listener))
                            listeners.Add(operation.Listener);
                        break;
                    }

                case OperationType.RemoveListener:
                    {
                        if (!channelToListeners.TryGetValue(operation.Channel, out var listeners))
                            break;

                        listeners.Remove(operation.Listener);

                        if (listeners.Count == 0)
                            channelToListeners.Remove(operation.Channel);
                        break;
                    }

                case OperationType.SendMessage:
                    {
                        if (!channelToListeners.TryGetValue(operation.Channel, out var listeners) ||
                            listeners.Count == 0)
                            break;

                        var listenerSnapshot = new List<Action<string>>(listeners);
                        foreach (var listener in listenerSnapshot)
                            listener?.Invoke(operation.Message);
                        break;
                    }
            }

#if UNITY_EDITOR
            if (logRequests)
                Log("PROCESSED operation");
#endif
        }
    }

    private static void AdminCommandHandler(string message)
    {
        string command = (message ?? "")
            .Trim()
            .Replace(' ', '_')
            .ToUpperInvariant();

        switch (command)
        {
            case "SHOW_REQUESTS":
                logRequests = true;
#if UNITY_EDITOR
                Log("ADMIN SHOW_REQUESTS Enabled");
#endif
                break;

            case "HIDE_REQUESTS":
                logRequests = false;
#if UNITY_EDITOR
                Log("ADMIN SHOW_REQUESTS Disabled");
#endif
                break;

            case "STOP_ALL":
                stopAll = true;
#if UNITY_EDITOR
                Log("ADMIN STOP_ALL Enabled — Non-admin ops blocked");
#endif
                break;

            case "SHOW_METRICS":
#if UNITY_EDITOR
                Log($"ADMIN METRICS → Enqueued={totalEnqueued}, Processed={totalProcessed}, Dropped={totalDropped}, PeakQueue={peakQueueDepth}");
#endif
                break;

            case "RESET_METRICS":
                totalEnqueued = totalProcessed = totalDropped = peakQueueDepth = 0;
#if UNITY_EDITOR
                Log("ADMIN METRICS Reset");
#endif
                break;

            default:
#if UNITY_EDITOR
                Log($"ADMIN Unknown Command \"{command}\"");
#endif
                break;
        }
    }
}