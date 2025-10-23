namespace Artnet.Universes;
/// <summary>
/// Stores known and subscribed universes, and handles data updates for subscribed universes.
/// </summary>
public class DmxUniverseRepository : IUniverseSource, IUniverseSink
{
    /// <summary>
    /// Timestamp source
    /// </summary>
    private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();
    /// <summary>
    /// All detected universes will be kept here, regardless of subscription. For each detected
    /// universe, the detection timestamp should be registered as a Value.
    /// </summary>
    private readonly SortedList<LittleEndianUniverse, long> DetectedUniverses = new();
    /// <summary>
    /// Universes we should keep the DMX data of, will be kept here.
    /// </summary>
    private readonly SortedList<LittleEndianUniverse, SequenceNumberedUniverse> SubscribedUniverses = new();
    /// <summary>
    /// All universe numbers regardless of subscription are enumerable here.
    /// </summary>
    public IEnumerable<LittleEndianUniverse> KnownUniverses => DetectedUniverses.Keys;
    /// <summary>
    /// Get the Data for a subscribed universe; will throw if the universe wasn't subscribed to.
    /// The Data property of the return class will be up to date so long as the Universe's been
    /// subscribed to; no need to call this twice.
    /// </summary>
    /// <param name="key">Universe number</param>
    /// <returns>Universe Data</returns>
    public IUniverse GetSubscribedUniverse(LittleEndianUniverse key) => SubscribedUniverses[key];
    /// <summary>
    /// Register a universe number as subscribed so we can keep it up to date;
    /// This doesn't need to be a KnownUniverse, but it's recommended to do incidental
    /// runtime checks if your Universe has been detected yet, and if not, maybe do a Warning 
    /// </summary>
    /// <param name="universe">Universe number to subscribe to</param>
    public void SetUniverseSubscribed(LittleEndianUniverse universe) =>
        SubscribedUniverses.Add(universe, new SequenceNumberedUniverse());
    /// <summary>
    /// Size of DMX Preamble structure 
    /// </summary>
    private static readonly int DmxPreambleSize = Marshal.SizeOf<DmxPreamble>();
    /// <summary>
    /// Provided the preamble and announced DMX length, use the Payload data to update
    /// the universe if it was subscribed to. Data will be ignored if it's too big, too
    /// small or not among subscriptions.
    /// </summary>
    /// <param name="dmxLength">Size of DMX data as announced by the preamble</param>
    /// <param name="preamble">The Preamble containing the Universe Addressing & Ports</param>
    /// <param name="payload">DMX Data payload that came after the preamble</param>
    public void Update(ushort dmxLength, DmxPreamble preamble, ReadOnlySpan<byte> payload)
    {
        if (dmxLength is < 2 or > 512 || dmxLength % 2 == 1)
            return;
        DetectedUniverses[preamble.Universe] = Stopwatch.ElapsedMilliseconds;
        if (!SubscribedUniverses.TryGetValue(preamble.Universe, out var universe))
            return;
        if (!universe.PortSequenceNumbers.TryGetValue(
                preamble.OriginatingPort, out var previousSequenceNumber)
            || (preamble.SequenceNumber - previousSequenceNumber) is > 0 or < -253)
            payload.Slice(DmxPreambleSize, dmxLength).CopyTo(universe.DmxData);
        universe.PortSequenceNumbers[preamble.OriginatingPort] = preamble.SequenceNumber;
    }
}