using System;

namespace TypingRealm.Messaging.Connecting
{
    public sealed class ClientAlreadyConnectedException : Exception
    {
        public ClientAlreadyConnectedException(ConnectedClient connectedClient)
            : base($"Client {connectedClient.ClientId} is already connected.")
        {
        }
    }
}
