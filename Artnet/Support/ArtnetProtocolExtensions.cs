namespace Artnet.Support;

/// <summary>
/// Utility extensions to aid in turning datagrams into validated ArtNet payloads, and vice versa.
/// </summary>
public static class ArtnetProtocolExtensions
{
    /// <summary>
    /// Art-Net Header bytes. Note the definition explicitly includes the null terminator
    /// and takes the first 8 bytes of that, to more verbosely communicate the header size and
    /// composition with the reader.
    /// </summary>
    private static readonly byte[] ArtnetHeader = Encoding.ASCII.GetBytes("Art-Net\0").Take(8).ToArray();
    /// <summary>
    /// Artnet Opcodes are weird in that the spec says
    /// a) They're Little Endian (low byte first)
    /// b) The defined OpCodes only have the high bytes set
    /// Effectively turning the Little Endian OpCode into a Big Endian Opcode where
    /// we only have to check the byte at the biggest address to know the Opcode;
    /// we only need to check the lower address for 0, because no opcodes use that (yet)
    /// Weird huh.
    /// So the byte at this address is always 0x00
    /// </summary>
    private const int OPCODE_LO = 8;
    /// <summary>
    /// Check the docs on OPCODE_LO, but the byte at this position effectively is
    /// what tells us what the OPCODE is.
    /// </summary>
    private const int OPCODE_HI = 9;
    /// <summary>
    /// MSB of the 16 bit Artnet Version number, but we're at version 1.4 (14 decimal)
    /// so this is always 0
    /// </summary>
    private const int VERSION_MSB = 10;
    /// <summary>
    /// This is expected to be in the ballpart around 14 decimal; the Artnet spec claims to
    /// be largely backward/forward compatible, so hence the ballpark. 
    /// </summary>
    private const int VERSION_LSB = 11;
    /// <summary>
    /// Where the actual payload of an ArtNet package _generally_ starts. Except for PollReply.
    /// PollReply has no version. But it has so many fields, you'd really hope it would send a Version number.
    /// It's best we zero out a byte array of at least 207 (check the spec) to accomodate receiving
    /// the PollReply and hope the contents are compatible. 
    /// </summary>
    private const int PAYLOAD_START = 12;
    /// <summary>
    /// Validate the Buffer of the Received Datagram, output the Opcode and the remaining Payload for parsing
    ///  - It checks if the package is big enough (heh)
    ///  - It validates the header bytes ("Art-Net\0")
    ///  - It validates the opcode to be known
    ///  - It validates the protocol version (in case we got anything other than a PollReply)
    /// Upon failure, the presumed payload will still be available, but the return value will have the
    /// FailureMode flag set.
    /// </summary>
    /// <param name="buffer">Datagram Buffer to Validate for correct ArtNet</param>
    /// <param name="payload">Post-ArtNet header Payload</param>
    /// <returns>OpCode Flags Enum that has FailureMode set in case of a failure.</returns>
    public static ArtNetOpCode ValidateProtocol(this DatagramReceiveBuffer buffer, out ReadOnlySpan<byte> payload)
    {
        if (buffer.Data[OPCODE_HI] == (int)ArtNetOpCode.PollReply)
            payload = ((Span<byte>)buffer.Data).Slice(PAYLOAD_START, Math.Max(0, buffer.Length - VERSION_MSB));
        else
            payload = ((Span<byte>)buffer.Data).Slice(PAYLOAD_START, Math.Max(0, buffer.Length - PAYLOAD_START));

        if (buffer.Length <= 12)
            return ArtNetOpCode.PacketTooShort;
        if (!buffer.Data.Take(ArtnetHeader.Length).SequenceEqual(ArtnetHeader))
            return ArtNetOpCode.HeaderWrong;
        else if (buffer.Data[OPCODE_LO] != 0)
            return ArtNetOpCode.OpcodeTooBig;
        else if (!Enum.IsDefined(typeof(ArtNetOpCode), (int)buffer.Data[OPCODE_HI]))
            return ArtNetOpCode.IllegalOpcode;
        else if (buffer.Data[OPCODE_HI] != (int)ArtNetOpCode.PollReply && (
                     buffer.Data[VERSION_MSB] != 0 ||
                     buffer.Data[VERSION_LSB] < 10 || 
                     buffer.Data[VERSION_LSB] > 14))
            return ArtNetOpCode.VersionWrong;
        else
            return (ArtNetOpCode)buffer.Data[OPCODE_HI];
    }
    /// <summary>
    /// Provided an opcode, a payload struct, and some sufficient byte[]-allocation (recommended 1000 or more),
    /// will prepare that memory to contain a valid ArtNet datagram for transmission, and outputs the size of the
    /// datagram.
    /// </summary>
    /// <param name="opCode">ArtNet Opcode to use with the payload (ensure this matches the struct; this will not be validated)</param>
    /// <param name="payload">
    /// Payload struct that will be serialized in-order and to platform-endianness(!).
    /// Sanity-check structs because the endianness of ArtNet is somewhat mixed.
    /// </param>
    /// <param name="allocation">
    /// Buffer to write the datagram to.
    /// </param>
    /// <typeparam name="TPayload"></typeparam>
    /// <returns>Transmission length as written to the allocation. We start writing at 0</returns>
    /// <exception cref="ArgumentException">In case the OpCode could not be sent</exception>
    /// <exception cref="ArgumentOutOfRangeException">In case the supplied byte array was not big enough.</exception>
    public static int BuildProtocol<TPayload>(this ArtNetOpCode opCode, TPayload payload, byte[] allocation) where TPayload : struct
    {
        int packetSize = 0;
        if (opCode == ArtNetOpCode.PollReply)
            packetSize = VERSION_MSB + Marshal.SizeOf<TPayload>();
        else if (opCode < ArtNetOpCode.FailureMode)
            packetSize = PAYLOAD_START + Marshal.SizeOf<TPayload>();
        else
            throw new ArgumentException($"Cannot send packet for opcode {opCode}", nameof(opCode));
        if (packetSize > allocation.Length)
            throw new ArgumentOutOfRangeException($"Not enough space to compose the datagram", nameof(allocation));
        ArtnetHeader.CopyTo(allocation, 0);
        allocation[OPCODE_LO] = 0;
        allocation[OPCODE_HI] = (byte)((int)opCode & 0xff);
        var span = (Span<byte>)allocation;
        if (opCode == ArtNetOpCode.PollReply)
        {
            MemoryMarshal.Write(span.Slice(VERSION_MSB), ref payload);
        }
        else
        {
            allocation[VERSION_MSB] = 0;
            allocation[VERSION_LSB] = 14;
            MemoryMarshal.Write(span.Slice(PAYLOAD_START), ref payload);
        }

        return packetSize;
    }
}