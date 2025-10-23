namespace Artnet.Universes;
/// <summary>
/// Interface for exposing DMX data
/// </summary>
public interface IUniverse
{
    /// <summary>
    /// DMX Data. Expected but not guarnateed:
    ///  - 512 in length
    ///  - Any member updates at any time.
    /// </summary>
    byte[] Data { get; }
}