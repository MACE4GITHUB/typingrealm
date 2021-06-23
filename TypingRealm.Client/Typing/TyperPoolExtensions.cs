using System;

namespace TypingRealm.Client.Typing
{
    public static class TyperPoolExtensions
    {
        public static (Typer Typer, string id) MakeUniqueTyper(this ITyperPool typerPool)
        {
            var id = Guid.NewGuid().ToString();
            var typer = typerPool.MakeUniqueTyper(id);

            return (typer, id);
        }
    }
}
