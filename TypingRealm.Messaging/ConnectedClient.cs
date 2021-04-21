using System;
using System.Collections.Generic;
using System.Linq;
using TypingRealm.Messaging.Updating;

namespace TypingRealm.Messaging
{
    /// <summary>
    /// Every live connection has single ConnectedClient instance associated with it.
    /// Changing client's Group will schedule update for all the clients that
    /// are currently in both previous and new group of this client.
    /// </summary>
    // TODO: Cover this class thoroughly with unit tests.
    // Test multi-group support and changes related to single-group field.
    public sealed class ConnectedClient
    {
        private readonly IUpdateDetector _updateDetector;
        private string? _singleGroup;
        private readonly HashSet<string> _groups
            = new HashSet<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedClient"/> class.
        /// Sets the single group of the client.
        /// </summary>
        /// <param name="clientId">Unique connected client identity. There
        /// should be no any clients connected at the same time with the same
        /// identity.</param>
        /// <param name="connection">Client's connection.</param>
        /// <param name="updateDetector">Update detector for marking groups for
        /// update when the group changes.</param>
        /// <param name="group">Messaging group where the client is placed initially.</param>
        public ConnectedClient(
            string clientId,
            IConnection connection,
            IUpdateDetector updateDetector,
            string group)
            : this(clientId, connection, updateDetector)
        {
            _singleGroup = group;
        }

        public ConnectedClient(
            string clientId,
            IConnection connection,
            IUpdateDetector updateDetector,
            IEnumerable<string> groups)
            : this(clientId, connection, updateDetector)
        {
            _groups = new HashSet<string>(groups);
        }

        public ConnectedClient(
            string clientId,
            IConnection connection,
            IUpdateDetector updateDetector)
        {
            ClientId = clientId;
            Connection = connection;
            _updateDetector = updateDetector;
        }

        /// <summary>
        /// Gets unique connected client identity.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Gets client's connection.
        /// </summary>
        public IConnection Connection { get; }

        /// <summary>
        /// Gets or sets messaging group where the client belongs to. Marks
        /// update detector for changes so that both previous and new groups are
        /// updated on the next update cycle.
        /// </summary>
        public string Group
        {
            get => _singleGroup ?? throw new InvalidOperationException("Single group has not been set.");
            set
            {
                if (_singleGroup == value)
                    return;

                if (_singleGroup != null)
                    _updateDetector.MarkForUpdate(_singleGroup);

                _singleGroup = value;
                _updateDetector.MarkForUpdate(_singleGroup);
            }
        }

        public IEnumerable<string> Groups => _singleGroup == null
            ? _groups
            : _groups.Append(_singleGroup);

        public void AddToGroup(string group)
        {
            if (_groups.Contains(group))
                return;

            _groups.Add(group);
            _updateDetector.MarkForUpdate(group);
        }

        public void RemoveFromGroup(string group)
        {
            if (!_groups.Contains(group))
                return;

            _groups.Remove(group);
            _updateDetector.MarkForUpdate(group);
        }
    }
}
