namespace TypingRealm.RopeWar
{
    public interface IContestStore
    {
        Contest? Find(string contestId);
        Contest? FindByContestantId(string contestantId);
        void Save(Contest contest);
    }
}
