namespace Artnet;

/// <summary>
/// If you just want to be in the ArtNet network and read DMX values.
/// </summary>
public static class UnfilteredDmxReader
{
    /// <summary>
    /// If ServiceCollection's too much noise for what you're trying to achieve, which is probably
    /// just something that listens on ArtNet and acts upon it. This will set up everything in a
    /// reasonable way that's light on resources, and outputs an IUniverseSource which can be used to
    /// read out DMX universes.
    /// </summary>
    /// <param name="subnetPattern">Pattern of the preferred subnet; will bind to the best option.</param>
    /// <param name="dumpInterval">Diagnostic dump interval (defaults to 10 seconds)</param>
    /// <returns></returns>
    public static IUniverseSource CreateSource(string subnetPattern = "^10\\.", int dumpInterval = 10000)
    {
        var services = (new ServiceCollection()).AddArtnetLogToConsole(subnetPattern).BuildServiceProvider();
        services.GetRequiredService<IPeriodicInspector>().Start(dumpInterval);
        services.GetRequiredService<IArtnetReceiver>().Start();
        return services.GetRequiredService<IUniverseSource>();
    }
}