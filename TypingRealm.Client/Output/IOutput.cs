using TypingRealm.Client.Typing;

namespace TypingRealm.Client.Output
{
    public interface IOutput
    {
        /// <summary>
        /// Clears output.
        /// </summary>
        void Clear();

        /// <summary>
        /// Writes an empty line (or appends a newline after a written line).
        /// </summary>
        void WriteLine();

        /// <summary>
        /// Writes a value without a new line at the end.
        /// </summary>
        /// <param name="value">Value to write.</param>
        void Write(string value);

        /// <summary>
        /// Writes a typer without a new line at the end.
        /// </summary>
        /// <param name="typerInformation">Typer to write.</param>
        void Write(ITyperInformation typerInformation);

        /// <summary>
        /// Writes input field state.
        /// </summary>
        void Write(IInputComponent inputComponent);

        /// <summary>
        /// Finalizes screen: writes empty lines at the end etc, so everything is beautiful.
        /// </summary>
        void FinalizeScreen();
    }
}
