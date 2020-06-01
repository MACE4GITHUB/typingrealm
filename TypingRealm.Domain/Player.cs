using System;
using System.Linq;
using TypingRealm.Domain.Movement;

namespace TypingRealm.Domain
{
    public sealed class Player
    {
        private readonly ILocationStore _locationStore;

        public Player(PlayerId playerId, PlayerName name, LocationId locationId, ILocationStore locationStore)
        {
            PlayerId = playerId;
            Name = name;
            LocationId = locationId;
            _locationStore = locationStore;
        }

        public PlayerId PlayerId { get; }
        public PlayerName Name { get; }
        public LocationId LocationId { get; private set; }

        public void MoveToLocation(LocationId locationId)
        {
            if (LocationId == locationId)
                throw new InvalidOperationException($"Player {PlayerId} is already at location {LocationId}. Cannot move to the same location.");

            var currentLocation = _locationStore.Find(LocationId);
            if (currentLocation == null)
                throw new InvalidOperationException($"The player {PlayerId} is currently at invalid location {LocationId}. Cannot move to location {locationId}.");

            if (!currentLocation.Locations.Contains(locationId))
                throw new InvalidOperationException($"Cannot move the player {PlayerId} from {LocationId} to {locationId}.");

            LocationId = locationId;
        }

        public string GetUniquePlayerPosition()
        {
            return $"l_{LocationId}";
        }
    }
}
