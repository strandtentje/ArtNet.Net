namespace Artnet;

public interface IArtnetTransmitter
{
    void Send<TPacket>(ArtNetOpCode opcode, TPacket packet, byte[]? tail = null) where TPacket : struct;
}