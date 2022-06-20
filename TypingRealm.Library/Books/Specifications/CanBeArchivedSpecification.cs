using System;
using System.Linq.Expressions;

namespace TypingRealm.Library.Books.Specifications;

public sealed class CanBeArchivedSpecification : Specification<Book.State>
{
    public override Expression<Func<Book.State, bool>> ToExpression()
    {
        return state => !state.IsArchived
            && state.ProcessingStatus != ProcessingStatus.Processing;
    }
}
