namespace TypingRealm.Client.Interaction;

public sealed record ScreenDependencies<TManager, TPrinter, THandler>(
    TManager Manager,
    TPrinter Printer,
    THandler Handler);
