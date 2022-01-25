using System.Threading;
using System.Threading.Tasks;
using TypingRealm.Data.Resources;

namespace TypingRealm.Data.Api.Client;

public interface ILocationsClient
{
    ValueTask<Location> GetLocationAsync(string locationId, CancellationToken cancellationToken);
}
