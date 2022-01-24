using System.Linq;

namespace TypingRealm.Typing.Infrastructure.DataAccess.Repositories;

public sealed class TextRepository
    : Repository<Text, Entities.Text>, ITextRepository
{
    public TextRepository(DataContext context) : base(context) { }

    protected override IQueryable<Entities.Text> IncludeAllChildren(
        IQueryable<Entities.Text> data)
    {
        return data;
    }

    protected override Entities.Text ToDbo(Text entity)
    {
        var state = entity.GetState();
        return Entities.Text.ToDbo(state);
    }

    protected override Text ToEntity(Entities.Text dbo)
    {
        var state = dbo.ToState();
        return Text.FromState(state);
    }
}
