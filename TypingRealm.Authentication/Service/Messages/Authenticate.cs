using TypingRealm.Messaging;

namespace TypingRealm.Authentication.Service.Messages
{
    [Message]
    public sealed class Authenticate
    {
#pragma warning disable CS8618
        public Authenticate() { }
#pragma warning restore CS8618
        public Authenticate(string accessToken)
        {
            AccessToken = accessToken;
        }

        public string AccessToken { get; set; }
    }
}
