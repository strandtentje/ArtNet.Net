namespace Artnet.Models;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct LittleEndianUniverse
{
    public byte LsbOctet;
    public byte MsbSeptet;
    public override int GetHashCode() => LsbOctet << 8 | MsbSeptet;
}