using System.Collections.Generic;
using TypingRealm.Messaging;

namespace TypingRealm.RopeWar
{
#pragma warning disable CS8618
    [Message]
    public sealed class JoinContest
    {
        public Side Side { get; set; }
    }

    [Message]
    public sealed class StartContest
    {
    }

    [Message]
    public sealed class PullRope
    {
        public int Distance { get; set; }
    }

    [Message]
    public sealed class ContestUpdate
    {
        // If progress is 0 - left team has won.
        // If progress is 100 - right team has won.
        public int Progress { get; set; }
        public List<ContestantUpdate> Contestants { get; set; }
        public bool HasStarted { get; set; }
        public bool HasEnded { get; set; }
    }

    // Dictionary serialization is not supported, so this crutch for now.
    [Message] // This is another crutch: internal classes for protobuf serialization.
    public sealed class ContestantUpdate
    {
        public string ContestantId { get; set; }
        public Side Side { get; set; }
    }
#pragma warning restore CS8618
}
