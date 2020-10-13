using System;
using TypingRealm.Messaging;

namespace TypingRealm.Authentication
{
    public sealed class Authenticate : Message
    {
#pragma warning disable CS8618
        public Authenticate() { }
#pragma warning restore CS8618
        public Authenticate(string accessToken)
        {
            AccessToken = accessToken;
            MessageId = Guid.NewGuid().ToString();
        }

        public string AccessToken { get; set; }
    }
}
