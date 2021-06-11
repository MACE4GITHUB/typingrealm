using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.Profiles.Api.Client;

namespace TypingRealm.World.Layers
{
    public sealed class CharacterActivityStore : ICharacterActivityStore
    {
        private readonly ICharactersClient _charactersClient;
        private readonly IActivityStore _activityStore;

        public CharacterActivityStore(
            ICharactersClient charactersClient,
            IActivityStore activityStore)
        {
            _charactersClient = charactersClient;
            _activityStore = activityStore;
        }

        public async ValueTask<bool> CanPerformActionsInLayerAsync(string characterId, Layer layer, CancellationToken cancellationToken)
        {
            // Get already started activities.
            var activitiesIds = await _charactersClient.GetActivitiesAsync(characterId, cancellationToken)
                .ConfigureAwait(false);

            // For now whenever we already have any started activity - do not allow any actions in the whole World domain anymore.
            if (activitiesIds.Any())
                return false;

            var activities = await _activityStore.GetActivitiesForCharacterAsync(characterId, cancellationToken)
                .ConfigureAwait(false);

            return CanPerformActionsInLayer(activities, layer);
        }

        public ValueTask EnterActivityAsync(string characterId, string activityId, CancellationToken cancellationToken)
        {
            return _charactersClient.EnterActivityAsync(characterId, activityId, cancellationToken);
        }

        public ValueTask LeaveActivityAsync(string characterId, string activityId, CancellationToken cancellationToken)
        {
            return _charactersClient.LeaveActivityAsync(characterId, activityId, cancellationToken);
        }

        private static bool CanPerformActionsInLayer(Stack<Activity> activities, Layer layer)
        {
            if (activities.Count == 0)
            {
                // Player is in World, it has no removable activities.
                if (layer == Layer.World)
                    return true;

                return false;
            }

            var currentActivity = activities.Peek();

            // If activity is in progress - you can only talk to a different domain until activity is finished.
            // We cannot edit the activity anymore.
            if (currentActivity.IsInProgress)
                return false;

            // While activity didn't start yet - we can edit it's state (join another team, leave, vote to start).
            if (currentActivity.Layer == layer && !currentActivity.HasStarted)
                return true;

            // But we cannot do any actions from other layers - like moving to another location.
            // It will also return false if for some reason activity has been finished but you still have it as your current activity.
            return false;
        }
    }

    public enum Layer
    {
        World = 1,
        RopeWar = 2,
        Road = 3,
        Combat = 4
    }

    public interface ICharacterActivityStore
    {
        ValueTask<bool> CanPerformActionsInLayerAsync(string characterId, Layer layer, CancellationToken cancellationToken);
        ValueTask EnterActivityAsync(string characterId, string activityId, CancellationToken cancellationToken);
        ValueTask LeaveActivityAsync(string characterId, string activityId, CancellationToken cancellationToken);
    }

    public interface IActivityStore
    {
        ValueTask<Stack<Activity>> GetActivitiesForCharacterAsync(string characterId, CancellationToken cancellationToken);
    }

    public abstract class LayerHandler<TMessage> : IMessageHandler<TMessage>
    {
        private readonly Layer _layer;

        protected LayerHandler(ICharacterActivityStore characterActivityStore, Layer layer)
        {
            CharacterActivityStore = characterActivityStore;
            _layer = layer;
        }

        protected ICharacterActivityStore CharacterActivityStore { get; }

        public async ValueTask HandleAsync(ConnectedClient sender, TMessage message, CancellationToken cancellationToken)
        {
            var canPerformAction = await CharacterActivityStore.CanPerformActionsInLayerAsync(sender.ClientId, _layer, cancellationToken)
                .ConfigureAwait(false);

            if (!canPerformAction)
                throw new InvalidOperationException("Character cannot perform this action in current layer.");

            await HandleMessageAsync(sender, message, cancellationToken)
                .ConfigureAwait(false);
        }

        protected abstract ValueTask HandleMessageAsync(
            ConnectedClient sender,
            TMessage message,
            CancellationToken cancellationToken);
    }
}
