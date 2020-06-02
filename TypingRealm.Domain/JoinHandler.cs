﻿using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Domain.Messages;
using TypingRealm.Messaging;

namespace TypingRealm.Domain
{
    public sealed class JoinHandler : IMessageHandler<Join>
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IPlayerFactory _playerFactory;

        public JoinHandler(
            IPlayerRepository playerRepository,
            IPlayerFactory playerFactory)
        {
            _playerRepository = playerRepository;
            _playerFactory = playerFactory;
        }

        public ValueTask HandleAsync(ConnectedClient sender, Join message, CancellationToken cancellationToken)
        {
            var joinedPlayer = _playerRepository.FindByClientId(sender.ClientId);
            if (joinedPlayer != null)
                throw new InvalidOperationException($"CLient {sender.ClientId} already has joined with player {joinedPlayer.PlayerId}.");

            var playerName = new PlayerName(message.PlayerId);
            var player = _playerFactory.CreateNew(playerName);

            _playerRepository.Save(sender.ClientId, player);
            sender.Group = player.GetUniquePlayerPosition();
            return default;
        }
    }
}
