namespace TypingRealm.Authentication.ConsoleClient;

internal class PkceClient
{
    private string _issuer;
    private string _pkceClientId;

    public PkceClient(string issuer, string pkceClientId)
    {
        _issuer = issuer;
        _pkceClientId = pkceClientId;
    }
}
