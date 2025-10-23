namespace Artnet;

public interface IArtnetReceiver
{
    event EventHandler<byte>? ArtnetPoll;
    event EventHandler<PollReplyPacket>? ArtnetPollReply;
    IArtnetReceiver Start();
    void Stop();
}