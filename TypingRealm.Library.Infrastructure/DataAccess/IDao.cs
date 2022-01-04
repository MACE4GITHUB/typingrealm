namespace TypingRealm.Library.Infrastructure.DataAccess;

// TODO: Move this to some shared framework project and reuse it in other Infrastructure projects.
public interface IDao<in T>
{
    string Id { get; }
    void MergeFrom(T from);
}
