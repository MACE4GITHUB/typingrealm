using System;
using System.Threading.Tasks;

namespace TypingRealm.Typing
{
    /// <summary>
    /// Validates the text typing result and adds it to user session if
    /// everything is valid.
    /// </summary>
    public interface ITypingResultProcessor
    {
        /// <summary>
        /// Validates the text typing result and adds it to user session if
        /// everything is valid.
        /// </summary>
        /// <param name="userSessionId">Active user session.</param>
        /// <param name="textTypingResult">Result of typing one text.</param>
        ValueTask AddTypingResultAsync(string userSessionId, TextTypingResult textTypingResult);
    }

    public sealed class TypingResultProcessor : ITypingResultProcessor
    {
        private readonly ITextTypingResultValidator _textTypingResultValidator;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly ITypingSessionRepository _typingSessionRepository;
        private readonly ITextRepository _textRepository;

        public TypingResultProcessor(
            ITextTypingResultValidator textTypingResultValidator,
            IUserSessionRepository userSessionRepository,
            ITypingSessionRepository typingSessionRepository,
            ITextRepository textRepository)
        {
            _textTypingResultValidator = textTypingResultValidator;
            _userSessionRepository = userSessionRepository;
            _typingSessionRepository = typingSessionRepository;
            _textRepository = textRepository;
        }

        public async ValueTask AddTypingResultAsync(string userSessionId, TextTypingResult textTypingResult)
        {
            var userSession = await _userSessionRepository.FindAsync(userSessionId)
                .ConfigureAwait(false);

            if (userSession == null)
                throw new InvalidOperationException("User session does not exist.");

            // TODO: Validate that it is currently active session.

            var typingSession = await _typingSessionRepository.FindAsync(userSession.TypingSessionId)
                .ConfigureAwait(false);

            if (typingSession == null)
                throw new InvalidOperationException("Typing session does not exist.");

            // TODO: Validate that it is type-able (active, etc).

            var typingSessionText = typingSession.GetTypingSessionTextAtIndexOrDefault(
                textTypingResult.TypingSessionTextIndex);

            if (typingSessionText == null)
                throw new InvalidOperationException("Typing session text with this index does not exist in typing session.");

            var text = await _textRepository.FindAsync(typingSessionText.TextId)
                .ConfigureAwait(false);

            if (text == null)
                throw new InvalidOperationException("Text with such ID does not exist.");

            if (typingSessionText.Value != text.Value)
                throw new InvalidOperationException("Typing session text value differs from the one from the text store. Corrupted state.");

            // If this doesn't throw - validation succeeded.
            await _textTypingResultValidator.ValidateAsync(text.Value, textTypingResult)
                .ConfigureAwait(false);

            userSession.LogResult(textTypingResult);
            await _userSessionRepository.SaveAsync(userSession)
                .ConfigureAwait(false);
        }
    }
}
