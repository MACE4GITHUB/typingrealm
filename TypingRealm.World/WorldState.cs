using System.Collections.Generic;
using TypingRealm.Messaging;
using TypingRealm.Profiles.Activities;
using TypingRealm.World.Activities;

namespace TypingRealm.World
{
#pragma warning disable CS8618
    [Message]
    public sealed class WorldState
    {
        public string LocationId { get; set; }
        public List<string> Locations { get; set; }
        public List<ActivityType> AllowedActivityTypes { get; set; } = new List<ActivityType>();
        public List<string> Characters { get; set; }

        public List<RopeWarActivityState> RopeWarActivities { get; set; } = new List<RopeWarActivityState>();
    }
#pragma warning restore CS8618
}
