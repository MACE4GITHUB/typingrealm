using System;

namespace TypingRealm.World
{
    public abstract class Activity
    {
        protected Activity(string activityId, string name, string creatorId)
        {
            ActivityId = activityId;
            Name = name;
            CreatorId = creatorId;
        }

        public string ActivityId { get; }
        public string Name { get; }
        public string CreatorId { get; }

        public bool HasStarted { get; private set; }
        public bool HasFinished { get; private set; }

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
