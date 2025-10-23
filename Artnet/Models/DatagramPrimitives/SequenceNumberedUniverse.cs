namespace Artnet.Models;
/// <summary>
/// To maintain the latest state of a DMX universe in an ArtNet network.
/// </summary>
public class SequenceNumberedUniverse : IUniverse
{
    /// <summary>
    /// A Port-To-SequenceNumber mapping to aid in
    /// a) Keeping DMX Data Updates in Sequence
    /// b) Discriminating between Data from multiple DMX inputs.
    /// </summary>
    public readonly SortedList<byte, byte> PortSequenceNumbers = new();
    /// <summary>
    /// Most Recent DMX Data for this universe. Guaranteed to be 512 in length.
    /// </summary>
    public readonly byte[] DmxData = new byte[512];
    /// <summary>
    /// Implementation of the IUniverse accessor for exposing to actual DMX Data consumers.
    /// </summary>
    byte[] IUniverse.Data => DmxData;
}