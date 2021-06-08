using System;
using TypingRealm.Messaging;

namespace TypingRealm.World
{
    public static class LocationStoreExtensions
    {
        public static Location FindLocationForClient(this ILocationStore locationStore, ConnectedClient sender)
        {
            var location = locationStore.FindLocationForCharacter(sender.ClientId);
            if (location == null)
                throw new InvalidOperationException("Character never joined the world.");

            if (location.LocationId != sender.Group)
                throw new InvalidOperationException("Synchronization mismatch.");

            return location;
        }
    }
}
