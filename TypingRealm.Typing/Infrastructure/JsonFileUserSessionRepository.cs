using System.Collections.Generic;
using TypingRealm.Typing.Framework;

namespace TypingRealm.Typing.Infrastructure
{
    public sealed class JsonFileUserSessionRepository : StateBasedRepository<UserSession, UserSession.State>, IUserSessionRepository
    {
        public JsonFileUserSessionRepository()
            : base(new JsonFileRepository<UserSession.State>("user-sessions.json")) { }

        public IAsyncEnumerable<UserSession> FindAllForUser(string userId)
            => LoadAllAsync(x => x.UserId == userId);

        protected override UserSession CreateFromState(UserSession.State state) => UserSession.FromState(state);
        protected override UserSession.State GetFromEntity(UserSession entity) => entity.GetState();
    }
}
