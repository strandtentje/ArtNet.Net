namespace Artnet.Models;
/// <summary>
/// To accomodate and denote receiving 2-byte unsigned ints without immediately
/// casting them. We can do this because in the ArtNet protocol, often we can get
/// by checking one byte without immediately casting or making a new ushort.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BigEndianUShort
{
    /// <summary>
    /// Big part of the 2-byte int which comes first in the network protocol
    /// </summary>
    public byte MSB;
    /// <summary>
    /// Small part of the 2-byte int which comes last in the network protocol
    /// </summary>
    public byte LSB;
    /// <summary>
    /// Turn the current value into a regular ushort suitable for use on the current platform.
    /// </summary>
    public ushort Value => (ushort)(MSB << 8 | LSB);

    public static BigEndianUShort FromUShort(ushort val)
    {
        byte msb = (byte)((val >> 8) & 0b1111_1111);
        byte lsb = (byte)(val & 0b1111_1111);
        return new BigEndianUShort()
        {
            MSB = msb,
            LSB = lsb
        };
    }
}