using System;
using System.Collections.Generic;
using System.Linq;
using TypingRealm.Messaging;
using TypingRealm.World.Activities;
using TypingRealm.World.Activities.RopeWar;

namespace TypingRealm.World
{
#pragma warning disable CS8618
    [Message]
    public sealed class WorldState
    {
        public string LocationId { get; set; }
        public List<string> Locations { get; set; }
        public List<ActivityType> AllowedActivityTypes { get; set; }
        public List<string> Characters { get; set; }

        public List<RopeWarActivityState> RopeWarActivities { get; set; }
    }

    public sealed class LocationPersistenceState
    {
        public string LocationId { get; set; }
        public HashSet<string> Locations { get; set; }
        public HashSet<ActivityType> AllowedActivityTypes { get; set; }
        public HashSet<Activity> Activities { get; set; }
        public HashSet<string> Characters { get; set; }
    }
#pragma warning restore CS8618

#pragma warning disable IDE0032
    public sealed class Location
    {
        private readonly string _locationId;
        private readonly HashSet<string> _locations;
        private readonly HashSet<ActivityType> _allowedActivityTypes;
        private readonly HashSet<Activity> _activities = new HashSet<Activity>();
        private readonly HashSet<string> _characters = new HashSet<string>();

        private Location(LocationPersistenceState persistenceState)
        {
            _locationId = persistenceState.LocationId;
            _locations = persistenceState.Locations;
            _allowedActivityTypes = persistenceState.AllowedActivityTypes;
            _activities = persistenceState.Activities;
            _characters = persistenceState.Characters;
        }

        public static Location FromPersistenceState(LocationPersistenceState state)
        {
            return new Location(state);
        }

        public LocationPersistenceState GetPersistenceState()
        {
            return new LocationPersistenceState
            {
                LocationId = _locationId,
                Locations = _locations,
                Activities = _activities,
                Characters = _characters,
                AllowedActivityTypes = _allowedActivityTypes
            };
        }

        public Location(
            string locationId,
            HashSet<string> locations,
            HashSet<ActivityType> allowedActivityTypes)
        {
            _locationId = locationId;
            _locations = locations;
            _allowedActivityTypes = allowedActivityTypes;
        }

        public string LocationId => _locationId;
        public bool CanProposeRopeWar => _allowedActivityTypes.Contains(ActivityType.RopeWar);

        public IEnumerable<string> Characters => _characters;
        public IEnumerable<string> Locations => _locations;

        // TODO: Make this private, do not expose modifiable objects outside aggregate root boundary.
        public IEnumerable<RopeWarActivity> RopeWarActivities => _activities
            .Where(a => a.Type == ActivityType.RopeWar)
            .Select(activity => (RopeWarActivity)activity);

        public void VoteToStartRopeWar(string characterId)
        {
            var ropeWar = GetRopeWarFor(characterId);
            if (ropeWar == null)
                throw new InvalidOperationException("Rope war does not exist for this character.");

            if (ropeWar.HasStarted || ropeWar.HasFinished)
                throw new InvalidOperationException("Rope war has already started or finished.");

            ropeWar.VoteToStart(characterId);
        }

        public void CreateActivity(Activity activity)
        {
            if (!_allowedActivityTypes.Contains(activity.Type))
                throw new InvalidOperationException("Cannot create this activity at this location.");

            _activities.Add(activity);
        }

        public void AddCharacter(string character)
        {
            _characters.Add(character);
        }

        public void RemoveCharacter(string character)
        {
            // TODO: When moving from one location to another we need to do a transaction. Consider having a separate "Character" aggregate that will have locationId set on it.
            _characters.Remove(character);
        }

        public WorldState GetWorldState()
        {
            return new WorldState
            {
                LocationId = LocationId,
                Locations = _locations.ToList(),
                AllowedActivityTypes = _allowedActivityTypes.ToList(),
                RopeWarActivities = RopeWarActivities.Select(activity => activity.GetState()).ToList(),
                Characters = _characters.ToList()
            };
        }

        private RopeWarActivity? GetRopeWarFor(string characterId)
        {
            return RopeWarActivities.FirstOrDefault(rw => rw.HasParticipant(characterId));
        }
    }
#pragma warning restore IDE0032 // Use auto property
}
