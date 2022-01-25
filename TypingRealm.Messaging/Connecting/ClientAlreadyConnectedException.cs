using System;

namespace TypingRealm.Messaging.Connecting;

/// <summary>
/// This exception is thrown from <see cref="ConnectedClientStore"/> when we
/// try to add <see cref="ConnectedClient"/> that already exists in the store.
/// </summary>
public sealed class ClientAlreadyConnectedException : Exception
{
    public ClientAlreadyConnectedException(ConnectedClient connectedClient)
        : base($"Client {connectedClient.ClientId} is already connected.")
    {
    }
}
