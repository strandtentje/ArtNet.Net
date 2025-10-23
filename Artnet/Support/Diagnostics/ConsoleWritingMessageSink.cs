// ReSharper disable MemberCanBePrivate.Global
namespace Artnet.Support;

/// <summary>
/// Relatively non-blocking implementation of IErrorSink that queues up errors
/// and pumps them into console on another thread so as to avoid runtime blockinng.
/// </summary>
public class ConsoleWritingMessageSink : IDisposable, IErrorSink, IMessageSink
{
    /// <summary>
    /// Gets a value indicating whether the message sink is currently pumping 
    /// </summary>
    public bool IsRunning { get; private set; }

    private readonly object QueueActivityLock = new();
    private readonly ConcurrentQueue<(char prefix, string message)> MessageQueue = new();
    private readonly EventWaitHandle MessageAvailability = new(false, EventResetMode.AutoReset);

    /// <summary>
    /// Report error text.
    /// </summary>
    /// <param name="description">Error text</param>
    public void IngestError(string description)
    {
        if (!IsRunning) return;
        lock (QueueActivityLock)
        {
            MessageQueue.Enqueue(('E', description));
            MessageAvailability.Set();
        }
    }
    /// <summary>
    /// Report message text
    /// </summary>
    /// <param name="description">Message text</param>
    public void IngestMessage(string description)
    {
        if (!IsRunning) return;
        lock (QueueActivityLock)
        {
            MessageQueue.Enqueue(('O', description));
            MessageAvailability.Set();
        }
    }
    /// <summary>
    /// Start the message pumping
    /// </summary>
    public void Start()
    {
        lock (QueueActivityLock)
        {
            if (IsRunning) return;
            IsRunning = true;
            Task.Run(IngestMessageQueue);
        }
    }

    /// <summary>
    /// Stop the message pumping
    /// </summary>
    public void Stop()
    {
        lock (QueueActivityLock)
        {
            if (!IsRunning) return;
            IsRunning = false;
            MessageQueue.Clear();
            MessageAvailability.Set();
            MessageQueue.Clear();
        }
    }

    /// <summary>
    /// So long as the running flag is on, attempts to pull messages from the queue.
    /// </summary>
    private void IngestMessageQueue()
    {
        while (IsRunning)
        {
            MessageAvailability.WaitOne();
            while (MessageQueue.TryDequeue(out var entry))
                Console.WriteLine($"[{entry.prefix}|{DateTime.Now:yyMMdd_HHmm}] {entry.message}");
        }
    }

    /// <summary>
    /// Create a new started message sink.
    /// </summary>
    /// <returns>Instance</returns>
    public static ConsoleWritingMessageSink StartNew()
    {
        ConsoleWritingMessageSink instance = new();
        instance.Start();
        return instance;
    }

    public void Dispose() => Stop();
}