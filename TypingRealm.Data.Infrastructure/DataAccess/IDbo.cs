namespace TypingRealm.Data.Infrastructure.DataAccess
{
    public interface IDbo<in T>
    {
        string Id { get; }
        void MergeFrom(T from);
    }
}
