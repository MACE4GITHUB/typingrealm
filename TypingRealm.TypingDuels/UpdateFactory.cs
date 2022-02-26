using TypingRealm.Messaging.Updating;

namespace TypingRealm.TypingDuels;

public sealed class UpdateFactory : IUpdateFactory
{
    public object GetUpdateFor(string clientId)
    {
        return new Update();
    }
}
