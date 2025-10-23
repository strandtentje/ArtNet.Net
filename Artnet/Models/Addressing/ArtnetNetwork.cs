namespace Artnet.Models;

/// <summary>
/// Data object and factory methods for selecting a suitable artnet network, by fixed configuration,
/// or by adaptors as configured in your operating system.
/// </summary>
/// <param name="address">IP Address to bind to</param>
/// <param name="broadcast">Broadcast Address</param>
/// <param name="mask">Subnet mask</param>
/// <param name="port">Artnet Port number, typically 6454</param>
public class ArtnetNetwork(IPAddress address, IPAddress broadcast, IPAddress mask, ushort port)
{
    /// <summary>
    /// Artnet IP address, typically in the 2.x.x.x or 10.x.x.x range.
    /// </summary>
    public IPAddress Address => address;

    /// <summary>
    /// Artnet subnet mask, typically 255.0.0.0
    /// </summary>
    public IPAddress Mask => mask;

    /// <summary>
    /// Broadcast address, typically 2.255.255.255 or 10.255.255.255
    /// </summary>
    public IPAddress Broadcast => broadcast;

    /// <summary>
    /// Artnet port, typically UDP 6454
    /// </summary>
    public ushort Port => port;
    public override string ToString() => $"{address}:{port}/{mask}/{broadcast}";
}