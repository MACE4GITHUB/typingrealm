using System;
using System.Collections.Generic;
using System.Linq;
using TypingRealm.Messaging;
using TypingRealm.Profiles.Activities;
using TypingRealm.World.Activities.RopeWar;

namespace TypingRealm.World.Activities;
#pragma warning disable CS8618
[Message]
public sealed class RopeWarActivityState
{
    public long Bet { get; set; }
    public List<string> LeftSideParticipants { get; set; } = new List<string>();
    public List<string> RightSideParticipants { get; set; } = new List<string>();

    public string ActivityId { get; set; }
}
#pragma warning restore CS8618

// TODO: Create CHARACTER AR and check it's activity. Cannot participate in activity when already in another activity.
// TODO: Generalize this: CANNOT join ACTIVITY when already in another activity.
// Should check if currently engaged in any other activity - then disallow joining another activity.
// Should subtract "bet" from character money. Or rather put that money on hold so nobony can spend more money from the account until the end of the ropewar contest.
public sealed class RopeWarActivity : Activity
{
    private readonly HashSet<string> _leftSideParticipants = new HashSet<string>();
    private readonly HashSet<string> _rightSideParticipants = new HashSet<string>();
    private readonly HashSet<string> _votesToStart = new HashSet<string>();

    public RopeWarActivity(
        string activityId,
        string name,
        string creatorId,
        long bet)
        : base(activityId, ActivityType.RopeWar, name, creatorId)
    {
        Bet = bet;
    }

    public long Bet { get; }
    public IEnumerable<string> LeftSideParticipants => _leftSideParticipants;
    public IEnumerable<string> RightSideParticipants => _rightSideParticipants;
    public IEnumerable<string> VotesToStart => _votesToStart;

    public override IEnumerable<string> GetParticipants()
    {
        return _leftSideParticipants.Concat(_rightSideParticipants);
    }

    public void VoteToStart(string characterId)
    {
        _votesToStart.Add(characterId);

        if (_leftSideParticipants.Concat(_rightSideParticipants).All(
            participant => _votesToStart.Contains(participant))
            && _leftSideParticipants.Any() && _rightSideParticipants.Any())
            Start();
    }

    public RopeWarActivityState GetState()
    {
        return new RopeWarActivityState
        {
            ActivityId = ActivityId,
            Bet = Bet,
            LeftSideParticipants = _leftSideParticipants.ToList(),
            RightSideParticipants = _rightSideParticipants.ToList()
        };
    }

    public void Join(string characterId, RopeWarSide side)
    {
        if (side == RopeWarSide.Left)
        {
            JoinToLeftSide(characterId);
            return;
        }

        if (side == RopeWarSide.Right)
        {
            JoinToRightSide(characterId);
            return;
        }

        throw new InvalidOperationException("Unknown side.");
    }

    public void JoinToLeftSide(string characterId)
    {
        if (_leftSideParticipants.Contains(characterId))
            throw new InvalidOperationException("Already joined to the left side.");

        _votesToStart.Clear();

        if (_rightSideParticipants.Contains(characterId))
            _rightSideParticipants.Remove(characterId);

        _leftSideParticipants.Add(characterId);
    }

    public void JoinToRightSide(string characterId)
    {
        if (_rightSideParticipants.Contains(characterId))
            throw new InvalidOperationException("Already joined to the right side.");

        _votesToStart.Clear();

        if (_leftSideParticipants.Contains(characterId))
            _leftSideParticipants.Remove(characterId);

        _rightSideParticipants.Add(characterId);
    }

    public void SwitchSides(string characterId)
    {
        if (_leftSideParticipants.Contains(characterId))
            JoinToRightSide(characterId);
        else
            JoinToLeftSide(characterId);
    }

    public void Leave(string characterId)
    {
        if (!_rightSideParticipants.Contains(characterId)
            && !_leftSideParticipants.Contains(characterId))
            throw new InvalidOperationException("Cannot leave because not partaking in this activity.");

        _votesToStart.Clear();

        _rightSideParticipants.Remove(characterId);
        _leftSideParticipants.Remove(characterId);
    }
}
