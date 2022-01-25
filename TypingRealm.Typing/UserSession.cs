using System;
using System.Collections.Generic;
using System.Linq;
using TypingRealm.Typing.Framework;

namespace TypingRealm.Typing;

/// <summary>
/// Aggregate root.
/// </summary>
public sealed class UserSession : IIdentifiable
{
    public sealed record State(
        string UserSessionId,
        string UserId,
        string TypingSessionId,
        DateTime CreatedUtc,
        int UserTimeZoneOffsetMinutes,
        List<TextTypingResult> TextTypingResults) : IIdentifiable
    {
        string IIdentifiable.Id => UserSessionId;
    }

    private readonly State _state;

    public UserSession(
        string userSessionId,
        string userId,
        string typingSessionId,
        DateTime createdUtc,
        int userTimeZoneOffsetMinutes)
    {
        // TODO: Validate.

        _state = new State(userSessionId, userId, typingSessionId, createdUtc, userTimeZoneOffsetMinutes, new List<TextTypingResult>());
    }

    string IIdentifiable.Id => _state.UserSessionId;

    #region State

    private UserSession(State state)
    {
        // TODO: Validate.

        _state = state with
        {
            TextTypingResults = new List<TextTypingResult>(state.TextTypingResults)
        };
    }

    public static UserSession FromState(State state) => new UserSession(state);

    public State GetState() => _state with
    {
        TextTypingResults = new List<TextTypingResult>(_state.TextTypingResults)
    };

    #endregion

    public string TypingSessionId => _state.TypingSessionId;

    public IEnumerable<TextTypingResult> GetTextTypingResults() => _state.TextTypingResults.ToList();

    public void LogResult(TextTypingResult textTypingResult)
    {
        // TODO: Validate. (results should follow each other with max allowed pause, etc)
        // Consider allowing logging different results from different texts: like you can type 6th text
        // in the beginning, and then type 2nd text two times, everything will be logged.

        _state.TextTypingResults.Add(textTypingResult);
    }
}
