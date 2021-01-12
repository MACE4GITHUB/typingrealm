namespace TypingRealm.Client.Typing
{
    public interface ITextGenerator
    {
        /// <summary>
        /// Gets a random short word/phrase for navigation. Used for menus and
        /// roaming the world.
        /// </summary>
        // TODO: Consider making async.
        string GetPhrase();

        // TODO: Implement getting phrase by reference id (for actions).
    }
}
