namespace Artnet.Universes;
/// <summary>
/// Interface for Consumers of DMX universes and their data;
/// Exposes Universes that have have been previously detected, and
/// allows for subscribing to particular universes.
/// </summary>
public interface IUniverseSource
{
    /// <summary>
    /// All universe numbers regardless of subscription are enumerable here.
    /// </summary>
    IEnumerable<LittleEndianUniverse> KnownUniverses { get; }
    /// <summary>
    /// Get the Data for a subscribed universe; will throw if the universe wasn't subscribed to.
    /// The Data property of the return class will be up to date so long as the Universe's been
    /// subscribed to; no need to call this twice.
    /// </summary>
    /// <param name="key">Universe number</param>
    /// <returns>Universe Data</returns>
    IUniverse GetSubscribedUniverse(LittleEndianUniverse key);
    /// <summary>
    /// Register a universe number as subscribed so we can keep it up to date;
    /// This doesn't need to be a KnownUniverse, but it's recommended to do incidental
    /// runtime checks if your Universe has been detected yet, and if not, maybe do a Warning 
    /// </summary>
    /// <param name="universe">Universe number to subscribe to</param>
    void SetUniverseSubscribed(LittleEndianUniverse universe);
}