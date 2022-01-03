using System;
using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Messaging;
using TypingRealm.Profiles.Activities;
using TypingRealm.Profiles.Api.Client;

namespace TypingRealm.World.Layers
{
    public sealed class CharacterActivityStore : ICharacterActivityStore
    {
        private readonly IActivitiesClient _activitiesClient;
        private readonly IActivityStore _activityStore;

        public CharacterActivityStore(
            IActivitiesClient activitiesClient,
            IActivityStore activityStore)
        {
            _activitiesClient = activitiesClient;
            _activityStore = activityStore;
        }

        public async ValueTask<bool> CanPerformActionsInLayerAsync(string characterId, Layer layer, CancellationToken cancellationToken)
        {
            // Get already started activities.
            var currentActivity = await _activitiesClient.GetCurrentActivityAsync(characterId, cancellationToken)
                .ConfigureAwait(false);

            // For now whenever we already have any started activity - do not allow any actions in the whole World domain anymore.
            if (currentActivity != null)
                return false;

            var activity = _activityStore.GetCurrentCharacterActivityOrDefault(characterId);

            return CanPerformActionsInLayer(activity, layer);
        }

        private static bool CanPerformActionsInLayer(Activity? currentActivity, Layer layer)
        {
            if (currentActivity == null)
            {
                // Player is in World, it has no removable activities.
                if (layer == Layer.World)
                    return true;

                return false;
            }

            // If activity is in progress - you can only talk to a different domain until activity is finished.
            // We cannot edit the activity anymore.
            if (currentActivity.IsInProgress)
                return false;

            // While activity didn't start yet - we can edit it's state (join another team, leave, vote to start).
            if (currentActivity.Type.GetLayer() == layer && !currentActivity.HasStarted)
                return true;

            // But we cannot do any actions from other layers - like moving to another location.
            // It will also return false if for some reason activity has been finished but you still have it as your current activity.
            return false;
        }
    }

    public static class ActivityTypeExtensions
    {
        public static Layer GetLayer(this ActivityType activityType) => activityType switch
        {
            ActivityType.RopeWar => Layer.RopeWar,
            _ => throw new InvalidOperationException("Unknown activity type.")
        };
    }

    public enum Layer
    {
        Unspecified = 0,

        World = 1,
        RopeWar = 2,
        Road = 3,
        Combat = 4
    }

    public interface ICharacterActivityStore
    {
        ValueTask<bool> CanPerformActionsInLayerAsync(string characterId, Layer layer, CancellationToken cancellationToken);
    }

    public interface IActivityStore
    {
        Activity? GetCurrentCharacterActivityOrDefault(string characterId);
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
