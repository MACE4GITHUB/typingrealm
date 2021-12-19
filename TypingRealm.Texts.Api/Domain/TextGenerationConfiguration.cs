namespace TypingRealm.Texts
{
    public sealed record TextGenerationConfiguration(
        int Length)
    {
        public string UniqueKey => $"length={Length}";
    }
}
