namespace Artnet.Support;

/// <summary>
/// Implementations should accept message strings here.
/// </summary>
public interface IMessageSink
{
    /// <summary>
    /// Report a parameterless message string
    /// </summary>
    /// <param name="description">Message description in natural language</param>
    void IngestMessage(string description);
}