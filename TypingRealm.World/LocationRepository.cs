using System;
using System.Collections.Generic;
using System.Linq;
using TypingRealm.Profiles.Activities;
using TypingRealm.Profiles.Api.Client;
using TypingRealm.Profiles.Api.Resources;
using TypingRealm.World.Layers;

namespace TypingRealm.World
{
    public sealed class LocationStore
    {
        public List<Location> Locations { get; } = new List<Location>
        {
            Location.FromPersistenceState(new LocationPersistenceState
            {
                LocationId = "1",
                Locations = new HashSet<string> { "2" },
                AllowedActivityTypes = new HashSet<ActivityType>(),
                Characters = new HashSet<string>(),
                Activities = new HashSet<Activity>()
            }),
            Location.FromPersistenceState(new LocationPersistenceState
            {
                LocationId = "2",
                Locations = new HashSet<string> { "1" },
                AllowedActivityTypes = new HashSet<ActivityType>
                {
                    ActivityType.RopeWar
                },
                Characters = new HashSet<string>(),
                Activities = new HashSet<Activity>()
            })
        };
    }

    public sealed class LocationRepository : ILocationRepository, IActivityStore
    {
        private readonly LocationStore _locationStore;
        private readonly IActivitiesClient _activitiesClient;

        public LocationRepository(
            LocationStore locationStore,
            IActivitiesClient activitiesClient)
        {
            _locationStore = locationStore;
            _activitiesClient = activitiesClient;
        }

        public Location? Find(string locationId)
        {
            return _locationStore.Locations.FirstOrDefault(l => l.LocationId == locationId);
        }

        public Location? FindLocationForCharacter(string characterId)
        {
            return _locationStore.Locations.FirstOrDefault(l => l.Characters.Contains(characterId));
        }

        public Location FindStartingLocation(string characterId)
        {
            return _locationStore.Locations.FirstOrDefault(l => l.LocationId == "1")!;
        }

        public Activity? GetCurrentCharacterActivityOrDefault(string characterId)
        {
            var location = _locationStore.Locations.FirstOrDefault(l => l.Characters.Contains(characterId));
            if (location == null)
                throw new InvalidOperationException("Location for this character does not exist.");

            // TODO: Get descriptors instead of actual objects that can be modified.
            // TODO: Query total list of activities, not just ropewar activities.
            // And somehow get the STACK of activities for the character?..
            var currentActivity = location.GetCurrentActivityOrDefault(characterId);

            return currentActivity;
        }

        public void Save(Location location)
        {
            foreach (var @event in location.CommitDomainEvents())
            {
                if (@event.Type == DomainEventType.ActivityStarted)
                {
                    HandleActivityStarted(@event.ActivityId, @event.ActivityType);
                }
            }
        }

        private Location GetLocationForActivity(string activityId)
        {
            var location = _locationStore.Locations.SingleOrDefault(l => l.HasActivity(activityId));
            if (location == null)
                throw new InvalidOperationException("No such activity found in any of the locations.");

            return location;
        }

        private void HandleActivityStarted(string activityId, ActivityType activityType)
        {
            var location = GetLocationForActivity(activityId);
            var characters = location.GetAllCharactersInActivity(activityId);

            var activityResource = new ActivityResource(activityId, activityType, characters);
            _activitiesClient.StartActivityAsync(activityResource, default)
                .AsTask().GetAwaiter().GetResult();
        }
    }
}
