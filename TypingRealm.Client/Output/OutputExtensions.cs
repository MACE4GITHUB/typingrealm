using TypingRealm.Client.Typing;

namespace TypingRealm.Client.Output
{
    public static class OutputExtensions
    {
        public static void WriteLine(this IOutput output, string value)
        {
            output.Write(value);
            output.WriteLine();
        }

        public static void WriteLine(this IOutput output, Typer typer)
        {
            output.Write(typer);
            output.WriteLine();
        }
    }
}
