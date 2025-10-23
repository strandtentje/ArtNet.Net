namespace Artnet.Models;

/// <summary>
/// Artnet "Opcode" or protocol failure mode, depending on how the datagram was received.
/// </summary>
[Flags]
public enum ArtNetOpCode
{
    /// <summary>
    /// The ArtPoll packet is used to discover the presence of other Controllers, Nodes and
    /// Media Servers. The ArtPoll packet can be sent by any device, but is usually only sent by
    /// the Controller. Both Controllers and Nodes respond to the packet.
    /// From: Art-Net 4 Protocol Release V1.4 (art-net.pdf)
    /// </summary>
    Poll = 0x20,
    /// <summary>
    /// A device, in response to a Controller’s ArtPoll, sends the ArtPollReply. The device should
    /// wait for a random delay of up to 1s before sending the reply. This mechanism is intended
    /// to reduce packet bunching when scaling up to very large systems.
    /// From: Art-Net 4 Protocol Release V1.4 (art-net.pdf)
    /// </summary>
    PollReply = 0x21,
    /// <summary>
    /// ArtDmx is the data packet used to transfer DMX512 data.
    /// From: Art-Net 4 Protocol Release V1.4 (art-net.pdf)
    /// </summary>
    Dmx = 0x50,
    /// <summary>
    /// Any failure mode that didn't result in a proper packet
    /// </summary>
    FailureMode = 0x00ff_0000,
    /// <summary>
    /// Received Opcode may be within the ArtNet spec, but not implemented here. If the
    /// limited implementation of this library is enough for your application, this may be
    /// safely ignored.
    /// </summary>
    IllegalOpcode = FailureMode | 0x01,
    /// <summary>
    /// Typically indicated a spotty network connection or the configured ArtNet port
    /// also being used for other protocols. Check wiring and port settings.
    /// </summary>
    PacketTooShort = FailureMode | 0x02,
    /// <summary>
    /// As of the v1.4 ArtNet spec, no opcode utilizes the full 16 opcode bits. This may
    /// indicate your network is on a newer version, or more likely, there are network errors
    /// physically, or due to protocols getting mixed up. 
    /// </summary>
    OpcodeTooBig = FailureMode | 0x03,
    /// <summary>
    /// This means most superficial checks of the ArtNet packet passed, but the Header wasn't
    /// matched (fully) - again likely physical network errors or mixed protocols.
    /// </summary>
    HeaderWrong = FailureMode | 0x04,
    /// <summary>
    /// We were expecting version 1.0 - 1.4 but got something else instead and ceased to parse
    /// the datagram.
    /// </summary>
    VersionWrong = FailureMode | 0x05,
}