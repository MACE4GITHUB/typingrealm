using TypingRealm.Client.Output;
using TypingRealm.Client.Typing;

namespace TypingRealm.Client.World
{
    public sealed class WorldPrinter : IPrinter<WorldScreenState>
    {
        private readonly IOutput _output;
        private readonly ITyperPool _typerPool;

        public WorldPrinter(
            IOutput output,
            ITyperPool typerPool)
        {
            _output = output;
            _typerPool = typerPool;
        }

        public void Print(WorldScreenState state)
        {
            if (state == null)
            {
                _output.WriteLine("LOADING...");
                return;
            }

            _output.WriteLine("World:");
            _output.WriteLine();
            _output.Write("Your current location: ");
            _output.WriteLine(state.CurrentLocation.Name);
            _output.WriteLine(state.CurrentLocation.Description);
            _output.WriteLine();

            _output.Write("Disconnect -    ");
            // TODO: Don't use ! operator.
            _output.Write(_typerPool.GetByKey("disconnect")!);
        }
    }
}
