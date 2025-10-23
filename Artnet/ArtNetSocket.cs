namespace Artnet;

/// <summary>
/// ArtNet-specific implementation of the common .Net UDP Datagram Socket. Usage of IServiceCollection
/// is recommended.
/// </summary>
/// <param name="networks">Networks to broadcast and listen to</param>
/// <param name="universes">
/// Target service for sending Universe Updates to.
/// We use this instead of events to avoid excessive allocations due to the plentiful DMX
/// datagrams on an ArtNet network.
/// </param>
/// <param name="receiveBuffers">Distributor of Buffers for the Receive workers</param>
/// <param name="errorSink">Target for Receive-time errors that don't need to break runtime</param>
/// <param name="sendBufferPool">
/// Pool of byte buffers used for sending ArtNet packets;
/// 10-20 buffers of 1000-2000 length is recommended
/// </param>
public class ArtNetSocket(
    ArtnetNetworkCollection networks,
    IUniverseSink universes,
    Distributor<DatagramReceiveBuffer> receiveBuffers,
    IMessageSink messages,
    IErrorSink errorSink,
    ObjectPool<byte[]> sendBufferPool)
    : Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp), IArtnetTransmitter, IArtnetReceiver
{
    /// <summary>
    /// When a Poll datagram was received on any ArtNet Network
    /// </summary>
    public event EventHandler<byte>? ArtnetPoll;

    /// <summary>
    /// When a Poll reply was received on any ArtNet Network
    /// </summary>
    public event EventHandler<PollReplyPacket>? ArtnetPollReply;

    /// <summary>
    /// Start Listening on the Artnet Network; after calling this,
    /// the ArtNetPoll and ArtNetPollReply events may start happening.
    /// Any DMX data updates will be send to the IUniverseSink implementation;
    /// its implementing instance is typically available to the DMX data consumer
    /// as IUniverseSource. 
    /// </summary>
    public IArtnetReceiver Start()
    {
        if (receiveBuffers.IsRunning) return this;

        receiveBuffers.BeforeStart += BuffersOnBeforeStart;
        receiveBuffers.ResourceAvailable += BuffersOnResourceAvailable;
        receiveBuffers.AfterStop += OnReceiveBuffersOnAfterStop;

        void OnReceiveBuffersOnAfterStop(object sender, EventArgs args)
        {
            receiveBuffers.ResourceAvailable -= BuffersOnResourceAvailable;
            receiveBuffers.AfterStop -= OnReceiveBuffersOnAfterStop;
        }

        void BuffersOnBeforeStart(object sender, EventArgs e)
        {
            SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            bool bound = false;
            foreach (ArtnetNetwork artnetNetwork in networks)
            {
                try
                {
                    IPEndPoint ipEndPoint = new IPEndPoint(artnetNetwork.Broadcast, artnetNetwork.Port);
                    Bind(ipEndPoint);
                    messages.IngestMessage($"Bound to {ipEndPoint}");
                    bound = true;
                    break;
                }
                catch (SocketException ex)
                {
                    errorSink.IngestError($"Failure during binding stage of {artnetNetwork}; {ex}");
                }
            }

            if (!bound)
                throw new InvalidOperationException(
                    "Could not bind to any address. Check earlier messages. Is your network OK?");
            SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            receiveBuffers.BeforeStart -= BuffersOnBeforeStart;
        }

        receiveBuffers.Start();
        return this;
    }

    /// <summary>
    /// Stop listening for incoming ArtNet traffic. Data may still be sent.
    /// </summary>
    public void Stop()
    {
        if (!receiveBuffers.IsRunning)
            receiveBuffers.Stop();
    }

    /// <summary>
    /// When a receive buffer becomes available, this starts using that for receiving ArtNet data.
    /// </summary>
    /// <param name="sender">Typically a Distributor</param>
    /// <param name="e">The buffer that has become available.</param>
    /// <exception cref="InvalidOperationException">
    /// On network errors; is Fatal and will break runtime and the Distributor.
    /// </exception>
    private void BuffersOnResourceAvailable(object sender, DatagramReceiveBuffer e)
    {
        e.Reset(networks.CommonPort);
        try
        {
            BeginReceiveFrom(e.Data, 0, e.Data.Length, SocketFlags.None, ref e.LocalEndpoint,
                ReceivingDatagram, e);
        }
        catch (SocketException ex)
        {
            receiveBuffers.Stop();
            throw new InvalidOperationException("Failed to listen on socket; are all supplied networks up?", ex);
        }
    }

    /// <summary>
    /// When we're done receiving an entire datagram, this will determine if it's ArtNet,
    /// what kind of Artnet it was and what to do next.
    /// </summary>
    /// <param name="ar">Came from the Socket we're overriding.</param>
    /// <exception cref="ArgumentException">
    /// If we got a datagram, but no buffer that was stored in,
    /// this will break runtime and the Distributor.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// If we couldn't acquire the datagram contents from the underlying socket,
    /// this will break runtime and the Distributor.
    /// </exception>
    private void ReceivingDatagram(IAsyncResult ar)
    {
        var buffer = ar.AsyncState as DatagramReceiveBuffer;
        try
        {
            if (!receiveBuffers.IsRunning)
                return;
            if (buffer == null)
            {
                if (receiveBuffers.IsRunning) receiveBuffers.Stop();
                throw new ArgumentException(
                    "Failed to finalize receiving datagram; was BeginReceiveFrom called with the wrong async state?");
            }

            try
            {
                buffer.Length = EndReceiveFrom(ar, ref buffer.RemoteEndpoint);
            }
            catch (SocketException ex)
            {
                if (receiveBuffers.IsRunning) receiveBuffers.Stop();
                throw new InvalidOperationException("Failed to finalize receiving datagram", ex);
            }

            if (buffer.IsLooping)
                return;

            var opcode = buffer.ValidateProtocol(out var payload);
            switch (opcode)
            {
                case ArtNetOpCode.Poll:
                    ArtnetPoll?.Invoke(this, payload[0]);
                    break;
                case ArtNetOpCode.PollReply:
                    if (ArtnetPollReply != null)
                        ArtnetPollReply.Invoke(this, MemoryMarshal.Read<PollReplyPacket>(payload));
                    break;
                case ArtNetOpCode.Dmx:
                    var preamble = MemoryMarshal.Read<DmxPreamble>(payload);
                    ushort dmxLength = preamble.DmxLength.Value;
                    universes.Update(dmxLength, preamble, payload);
                    break;
                default:
                    errorSink.IngestError(opcode.ToString());
                    break;
            }
        }
        finally
        {
            receiveBuffers.ReturnResource(buffer);
        }
    }

    /// <summary>
    /// Use this to send an ArtNet packet; provide OpCode and only the Payload part of
    /// the package as a struct. The Protocol header will be included automatically using
    /// this method.
    /// </summary>
    /// <param name="opcode">Opcode to include in the header. Make sure this matches the remaining packet</param>
    /// <param name="packet">Packet struct to send on the ArtNet network.</param>
    /// <typeparam name="TPacket">A struct that is to be turned into a bunch of bytes sequentially.</typeparam>
    public void Send<TPacket>(ArtNetOpCode opcode, TPacket packet) where TPacket : struct
    {
        var sendBuffer = sendBufferPool.Get();
        try
        {
            var sendSize = opcode.BuildProtocol(packet, sendBuffer);
            foreach (var broadcastEndpoint in networks.BroadcastEndpoints)
                SendTo(sendBuffer, 0, sendSize, SocketFlags.Broadcast, broadcastEndpoint);
        }
        finally
        {
            sendBufferPool.Return(sendBuffer);
        }
    }

    /// <summary>
    /// Stops the Distributor and releases underlying resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        Stop();
        base.Dispose(disposing);
    }
}