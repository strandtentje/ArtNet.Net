namespace Artnet.Support;

/// <summary>
/// Utility that occasionally dumps the current state of the Universes to the Console via IMessageSink
/// </summary>
/// <param name="messages">Message Sink</param>
/// <param name="universes">Universes to Check</param>
public class RecurringUniverseInspection(IMessageSink messages, IUniverseSource universes) : IDisposable, IPeriodicInspector
{
    private readonly object RunLock = new();
    private Timer? inspectionTimer = null;
    /// <summary>
    /// Start the inspector with the provided interval; when already running, won't do anything.
    /// </summary>
    /// <param name="interval">Interval. At least 1s (1000)</param>
    /// <returns>Itself</returns>
    public IPeriodicInspector Start(int interval)
    {
        lock (RunLock)
        {
            if (inspectionTimer != null || interval == int.MaxValue) return this;
            interval = Math.Max(1000, interval);
            inspectionTimer = new(WriteInspection, null, interval, interval);
        }

        return this;
    }
    /// <summary>
    /// Stop reporting universe state.
    /// </summary>
    public void Stop()
    {
        lock (RunLock)
        {
            if (inspectionTimer == null) return;
            inspectionTimer.Dispose();
            inspectionTimer = null;
        }
    }
    private void WriteInspection(object state)
    {
        var universesKnownUniverses = universes.KnownUniverses.ToArray();
        if (universesKnownUniverses.Length == 0)
        {
            messages.IngestMessage($"No universes active.");
        }
        foreach (LittleEndianUniverse universesKnownUniverse in universesKnownUniverses)
            messages.IngestMessage($"Subuniverse: {universesKnownUniverse.LsbOctet} Net: {universesKnownUniverse.MsbSeptet}");
    }
    public void Dispose() => Stop();
}