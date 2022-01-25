namespace TypingRealm.RopeWar;

public interface IContestStore
{
    Contest? Find(string contestId);
    Contest? FindActiveByContestantId(string contestantId);
    void Save(Contest contest);
}
