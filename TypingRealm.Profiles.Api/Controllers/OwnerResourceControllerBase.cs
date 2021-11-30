using System;
using TypingRealm.Hosting;

namespace TypingRealm.Profiles.Api.Controllers
{
    public abstract class OwnerResourceControllerBase<TEntity, TEntityResource> : TyrController
    {
        private readonly Func<TEntity, string> _entityToOwnerId;
        private readonly Func<TEntityResource, string> _entityResourceToOwnerId;

        protected OwnerResourceControllerBase(
            Func<TEntity, string> entityToOwnerId,
            Func<TEntityResource, string> entityResourceToOwnerId)
        {
            _entityToOwnerId = entityToOwnerId;
            _entityResourceToOwnerId = entityResourceToOwnerId;
        }

        protected bool IsOwner(TEntity resource)
        {
            return _entityToOwnerId(resource) == ProfileId;
        }

        protected bool IsOwner(TEntityResource entityResource)
        {
            return _entityResourceToOwnerId(entityResource) == ProfileId;
        }
    }
}
