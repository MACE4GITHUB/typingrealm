using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TypingRealm.Typing.Infrastructure.DataAccess.Entities;

#pragma warning disable CS8618
[Index(nameof(TextId))]
[Index(nameof(IndexInTypingSession), nameof(TypingSessionId), IsUnique = true)]
public class TypingSessionText
{
    [Key]
    public long RowId { get; set; }

    public string TextId { get; set; }
    public Text Text { get; set; }

    public string Value { get; set; }

    public int IndexInTypingSession { get; set; }
    public string TypingSessionId { get; set; }
    public TypingSession TypingSession { get; set; }

    public static TypingSessionText ToDbo(Typing.TypingSessionText state, int indexInTypingSession, string typingSessionId)
    {
        return new TypingSessionText
        {
            RowId = 0,
            TextId = state.TextId,
            Value = state.Value,
            IndexInTypingSession = indexInTypingSession,
            TypingSessionId = typingSessionId
        };
    }

    public Typing.TypingSessionText ToState()
    {
        return new Typing.TypingSessionText(TextId, Value);
    }

    public void MergeFrom(TypingSessionText from)
    {
        _ = from;
        throw new InvalidOperationException("The TypingSessionText is immutable.");
    }
}
#pragma warning restore CS8618
