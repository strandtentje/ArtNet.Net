using Artnet.Packets;

namespace Artnet.Models;

/// <summary>
/// Check the Artnet spec for this one :)
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct PollReplyPacket
{
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
    public byte[] IpV4;
        
    public BigEndianUShort Port, Firmware, SubSwitch, OEM;
    public byte UBEAVersion;
    public PollReplyStatus Status;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
    public string ShortName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string LongName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string NodeReport;

    public ushort PortCount;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
    public byte[] PortTypes;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
    public byte[] GoodInput;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
    public byte[] GoodOutput;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
    public byte[] SwIn;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
    public byte[] SwOut;

    public byte SwVideo;
    public byte SwMacro;
    public byte SwRemote;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 3)]
    public byte[] Padding;

    public byte Style;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 6)]
    public byte[] MacAddress;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
    public byte[] BindIpAddress;

    public byte BindIndex;
    public byte AltStatus;
}