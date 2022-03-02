using System.Collections.Generic;
using TypingRealm.Messaging;

namespace TypingRealm.TypingDuels;

#pragma warning disable CS8618
[Message]
public sealed class Update
{
    public IEnumerable<Typed> Progresses { get; set; }
}
#pragma warning restore CS8618
