using System.Collections.Generic;
using TypingRealm.Messaging;

namespace TypingRealm.Domain.Messages
{
    [Message]
    public sealed class Update
    {
#pragma warning disable CS8618
        public Update() { }
#pragma warning restore CS8618
        public Update(string locationId, IEnumerable<string> visiblePlayers)
            => (LocationId, VisiblePlayers) = (locationId, visiblePlayers);

        public string LocationId { get; set; }
        public IEnumerable<string> VisiblePlayers { get; set; }
    }
}
