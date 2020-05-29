namespace TypingRealm.Messaging.Connections
{
    public static class MessageSenderExtensions
    {
        public static NotificatorConnection WithNotificator(
            this IMessageSender messageSender, Notificator notificator)
        {
            return new NotificatorConnection(messageSender, notificator);
        }
    }
}
