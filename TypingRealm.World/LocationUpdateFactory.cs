using System;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.World
{
    public sealed class LocationUpdateFactory : IUpdateFactory
    {
        private readonly ILocationStore _locationStore;

        public LocationUpdateFactory(
            ILocationStore locationStore)
        {
            _locationStore = locationStore;
        }

        public object GetUpdateFor(string clientId)
        {
            return _locationStore.FindLocationForCharacter(clientId) ?? throw new InvalidOperationException("Location does not exist.");
        }
    }
}
