namespace TypingRealm.Authentication.Api;

public sealed class SuperAdminScopedAttribute : TyrAuthorizeAttribute
{
    public SuperAdminScopedAttribute() : base(TyrScopes.SuperAdmin)
    {
    }
}
