namespace Artnet.Support;
/// <summary>
/// Implementations should accept error strings here.
/// </summary>
public interface IErrorSink
{
    /// <summary>
    /// Report a parameterless error string.
    /// </summary>
    /// <param name="description">Error description in natural language.</param>
    void IngestError(string description);
}