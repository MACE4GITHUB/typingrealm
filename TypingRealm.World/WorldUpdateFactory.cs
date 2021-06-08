using System;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.World
{
    public sealed class WorldUpdateFactory : IUpdateFactory
    {
        private readonly ILocationStore _locationStore;

        public WorldUpdateFactory(
            ILocationStore locationStore)
        {
            _locationStore = locationStore;
        }

        public object GetUpdateFor(string clientId)
        {
            return _locationStore.FindLocationForCharacter(clientId)?.GetWorldState() ?? throw new InvalidOperationException("Location does not exist.");
        }
    }
}
