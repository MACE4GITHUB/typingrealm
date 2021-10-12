namespace TypingRealm.Typing
{
    public sealed class KeyPressEvent
    {
        public KeyPressEvent(int index, string key, decimal delay)
        {
            Index = index;
            Key = key;
            Delay = delay;
        }

        public int Index { get; }
        public string Key { get; }
        public decimal Delay { get; }
    }
}
