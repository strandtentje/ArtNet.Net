namespace Artnet.Support;

/// <summary>
/// A byte array pool policy that clears arrays that are returned.
/// </summary>
public class TransmitBufferPoolPolicy : IPooledObjectPolicy<byte[]>
{
    public byte[] Create() => new byte[1500];
    public bool Return(byte[] obj)
    {
        Array.Clear(obj, 0, obj.Length);
        return true;
    }
}