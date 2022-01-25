namespace TypingRealm.Messaging.Updating;

/// <summary>
/// Gets update for given clientId. The update is sent every time client
/// changes it's messaging group, and it can be triggered by <see cref="IUpdateDetector"/>.
/// </summary>
public interface IUpdateFactory
{
    object GetUpdateFor(string clientId);
}
