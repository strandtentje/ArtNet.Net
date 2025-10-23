namespace Artnet.Models;

/// <summary>
/// The fixed-length component of the payload of the DMX-opcode ArtNet datagram
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct DmxPreamble
{
    /// <summary>
    /// When greater than 0, this field is used to avoid out-of-order processing of DMX payloads.
    /// When 0, the DMX payload will be ingested unconditionally.
    /// </summary>
    public byte SequenceNumber;
    /// <summary>
    /// DMX port that caused this payload to be transmitted. Multiple ports transmitting to
    /// the same universe, will not be mixed by this implementation, rather, the latest transmission
    /// is the current one.
    /// </summary>
    public byte OriginatingPort;
    /// <summary>
    /// 15-bit universe number
    /// </summary>
    public LittleEndianUniverse Universe;
    /// <summary>
    /// Size (in bytes) announcement of the DMX payload that immediately follows the datagram.
    /// </summary>
    public BigEndianUShort DmxLength;
}