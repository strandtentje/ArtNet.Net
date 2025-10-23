namespace Artnet.Models;

/// <summary>
/// Size of the artnet subnet.
/// </summary>
public enum SubnetSize
{
    /// <summary>
    /// Most common small network subnet; expressed as 255.255.255.0 with a x.x.x.255 broadcast
    /// </summary>
    By24,

    /// <summary>
    /// Subnet often used with ie. 192.168.x.x; expressed as 255.255.0.0 with a x.x.255 broadcast
    /// </summary>
    By16,

    /// <summary>
    /// The "most standard" ArtNet subnet; expressed as 255.0.0.0 with a x.255.255.255 broadcast
    /// </summary>
    By8
}