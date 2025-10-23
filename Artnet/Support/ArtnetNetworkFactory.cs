// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
namespace Artnet.Models;

/// <summary>
/// Utilities to construct the network protocol addresses into
/// ArtnetNetworks and Collections of ArtnetNetworks.
/// </summary>
/// <param name="messages"></param>
public class ArtnetNetworkFactory(IMessageSink messages)
{
    /// <summary>
    /// Setup an Artnet Network for each detected NIC with an IPv4 address
    /// </summary>
    /// <returns>An artnet Network for each nic that's up</returns>
    public ArtnetNetworkCollection CollectionFromPhysical()
    {
        NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        messages.IngestMessage($"Detected {allNetworkInterfaces.Length} network interfaces");
        NetworkInterface[] ipv4Interfaces =
            [..allNetworkInterfaces.Where(x => x.Supports(NetworkInterfaceComponent.IPv4))];
        messages.IngestMessage($"Of which {ipv4Interfaces.Length} offer the supported IPv4 addressing");
        NetworkInterface[] wakingInterfaces =
            [..ipv4Interfaces.Where(x => x.OperationalStatus == OperationalStatus.Up)];
        foreach (NetworkInterface networkInterface in wakingInterfaces)
            messages.IngestMessage($"Interface {networkInterface.Name} is UP");
        UnicastIPAddressInformation[] unicastAddresses =
            [..wakingInterfaces.SelectMany(x => x.GetIPProperties().UnicastAddresses)];
        ArtnetNetwork[] result =
            [..unicastAddresses.Select(FromPhysical).ToArray()];
        foreach (ArtnetNetwork artnetNetwork in result)
            messages.IngestMessage($"ArtNet binding to: {artnetNetwork}");
        return result;
    }
    /// <summary>
    /// When using an IP address from the "Standard" artnet range.
    /// </summary>
    /// <param name="standardAddress">A 10.x.x.x or 2.x.x.x IP address</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">In case the address wasn't in range.</exception>
    public ArtnetNetwork FromStandardArtnetIp(IPAddress standardAddress)
    {
        var addressBytes = standardAddress.GetAddressBytes();
        if (addressBytes.ElementAtOrDefault(0) == 2)
            return new(standardAddress, IPAddress.Parse("2.255.255.255"), IPAddress.Parse("255.0.0.0"), 6454);
        else if (addressBytes.ElementAtOrDefault(0) == 10)
            return new(standardAddress, IPAddress.Parse("10.255.255.255"), IPAddress.Parse("255.0.0.0"), 6454);
        else
            throw new ArgumentException("Standard ArtNet IP ranges are 10.x.x.x or 2.x.x.x", nameof(standardAddress));
    }
    /// <summary>
    /// When using a non-standard IP-range for artnet, use this factory method.
    /// Supply IP address and subnet size.
    /// </summary>
    /// <param name="customAddress">Custom IP address for this particular node.</param>
    /// <param name="size">Custom IP range for this particular node.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public ArtnetNetwork FromCustomArtnetIp(IPAddress customAddress, SubnetSize size)
    {
        var addressBytes = customAddress.GetAddressBytes();
        switch (size)
        {
            case SubnetSize.By8:
                var by8Broadcast = new IPAddress([addressBytes[0], addressBytes[1], addressBytes[2], 0xFF]);
                return new ArtnetNetwork(customAddress, by8Broadcast, IPAddress.Parse("255.255.255.0"), 6454);
            case SubnetSize.By16:
                var by16Broadcast = new IPAddress([addressBytes[0], addressBytes[1], 0xFF, 0xFF]);
                return new ArtnetNetwork(customAddress, by16Broadcast, IPAddress.Parse("255.255.0.0"), 6454);
            case SubnetSize.By24:
                var by24Broadcast = new IPAddress([addressBytes[0], 0xFF, 0xFF, 0xFF]);
                return new ArtnetNetwork(customAddress, by24Broadcast, IPAddress.Parse("255.0.0.0"), 6454);
            default:
                throw new ArgumentException("Unknown Subnet Size", nameof(size));
        }
    }
    /// <summary>
    /// Setup an Artnet Network provided Unicast IP address info (the stuff you get from DHCP or enter manually in the 3 boxes)
    /// </summary>
    /// <param name="arg">Unicast info</param>
    /// <returns>A suitable artnet network</returns>
    public ArtnetNetwork FromPhysical(UnicastIPAddressInformation arg)
    {
        byte[] maskBytes = arg.IPv4Mask.GetAddressBytes();
        var ipRange = arg.Address.GetAddressBytes()
            .Zip(maskBytes, (address, mask) => address & mask);
        var broadcast = ipRange.Zip(maskBytes, (range, mask) => (byte)(range | ~mask)).ToArray();
        return new(arg.Address, new IPAddress(broadcast), arg.IPv4Mask, 6454);
    }
}