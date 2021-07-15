using System;

namespace TypingRealm.Client.Typing
{
    public static class TyperPoolExtensions
    {
        public static Typer MakeUniqueTyper(this ITyperPool typerPool)
        {
            var id = Guid.NewGuid().ToString();
            var typer = typerPool.MakeUniqueTyper(id);
            typer.Id = id;

            return typer;
        }

        public static void RemoveTyper(this ITyperPool typerPool, string id)
        {
            var typer = typerPool.GetByKey(id);
            if (typer == null)
                return;

            typerPool.RemoveTyper(typer);
        }
    }
}
