using System.Collections.Generic;

namespace TypingRealm.Client.Typing
{
    public sealed class ComponentPool
    {
        private readonly ITyperPool _focusTyperPool;
        private readonly Dictionary<Typer, IInputComponent> _components
            = new Dictionary<Typer, IInputComponent>();

        public ComponentPool(ITyperPool focusTyperPool)
        {
            _focusTyperPool = focusTyperPool;
        }

        public IInputComponent? FocusedComponent { get; set; } // TODO: Remove public setter.

        public void TestTyped(Typer typer)
        {
            if (FocusedComponent != null)
                return;

            if (_components.ContainsKey(typer))
            {
                FocusedComponent = _components[typer];
                _components[typer].IsFocused = true;
            }
        }

        public InputComponent MakeInputComponent()
        {
            (var typer, _) = _focusTyperPool.MakeUniqueTyper();

            var component = new InputComponent(typer);
            _components.Add(typer, component);

            return component;
        }
    }
}
