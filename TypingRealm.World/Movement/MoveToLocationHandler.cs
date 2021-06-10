using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.World.Layers;

namespace TypingRealm.World.Movement
{
    // TODO: I still can move between locations even when I participate in activity. Make sure it's impossible.
    public sealed class MoveToLocationHandler : LayerHandler<MoveToLocation>
    {
        private readonly ILocationStore _locationStore;

        public MoveToLocationHandler(
            ICharacterActivityStore characterActivityStore,
            ILocationStore locationStore)
            : base(characterActivityStore, Layer.World)
        {
            _locationStore = locationStore;
        }

        protected override ValueTask HandleLayeredMessageAsync(ConnectedClient sender, MoveToLocation message, CancellationToken cancellationToken)
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
            location.RemoveCharacter(characterId);
            _locationStore.Save(location);

            newLocation.AddCharacter(characterId);
            _locationStore.Save(newLocation);

            sender.Group = newLocation.LocationId;

            return default;
        }
    }
}
