using System.Collections.Generic;

namespace TypingRealm.Authentication;

public sealed record ClientCredentials(
    string ClientId, string ClientSecret, IEnumerable<string> Scopes);
