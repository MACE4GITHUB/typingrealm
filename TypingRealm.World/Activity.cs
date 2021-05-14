#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace TypingRealm.World
{
    public abstract class Activity
    {
        public string Name { get; set; }
        public string CreatorId { get; set; }
        public string ActivityId { get; set; }

        public bool HasStarted { get; set; }
        public bool HasFinished { get; set; }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
