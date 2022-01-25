using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Combat.Messages;
using TypingRealm.Messaging;
using TypingRealm.Messaging.Connecting;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Combat;

public sealed class EngageConnectionInitializer : IConnectionInitializer
{
    private readonly ICombatRoomStore _combatRoomStore;
    private readonly IUpdateDetector _updateDetector;

    public EngageConnectionInitializer(
        ICombatRoomStore combatRoomStore,
        IUpdateDetector updateDetector)
    {
        _combatRoomStore = combatRoomStore;
        _updateDetector = updateDetector;
    }

    public async ValueTask<ConnectedClient> ConnectAsync(IConnection connection, CancellationToken cancellationToken)
    {
        if (!(await connection.ReceiveAsync(cancellationToken).ConfigureAwait(false) is Engage message))
            throw new InvalidOperationException("First message is an invalid Engage message.");

        var existingRoom = _combatRoomStore.FindInBattle(message.PlayerId);
        if (existingRoom != null && message.CombatRoomId != existingRoom.CombatRoomId)
        {
            throw new InvalidOperationException("The player is already in battle in another room.");
        }

        _combatRoomStore.FindOrCreate(message.CombatRoomId, message.PlayerId);

        return new ConnectedClient(message.PlayerId, connection, _updateDetector, message.CombatRoomId);
    }
}
