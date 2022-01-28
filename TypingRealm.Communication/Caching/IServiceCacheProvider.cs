using System.Threading.Tasks;

namespace TypingRealm.Communication;

public interface IServiceCacheProvider
{
    ValueTask<ITyrCache> GetServiceCacheAsync(string keyPrefix = "");
}
