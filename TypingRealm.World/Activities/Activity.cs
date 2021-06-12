using System;
using TypingRealm.World.Activities;
using TypingRealm.World.Layers;

namespace TypingRealm.World
{
    public abstract class Activity
    {
        protected Activity(Layer layer, string activityId, ActivityType type, string name, string creatorId)
        {
            Layer = layer;
            ActivityId = activityId;
            Type = type;
            Name = name;
            CreatorId = creatorId;
        }

        public Layer Layer { get; }
        public ActivityType Type { get; }
        public string ActivityId { get; }
        public string Name { get; }
        public string CreatorId { get; }

        public bool HasStarted { get; private set; }
        public bool HasFinished { get; private set; }
        public bool CanEdit => !HasStarted && !HasFinished;

        public bool IsInProgress => HasStarted && !HasFinished;

        public abstract bool HasParticipant(string characterId);

        protected void Start()
        {
            if (HasStarted)
                throw new InvalidOperationException("Activity has already been started.");

            if (HasFinished)
                throw new InvalidOperationException("Activity has already been finished.");

            HasStarted = true;
        }

        /// <summary>
        /// Activity can be finished without starting - when we need to cancel it.
        /// </summary>
        protected void Finish()
        {
            if (HasFinished)
                throw new InvalidOperationException("Activity has already been finished.");

            HasFinished = true;
        }
    }
}
