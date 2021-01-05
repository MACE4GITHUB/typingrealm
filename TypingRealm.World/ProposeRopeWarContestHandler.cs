using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.Profiles.Api.Client;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace TypingRealm.World
{
    public sealed class ProposeRopeWarContestHandler : IMessageHandler<ProposeRopeWarContest>
    {
        private readonly ILocationStore _locationStore;
        private readonly ICharactersClient _charactersClient;

        public ProposeRopeWarContestHandler(ILocationStore locationStore, ICharactersClient charactersClient)
        {
            _locationStore = locationStore;
            _charactersClient = charactersClient;
        }

        public async ValueTask HandleAsync(ConnectedClient sender, ProposeRopeWarContest message, CancellationToken cancellationToken)
        {
            // TODO: Subtract "bet" from character money.

            var characterId = sender.ClientId;

            // TODO: Consider just using Group property to get location.
            var location = _locationStore.FindLocationForCharacter(sender.ClientId);
            if (location == null)
                throw new InvalidOperationException("Location does not exist.");

            if (location.LocationId != sender.Group)
                throw new InvalidOperationException("Synchronization mismatch.");

            if (!location.CanProposeRopeWar)
                throw new InvalidOperationException("Cannot propose ropewar here.");

            var activityId = Guid.NewGuid().ToString();

            // TODO: Maybe consider storing this info in WORLD domain (ALL CHARACTER INFO) for quick and easy access.
            // Or create a separate domain with faster access than profile API.
            await _charactersClient.EnterActivityAsync(characterId, activityId, cancellationToken)
                .ConfigureAwait(false);

            location.RopeWars.Add(new RopeWar
            {
                ActivityId = activityId,
                Bet = message.Bet,
                Name = message.Name,
                CreatorId = characterId,
                LeftSideParticipants = message.Side == RopeWarSide.Left ? new List<string>
                {
                    characterId
                } : new List<string>(),
                RightSideParticipants = message.Side == RopeWarSide.Right ? new List<string>
                {
                    characterId
                } : new List<string>()
            });
            _locationStore.Save(location);
        }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
