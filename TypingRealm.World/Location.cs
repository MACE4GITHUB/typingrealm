using System;
using System.Collections.Generic;
using System.Linq;
using TypingRealm.Messaging;
using TypingRealm.Profiles.Activities;
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

    public enum DomainEventType
    {
        ActivityStarted
    }

    public sealed record DomainEvent(
        DomainEventType Type,
        string LocationId,
        string CharacterId,
        string ActivityId,
        ActivityType ActivityType);

#pragma warning disable IDE0032
    public sealed class Location
    {
        private readonly string _locationId;
        private readonly HashSet<string> _locations;
        private readonly HashSet<ActivityType> _allowedActivityTypes;
        private readonly HashSet<Activity> _activities = new HashSet<Activity>();
        private readonly HashSet<string> _characters = new HashSet<string>();

        // Experimental take on domain events.
        private readonly Queue<DomainEvent> _domainEvents = new Queue<DomainEvent>();

        public IEnumerable<DomainEvent> CommitDomainEvents()
        {
            var events = _domainEvents.ToList();
            _domainEvents.Clear();

            return events;
        }

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
        private IEnumerable<RopeWarActivity> RopeWarActivities => _activities
            .Where(a => a.Type == ActivityType.RopeWar)
            .Select(activity => (RopeWarActivity)activity);

        public bool HasActivity(string activityId) => _activities.Any(a => a.ActivityId == activityId);

        public IEnumerable<string> GetAllCharactersInActivity(string activityId)
        {
            var activity = _activities.SingleOrDefault(a => a.ActivityId == activityId);
            if (activity == null)
                throw new InvalidOperationException("Activity doesn't exist on this location.");

            return activity.GetParticipants();
        }

        public Activity? GetCurrentActivityOrDefault(string characterId)
        {
            // TODO: Do not return the activity itself, return an immutable descriptor.
            return _activities.SingleOrDefault(a => a.HasParticipant(characterId));
        }

        public void VoteToStartRopeWar(string characterId)
        {
            var ropeWar = GetRopeWarFor(characterId);

            if (!ropeWar.CanEdit)
                throw new InvalidOperationException("Rope war has already started or finished, or you cannot edit it by some other reason.");

            ropeWar.VoteToStart(characterId);

            if (ropeWar.HasStarted)
            {
                _domainEvents.Enqueue(new DomainEvent(DomainEventType.ActivityStarted, _locationId, characterId, ropeWar.ActivityId, ActivityType.RopeWar));
            }
        }

        public void JoinRopeWarContest(string characterId, string ropeWarId, RopeWarSide side)
        {
            ValidateCharacterIsHere(characterId);
            ValidateCharacterIsNotInAnyActivity(characterId);

            var ropeWar = GetRopeWarById(ropeWarId);
            ropeWar.Join(characterId, side);
        }

        public string ProposeRopeWarContest(string characterId, string name, long bet, RopeWarSide side)
        {
            ValidateCharacterIsHere(characterId);
            ValidateCharacterIsNotInAnyActivity(characterId);

            var activityId = Guid.NewGuid().ToString();

            var ropeWar = new RopeWarActivity(activityId, name, characterId, bet);
            _activities.Add(ropeWar);

            ropeWar.Join(characterId, side);
            return activityId;
        }

        public void LeaveRopeWarContest(string characterId)
        {
            ValidateCharacterIsHere(characterId);

            var ropeWar = GetRopeWarFor(characterId);
            ropeWar.Leave(characterId);
        }

        public void SwitchSidesInRopeWar(string characterId)
        {
            ValidateCharacterIsHere(characterId);

            var ropeWar = GetRopeWarFor(characterId);
            ropeWar.SwitchSides(characterId);
        }

        public void AddCharacter(string characterId)
        {
            if (_characters.Contains(characterId))
                throw new InvalidOperationException("Character is already on this location.");

            // TODO: !!! validate somehow that character is NOT at ANY other location (location-less).

            _characters.Add(characterId);
        }

        public void RemoveCharacter(string characterId)
        {
            ValidateCharacterIsHere(characterId);

            if (IsCharacterInActivity(characterId))
                throw new InvalidOperationException("Character is participating in an activity. Cannot leave location.");

            // TODO: When moving from one location to another we need to do a transaction. Consider having a separate "Character" aggregate that will have locationId set on it.
            _characters.Remove(characterId);
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

        private RopeWarActivity GetRopeWarById(string ropeWarId)
        {
            var ropeWar = RopeWarActivities.SingleOrDefault(a => a.ActivityId == ropeWarId);
            if (ropeWar == null)
                throw new InvalidOperationException("RopeWarContest with this ID does not exist at this location.");

            return ropeWar;
        }

        private RopeWarActivity GetRopeWarFor(string characterId)
        {
            var ropeWar = RopeWarActivities.FirstOrDefault(rw => rw.HasParticipant(characterId));
            if (ropeWar == null)
                throw new InvalidOperationException("RopeWar for this character is not found.");

            return ropeWar;
        }

        private void ValidateCharacterIsHere(string characterId)
        {
            if (!_characters.Contains(characterId))
                throw new InvalidOperationException("Character is not at this location.");
        }

        private void ValidateCharacterIsNotInAnyActivity(string characterId)
        {
            // TODO: Think about making a Character entity that would have CurrentActivity property.
            if (IsCharacterInActivity(characterId))
                throw new InvalidOperationException("Character is already participating in an activity.");
        }

        private bool IsCharacterInActivity(string characterId)
        {
            return _activities.Any(activity => activity.HasParticipant(characterId));
        }
    }
#pragma warning restore IDE0032 // Use auto property
}
