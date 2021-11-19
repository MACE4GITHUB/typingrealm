﻿using TypingRealm.Typing.Framework;

namespace TypingRealm.Typing.Infrastructure
{
    public sealed class InMemoryUserSessionRepository : StateBasedRepository<UserSession, UserSession.State>, IUserSessionRepository
    {
        public InMemoryUserSessionRepository() : base(new InMemoryRepository<UserSession.State>()) { }

        protected override UserSession CreateFromState(UserSession.State state) => UserSession.FromState(state);
        protected override UserSession.State GetFromEntity(UserSession entity) => entity.GetState();
    }


}
