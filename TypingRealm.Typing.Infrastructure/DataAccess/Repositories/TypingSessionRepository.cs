using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TypingRealm.Typing.Infrastructure.DataAccess.Repositories;

public sealed class TypingSessionRepository
    : Repository<TypingSession, Entities.TypingSession>,
    ITypingSessionRepository
{
    public TypingSessionRepository(DataContext context) : base(context) { }

    protected override IQueryable<Entities.TypingSession> IncludeAllChildren(
        IQueryable<Entities.TypingSession> data)
    {
        return data.Include(x => x.Texts);
    }

    protected override Entities.TypingSession ToDbo(TypingSession entity)
    {
        var state = entity.GetState();
        return Entities.TypingSession.ToDbo(state);
    }

    protected override TypingSession ToEntity(Entities.TypingSession dbo)
    {
        var state = dbo.ToState();
        return TypingSession.FromState(state);
    }
}
