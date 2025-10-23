namespace Artnet;

public interface IArtnetTransmitter
{
    void Send<TPacket>(ArtNetOpCode opcode, TPacket packet) where TPacket : struct;
}