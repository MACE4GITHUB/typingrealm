using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Profiles.Api.Resources;

namespace TypingRealm.Profiles.Api.Client
{
    public interface IActivitiesClient
    {
        ValueTask<ActivityResource?> GetCurrentActivityAsync(string characterId, CancellationToken cancellationToken);
        ValueTask StartActivityAsync(ActivityResource activityResource, CancellationToken cancellationToken);
        ValueTask FinishActivityAsync(string activityId, CancellationToken cancellationToken);
    }
}
