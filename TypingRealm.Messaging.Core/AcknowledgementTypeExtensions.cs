namespace TypingRealm.Messaging;

public static class AcknowledgementTypeExtensions
{
    public static bool IsAcknowledgementRequired(this AcknowledgementType acknowledgementType)
        => acknowledgementType != AcknowledgementType.None;
}
