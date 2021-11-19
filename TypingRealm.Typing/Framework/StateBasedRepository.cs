using System.Threading.Tasks;

namespace TypingRealm.Typing.Framework
{
    public abstract class StateBasedRepository<TEntity, TState> : IRepository<TEntity>
        where TEntity : class
    {
        private readonly IRepository<TState> _repository;

        protected StateBasedRepository(IRepository<TState> repository)
        {
            _repository = repository;
        }

        protected abstract TEntity CreateFromState(TState state);
        protected abstract TState GetFromEntity(TEntity entity);

        public async ValueTask<TEntity?> FindAsync(string key)
        {
            var state = await _repository.FindAsync(key)
                .ConfigureAwait(false);

            if (state == null)
                return null;

            return CreateFromState(state);
        }

        public ValueTask<string> NextIdAsync()
        {
            return _repository.NextIdAsync();
        }

        public ValueTask SaveAsync(TEntity entity)
        {
            var state = GetFromEntity(entity);

            return _repository.SaveAsync(state);
        }
    }
}
