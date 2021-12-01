using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Typing;

namespace TypingRealm.Data.Infrastructure.DataAccess.Repositories
{
    public sealed class UserSessionRepository
        : Repository<UserSession, Entities.UserSession>, IUserSessionRepository
    {
        public UserSessionRepository(DataContext context) : base(context) { }

        public async IAsyncEnumerable<UserSession> FindAllForUser(string userId)
        {
            // TODO: Include everything I need everywhere especially in merges.
            // Or enable lazy loading (might cause performance issues).
            var asyncEnumerable = Context.UserSession
                .Include(x => x.TextTypingResults)
                .ThenInclude(x => x.Events)
                .Include(x => x.TypingSession)
                .ThenInclude(x => x.Texts)
                .Where(us => us.UserId == userId)
                .AsAsyncEnumerable()
                .Select(us =>
                {
                    var state = us.ToState();
                    var userSession = UserSession.FromState(state);

                    return userSession;
                });

            // Unfortunately we need to materialize it here because in foreach we are querying database again.
            foreach (var item in await asyncEnumerable.ToListAsync().ConfigureAwait(false))
            {
                yield return item;
            }
        }

        protected override IQueryable<Entities.UserSession> IncludeAllChildren(
            IQueryable<Entities.UserSession> data)
        {
            return data.Include(x => x.TextTypingResults)
                .ThenInclude(x => x.Events);
        }

        protected override Entities.UserSession ToDbo(UserSession entity)
        {
            var state = entity.GetState();
            return Entities.UserSession.ToDbo(state);
        }

        protected override UserSession ToEntity(Entities.UserSession dbo)
        {
            var state = dbo.ToState();
            return UserSession.FromState(state);
        }

    }
}
