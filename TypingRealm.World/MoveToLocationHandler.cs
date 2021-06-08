using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace TypingRealm.World
{
    // TODO: I still can move between locations even when I participate in activity. Make sure it's impossible.
    public sealed class MoveToLocationHandler : IMessageHandler<MoveToLocation>
    {
        private readonly ILocationStore _locationStore;

        public MoveToLocationHandler(ILocationStore locationStore)
        {
            _locationStore = locationStore;
        }

        public ValueTask HandleAsync(ConnectedClient sender, MoveToLocation message, CancellationToken cancellationToken)
        {
            var characterId = sender.ClientId;
            var location = _locationStore.FindLocationForCharacter(characterId);

            if (location == null)
            {
                // Character has never connected yet.
                throw new InvalidOperationException("Cannot move between locations. Did not join the world yet.");
            }

            // Consider using sender.Group to get current location.
            if (sender.Group != location.LocationId)
                throw new InvalidOperationException("Current group of player is not equal to location. Synchronization mismatch.");

            if (!location.Locations.Contains(message.LocationId))
                throw new InvalidOperationException("Cannot move to this location from another location.");

            var newLocation = _locationStore.Find(message.LocationId);
            if (newLocation == null)
                throw new InvalidOperationException("Location does not exist.");

            // TODO: Transaction?
            location.Characters.Remove(characterId);
            _locationStore.Save(location);

            newLocation.Characters.Add(characterId);
            _locationStore.Save(newLocation);

            sender.Group = newLocation.LocationId;

            return default;
        }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
