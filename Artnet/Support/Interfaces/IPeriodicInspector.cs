namespace Artnet.Support;

public interface IPeriodicInspector
{
    IPeriodicInspector Start(int interval);
}