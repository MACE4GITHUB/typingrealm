using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var userSessions = await IncludeAllChildren(Context.UserSession)
                .Where(us => us.UserId == userId)
                .AsAsyncEnumerable()
                .Select(us =>
                {
                    var state = us.ToState();
                    var userSession = UserSession.FromState(state);

                    return userSession;
                }).ToListAsync().ConfigureAwait(false);

            foreach (var userSession in userSessions)
            {
                yield return userSession;
            }
        }

        public async ValueTask<IEnumerable<UserSession>> FindAllForUserAsync(string userId)
        {
            // TODO: Use async api.
            return IncludeAllChildren(Context.UserSession)
                .AsEnumerable()
                .Select(x =>
                {
                    var state = x.ToState();
                    return UserSession.FromState(state);
                })
                .ToList();
        }

        public async ValueTask<IEnumerable<UserSession>> FindAllForUserFromTypingResultsAsync(string userId, DateTime fromTypingResultUtc)
        {
            // TODO: Use async api.
            return IncludeAllChildren(Context.UserSession)
                .Where(x => x.TextTypingResults.Any(y => y.SubmittedResultsUtc > fromTypingResultUtc))
                .AsEnumerable()
                .Select(x =>
                {
                    var state = x.ToState();
                    return UserSession.FromState(state);
                })
                .ToList();
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
