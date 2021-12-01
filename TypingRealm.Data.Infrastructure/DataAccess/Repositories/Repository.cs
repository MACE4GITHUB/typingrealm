using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TypingRealm.Typing.Framework;

namespace TypingRealm.Data.Infrastructure.DataAccess.Repositories
{
    public abstract class Repository<TEntity, TDbo>
        where TDbo : class, IDbo<TDbo>
        where TEntity : class, IIdentifiable
    {
        protected Repository(DataContext context)
        {
            Context = context;
        }

        protected DataContext Context { get; }

        public async ValueTask<TEntity?> FindAsync(string id)
        {
            var dbo = await IncludeAllChildren(Context.Set<TDbo>())
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (dbo == null)
                return null;

            return ToEntity(dbo);
        }

        public ValueTask<string> NextIdAsync()
        {
            return new ValueTask<string>(Guid.NewGuid().ToString());
        }

        public async ValueTask SaveAsync(TEntity entity)
        {
            var dbo = ToDbo(entity);
            var existing = await IncludeAllChildren(Context.Set<TDbo>())
                .FirstOrDefaultAsync(x => x.Id == entity.Id)
                .ConfigureAwait(false);

            if (existing == null)
            {
                await CreateNewAsync(dbo)
                    .ConfigureAwait(false);

                return;
            }

            await MergeAsync(dbo, existing)
                .ConfigureAwait(false);
        }

        private async ValueTask CreateNewAsync(TDbo dbo)
        {
            await Context.Set<TDbo>().AddAsync(dbo)
                .ConfigureAwait(false);

            await Context.SaveChangesAsync()
                .ConfigureAwait(false);
        }

        private async ValueTask MergeAsync(TDbo from, TDbo toExisting)
        {
            toExisting.MergeFrom(from);

            await Context.SaveChangesAsync()
                .ConfigureAwait(false);
        }

        protected abstract IQueryable<TDbo> IncludeAllChildren(IQueryable<TDbo> data);
        protected abstract TEntity ToEntity(TDbo dbo);
        protected abstract TDbo ToDbo(TEntity entity);
    }
}
