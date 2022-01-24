using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TypingRealm.Typing.Infrastructure.DataAccess.Entities;

#pragma warning disable CS8618
[Index(nameof(CreatedByUser))]
[Index(nameof(CreatedUtc))]
public class TypingSession : IDbo<TypingSession>
{
    [Key]
    public string Id { get; set; }

    public string CreatedByUser { get; set; }
    public DateTime CreatedUtc { get; set; }

    // Configuration fields here in flat properties.

    public virtual ICollection<TypingSessionText> Texts { get; set; }

    public static TypingSession ToDbo(Typing.TypingSession.State state)
    {
        return new TypingSession
        {
            Id = state.TypingSessionId,
            CreatedByUser = state.CreatedByUser,
            CreatedUtc = state.CreatedUtc,
            Texts = state.Texts
                .Select(t => TypingSessionText.ToDbo(t.Value, t.Key, state.TypingSessionId))
                .ToList()
        };
    }

    public Typing.TypingSession.State ToState()
    {
        return new Typing.TypingSession.State(
            Texts.ToDictionary(t => t.IndexInTypingSession, t => t.ToState()),
            Id,
            CreatedUtc,
            CreatedByUser,
            new TypingSessionConfiguration());
    }

    public void MergeFrom(TypingSession from)
    {
        if (CreatedByUser != from.CreatedByUser)
            CreatedByUser = from.CreatedByUser;

        if (CreatedUtc != from.CreatedUtc)
            CreatedUtc = from.CreatedUtc;

        foreach (var text in from.Texts)
        {
            var existing = Texts.FirstOrDefault(t => t.IndexInTypingSession == text.IndexInTypingSession);
            if (existing != null)
            {
                existing.MergeFrom(text);
                continue;
            }

            Texts.Add(text);
        }
    }
}
#pragma warning restore CS8618
