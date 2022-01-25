using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Combat.Messages;
using TypingRealm.Messaging;

namespace TypingRealm.Combat;

public sealed class AttackedHandler : IMessageHandler<Attacked>
{
    private readonly ICombatRoomStore _combatRoomStore;

    public AttackedHandler(ICombatRoomStore combatRoomStore)
    {
        _combatRoomStore = combatRoomStore;
    }

    public ValueTask HandleAsync(ConnectedClient sender, Attacked message, CancellationToken cancellationToken)
    {
        var combatRoom = _combatRoomStore.FindInBattle(sender.ClientId);
        if (combatRoom == null)
            throw new InvalidOperationException("Player is not in combat.");

        if (combatRoom.CombatRoomId != sender.Group)
            throw new InvalidOperationException("Inconsistent data between client group and combat room.");

        var target = combatRoom.Players.FirstOrDefault(p => p.PlayerId == message.TargetId);
        if (target == null)
            throw new InvalidOperationException("Target is not found in this combat room.");

        target.Health -= message.BodyPart switch
        {
            BodyPart.Head => 10,
            BodyPart.Body => 5,
            BodyPart.Legs => 7,
            _ => throw new InvalidOperationException("Unknown body part."),
        };

        if (target.Health <= 0)
            target.Health = 0;

        return default;
    }
}
