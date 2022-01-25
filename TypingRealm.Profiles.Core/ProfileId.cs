using System;
using TypingRealm.Common;

namespace TypingRealm.Profiles;

/// <summary>
/// Profile Identity. Contains authenticated User's Identity or hardcoded
/// values: "Service" for service tokens, "Anonymous" for non-authenticated
/// requests to non-protected endpoints.
/// </summary>
public sealed class ProfileId : Identity
{
    public static readonly string ServiceUserId = "Service";
    public static readonly string AnonymousUserId = "Anonymous";

    private ProfileId(string value) : base(value)
    {
    }

    public static ProfileId ForUser(string userId)
    {
        ArgumentNullException.ThrowIfNull(userId);

        if (userId.ToUpperInvariant() == ServiceUserId.ToUpperInvariant()
            || userId.ToUpperInvariant() == AnonymousUserId.ToUpperInvariant())
        {
            throw new ArgumentException("UserId cannot have these values: service, anonymous.");
        }

        return new(userId);
    }

    public static ProfileId ForService() => new(ServiceUserId);
    public static ProfileId Anonymous() => new(AnonymousUserId);
}
