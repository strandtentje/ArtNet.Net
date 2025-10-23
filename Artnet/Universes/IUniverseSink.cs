namespace Artnet.Universes;
/// <summary>
/// Interface for sinking DMX data updates
/// </summary>
public interface IUniverseSink
{
    /// <summary>
    /// Provided the preamble and announced DMX length, use the Payload data to update
    /// the universe if it was subscribed to. Data will be ignored if it's too big, too
    /// small or not among subscriptions.
    /// </summary>
    /// <param name="dmxLength">Size of DMX data as announced by the preamble</param>
    /// <param name="preamble">The Preamble containing the Universe Addressing & Ports</param>
    /// <param name="payload">DMX Data payload that came after the preamble</param>
    void Update(ushort dmxLength, DmxPreamble preamble, ReadOnlySpan<byte> payload);
}