namespace Artnet.Models;

/// <summary>
/// Collection of multiple artnet networks that is valid for use with the ArtnetSocket
/// </summary>
/// <param name="networks">(Unvalidated) networks to use</param>
public class ArtnetNetworkCollection(params ArtnetNetwork[] networks) : IEnumerable<ArtnetNetwork>
{
    /// <summary>
    /// Common port between Artnet Networks
    /// </summary>
    public ushort CommonPort { get; } = networks.Select(x => x.Port).Distinct().Count() == 1
        ? networks.First().Port
        : throw new InvalidOperationException(
            """
            Provided networks unsuitable for communicating with; either no networks were provided at all, 
            or multiple different ports were requested.
            """);

    /// <summary>
    /// All broadcast endpoints associated with the ArtnetNetworks in this collection
    /// </summary>
    public IPEndPoint[] BroadcastEndpoints { get; } = [..networks.Select(x => new IPEndPoint(x.Broadcast, x.Port))];
    public IEnumerator<ArtnetNetwork> GetEnumerator() => networks.Where(x => true).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public static implicit operator ArtnetNetworkCollection(ArtnetNetwork[] networks) => new(networks);
    public static implicit operator ArtnetNetworkCollection(ArtnetNetwork network) => new([network]);
}