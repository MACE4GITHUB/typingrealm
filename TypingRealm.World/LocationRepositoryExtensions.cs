using System;
using TypingRealm.Messaging;

namespace TypingRealm.World;

public static class LocationRepositoryExtensions
{
    public static Location FindLocationForClient(this ILocationRepository locationRepository, ConnectedClient sender)
    {
        var location = locationRepository.FindLocationForCharacter(sender.ClientId);
        if (location == null)
            throw new InvalidOperationException("Character never joined the world.");

        if (location.LocationId != sender.Group)
            throw new InvalidOperationException("Synchronization mismatch.");

        return location;
    }
}
