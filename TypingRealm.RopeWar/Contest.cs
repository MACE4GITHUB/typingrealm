using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingRealm.RopeWar
{
    public sealed class Contest
    {
#pragma warning disable CS8618
        public sealed class Data
        {
            public string ContestId { get; set; }
            public Dictionary<Side, List<string>> Contestants { get; set; }
            public int Progress { get; set; }
            public bool HasStarted { get; set; }
            public bool HasEnded { get; set; }
        }
#pragma warning restore CS8618

        private readonly string _contestId;
        private readonly Dictionary<Side, List<string>> _contestants = new Dictionary<Side, List<string>>
        {
            [Side.Left] = new List<string>(),
            [Side.Right] = new List<string>()
        };

        private int _progress;
        private bool _hasStarted;
        private bool _hasEnded;

        private Contest(Data data)
        {
            _contestId = data.ContestId;
            _contestants[Side.Left] = data.Contestants[Side.Left].ToList();
            _contestants[Side.Right] = data.Contestants[Side.Right].ToList();
            _progress = data.Progress;
            _hasStarted = data.HasStarted;
            _hasEnded = data.HasEnded;
        }

        public Contest(string contestId)
        {
            _contestId = contestId;
            _contestants = new Dictionary<Side, List<string>>
            {
                [Side.Left] = new List<string>(),
                [Side.Right] = new List<string>()
            };
        }

        public static Contest FromData(Data data)
        {
            return new Contest(data);
        }

        public Data GetData()
        {
            return new Data
            {
                ContestId = _contestId,
                Contestants = _contestants.ToDictionary(x => x.Key, x => x.Value.ToList()),
                Progress = _progress,
                HasStarted = _hasStarted,
                HasEnded = _hasEnded
            };
        }

        public void Join(string contestantId, Side side)
        {
            if (_hasStarted)
                throw new InvalidOperationException($"Contest {_contestId} has already started. Cannot join or change side.");

            if (_contestants[side].Contains(contestantId))
                throw new InvalidOperationException($"Contestant {contestantId} has already joined this contest to {side} side. Cannot join the same side again.");

            foreach (var contestantList in _contestants.Values)
            {
                contestantList.Remove(contestantId);
            }

            _contestants[side].Add(contestantId);
        }

        public void Start()
        {
            if (_hasStarted)
                throw new InvalidOperationException($"Contest {_contestId} has already started.");

            if (_contestants.Any(x => x.Value.Count == 0))
                throw new InvalidOperationException($"Cannot start the contest {_contestId}. One or more sides don't have contestants.");

            _hasStarted = true;
        }

        public void PullRope(string contestantId, int distance)
        {
            if (!_hasStarted)
                throw new InvalidOperationException($"Cannot pull rope until contest {_contestId} has started.");

            if (_hasEnded)
                throw new InvalidOperationException($"Cannot pull rope after contest {_contestId} has ended.");

            if (!_contestants.Values.Any(x => x.Contains(contestantId)))
                throw new InvalidOperationException($"Contestant {contestantId} is not participating in contest {_contestId}.");

            if (_contestants[Side.Left].Contains(contestantId))
                _progress -= distance;

            if (_contestants[Side.Right].Contains(contestantId))
                _progress += distance;

            CheckIfEnded();
        }

        private void CheckIfEnded()
        {
            if (_progress < -100)
                _progress = -100;

            if (_progress > 100)
                _progress = 100;

            if (_progress == -100 || _progress == 100)
                _hasEnded = true;
        }
    }
}
