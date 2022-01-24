using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TypingRealm.Typing.Infrastructure.DataAccess.Entities;

#pragma warning disable CS8618
[Index(nameof(UserId))]
[Index(nameof(CreatedUtc), nameof(UserTimeZoneOffsetMinutes))]
[Index(nameof(TypingSessionId))]
public class UserSession : IDbo<UserSession>
{
    [Key]
    public string Id { get; set; }

    public string UserId { get; set; }
    public DateTime CreatedUtc { get; set; }
    public int UserTimeZoneOffsetMinutes { get; set; }
    public virtual ICollection<TextTypingResult> TextTypingResults { get; set; }

    public string TypingSessionId { get; set; }
    public TypingSession TypingSession { get; set; }

    public static UserSession ToDbo(Typing.UserSession.State state)
    {
        return new UserSession
        {
            Id = state.UserSessionId,
            UserId = state.UserId,
            CreatedUtc = state.CreatedUtc,
            UserTimeZoneOffsetMinutes = state.UserTimeZoneOffsetMinutes,
            TextTypingResults = state.TextTypingResults
                .Select(ttr => TextTypingResult.ToDbo(ttr, state.UserSessionId))
                .ToList(),
            TypingSessionId = state.TypingSessionId
        };
    }

    public Typing.UserSession.State ToState()
    {
        return new Typing.UserSession.State(
            Id,
            UserId,
            TypingSessionId,
            CreatedUtc,
            UserTimeZoneOffsetMinutes,
            TextTypingResults.Select(ttr => ttr.ToState())
                .ToList());
    }

    public void MergeFrom(UserSession from)
    {
        if (UserId != from.UserId)
            throw new InvalidOperationException("UserSession UserId is immutable.");

        if (CreatedUtc != from.CreatedUtc)
            throw new InvalidOperationException("UserSession CreatedUtc is immutable.");

        if (UserTimeZoneOffsetMinutes != from.UserTimeZoneOffsetMinutes)
            throw new InvalidOperationException("UserSession UserTimeZoneOffsetMinutes is immutable");

        if (TypingSessionId != from.TypingSessionId)
            throw new InvalidOperationException("UserSession TypingSessionId is immutable.");

        foreach (var result in from.TextTypingResults)
        {
            var existing = TextTypingResults.FirstOrDefault(t => t.Id == result.Id);
            if (existing != null)
            {
                // TODO: Check that if any of the fields differ - throw.
                continue;
            }

            TextTypingResults.Add(result);
        }
    }
}
#pragma warning restore CS8618
