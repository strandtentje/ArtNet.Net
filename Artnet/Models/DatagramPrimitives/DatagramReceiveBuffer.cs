namespace Artnet.Models;

/// <summary>
/// A pre-allocated receive buffer for datagrams from the network, that keeps
/// track of the involved endpoints to avoid looping packets, and the length
/// ultimately received. May be used with the Distributor.
/// </summary>
/// <param name="size"></param>
public class DatagramReceiveBuffer(int size)
{
    /// <summary>
    /// Pre-allocated array of bytes to receive datagrams into.
    /// </summary>
    public readonly byte[] Data = new byte[size];

    /// <summary>
    /// Amount of data actually received at the most recent transmission
    /// </summary>
    public int Length;

    /// <summary>
    /// Local endpoint that would have received the payload
    /// </summary>
    public EndPoint LocalEndpoint = new IPEndPoint(IPAddress.Any, 6454);

    /// <summary>
    /// Remote endpoint that has sent the payload
    /// </summary>
    public EndPoint RemoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

    /// <summary>
    /// Returns true if the datagram was sent from the receiving end.
    /// </summary>
    public bool IsLooping =>
        LocalEndpoint is IPEndPoint ipLocal &&
        RemoteEndpoint is IPEndPoint ipRemote &&
        ipLocal.Address.GetAddressBytes().SequenceEqual(ipRemote.Address.GetAddressBytes()) &&
        ipLocal.Port == ipRemote.Port;

    /// <summary>
    /// Resets the sending and receiving endpoint properties to a safe
    /// default state.
    /// </summary>
    /// <param name="localPort">Port the local endpoint is listening on</param>
    public void Reset(ushort localPort)
    {
        LocalEndpoint = new IPEndPoint(IPAddress.Any, localPort);
        RemoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
    }
}