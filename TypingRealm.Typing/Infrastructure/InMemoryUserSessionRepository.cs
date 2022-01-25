using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TypingRealm.Typing.Framework;

namespace TypingRealm.Typing.Infrastructure;

public sealed class InMemoryUserSessionRepository : StateBasedRepository<UserSession, UserSession.State>, IUserSessionRepository
{
    public InMemoryUserSessionRepository() : base(new InMemoryRepository<UserSession.State>()) { }

    public IAsyncEnumerable<UserSession> FindAllForUser(string userId)
        => LoadAllAsync(x => x.UserId == userId);

    protected override UserSession CreateFromState(UserSession.State state) => UserSession.FromState(state);
    protected override UserSession.State GetFromEntity(UserSession entity) => entity.GetState();

    public async ValueTask<IEnumerable<UserSession>> FindAllForUserFromTypingResultsAsync(string userId, DateTime fromTypingResultUtc)
    {
        var items = new List<UserSession>();
        await foreach (var item in LoadAllAsync(x => x.TextTypingResults.Any(ttr => ttr.SubmittedResultsUtc > fromTypingResultUtc)))
        {
            items.Add(item);
        }

        return items;
    }

    public async ValueTask<IEnumerable<UserSession>> FindAllForUserAsync(string userId)
    {
        var items = new List<UserSession>();
        await foreach (var item in LoadAllAsync(x => true))
        {
            items.Add(item);
        }

        return items;
    }
}
