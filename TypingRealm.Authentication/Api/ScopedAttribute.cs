namespace TypingRealm.Authentication.Api;

public sealed class ScopedAttribute : TyrAuthorizeAttribute
{
    public ScopedAttribute(string scope) : base(scope)
    {
    }
}
