namespace Artnet;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Set up an ArtNet ServiceCollection with sane defaults,
    /// which means 8 receive buffers of 1500 bytes,
    /// listen on any ipv4 address that is assigned to an active network interface,
    /// </summary>
    /// <param name="services">Existing ServiceCollection</param>
    /// <param name="logger">Implementation to receive our information and error messages</param>
    /// <param name="preferredSubnet">Provide pattern of the preferred subnet here ie. ^192\.168\.1 or ^10\.</param>
    /// <typeparam name="TLogger"></typeparam>
    /// <returns>The modified service collection</returns>
    public static IServiceCollection AddArtnetWithSaneDefaults<TLogger>(
        this IServiceCollection services, string preferredSubnet, TLogger logger)
        where TLogger : IErrorSink, IMessageSink =>
        services
            .AddSingleton<IMessageSink>(logger)
            .AddSingleton<ArtnetNetworkFactory>()
            .AddSingleton<ArtnetNetworkCollection>(x =>
                x.GetRequiredService<ArtnetNetworkFactory>().CollectionFromPhysical(preferredSubnet))
            .AddSingleton<DmxUniverseRepository>()
            .AddSingleton<IUniverseSink>(x => x.GetRequiredService<DmxUniverseRepository>())
            .AddSingleton<IUniverseSource>(x => x.GetRequiredService<DmxUniverseRepository>())
            .AddSingleton(CreateReceiveBufferDistributor())
            .AddSingleton<IErrorSink>(logger)
            .AddSingleton(CreateTransmitBufferPool())
            .AddSingleton<RecurringUniverseInspection>()
            .AddSingleton<IPeriodicInspector>(x =>
                x.GetRequiredService<RecurringUniverseInspection>())
            .AddSingleton<ArtNetSocket>()
            .AddSingleton<IArtnetTransmitter>(x => x.GetRequiredService<ArtNetSocket>())
            .AddSingleton<IArtnetReceiver>(x => x.GetRequiredService<ArtNetSocket>());

    /// <summary>
    /// Works like `AddArtnetWithSaneDefaults`, except immediately puts messages and errors into Console,
    /// instead of some other place.
    /// </summary>
    /// <param name="services">Service collection to modify</param>
    /// <param name="preferredSubnet">Preferred subnet pattern ie ^192\.168\.1 or ^10\.</param>
    /// <returns></returns>
    public static IServiceCollection AddArtnetLogToConsole(this IServiceCollection services, string preferredSubnet) =>
        services.AddArtnetWithSaneDefaults(preferredSubnet, ConsoleWritingMessageSink.StartNew());

    /// <summary>
    /// Default Transmit buffer pool; makes 1500-byte transmit buffers and clears them on return.
    /// </summary>
    /// <returns></returns>
    public static ObjectPool<byte[]> CreateTransmitBufferPool() =>
        new DefaultObjectPool<byte[]>(new TransmitBufferPoolPolicy());

    /// <summary>
    /// Creates a new Distributor with 8 allocated resources for receive buffer
    /// </summary>
    /// <returns>Distributor ready for injection</returns>
    public static Distributor<DatagramReceiveBuffer> CreateReceiveBufferDistributor() => new(2, CreateReceiveBuffer);

    /// <summary>
    /// Creates a Receive buffer suitable for most ArtNet payloads.
    /// </summary>
    /// <returns>Receive buffer ready for distributing.</returns>
    private static DatagramReceiveBuffer CreateReceiveBuffer() => new(1500);
}