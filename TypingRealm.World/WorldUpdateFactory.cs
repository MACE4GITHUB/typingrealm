using System;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.World
{
    public sealed class WorldUpdateFactory : IUpdateFactory
    {
        private readonly ILocationRepository _locationStore;

        public WorldUpdateFactory(
            ILocationRepository locationStore)
        {
            _locationStore = locationStore;
        }

        public object GetUpdateFor(string clientId)
        {
            return _locationStore.FindLocationForCharacter(clientId)?.GetWorldState() ?? throw new InvalidOperationException("Location does not exist.");
        }
    }
}
