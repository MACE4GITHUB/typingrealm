namespace TypingRealm.Messaging.Messages;

/// <summary>
/// Connect message is used to specify client identifier and optionally its
/// initial group during initial connection stage.
/// </summary>
[Message]
public sealed class Connect
{
    // TODO: !!! Validate this message's ClientId when handling Connect message.
    // Currently if Character authentication is disabled - any user with a valid token can send ANY client id.
    // Take TOKEN SUB claim by default instead of this message's property.
    // Possibly even rename this message to ConnectCharacter and only use it to connect characters in the game,
    // do not use it at all for connecting with token to some auxiliary realtime system.

    public const string DefaultGroup = "Lobby";

#pragma warning disable CS8618
    public Connect() { }
#pragma warning restore CS8618
    public Connect(string clientId) => ClientId = clientId;
    public Connect(string clientId, string group)
        => (ClientId, Group) = (clientId, group);

    /// <summary>
    /// Client identifier. It should be unique and no clients with the same
    /// identifier should be connected at the same time.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Initial group of connected client. If no group specified, default
    /// constant value "Lobby" will be used.
    /// </summary>
    public string Group { get; set; } = DefaultGroup;
}
