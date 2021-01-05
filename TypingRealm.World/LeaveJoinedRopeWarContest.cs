using System;
using TypingRealm.Messaging;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace TypingRealm.World
{
    [Message]
    public sealed class LeaveJoinedRopeWarContest
    {
        // Should return money to player if it's not started yet, otherwise player loses money.
        // Should reset "vote" to start the contest.
        // Should delete the contest if it was the last player.
    }

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
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
