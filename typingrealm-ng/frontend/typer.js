/**
 * An abstraction around typing, any key processing event should trigger the
 * type(event) method. After typing is done, result read-only property can be
 * queried to get events data and other information to be uploaded to the server.
 */
export default class Typer {
    /** Initial text value that is needed to type. */
    #text;

    /** Events array, separate event for every key press. */
    #events = [];

    /**
     * When typing is successfully finished (with all mistakes corrected),
     * Typer serves as the data holder of events. No more actions can be performed then.
     */
    #isFinished = false;

    /**
     * Index of character in text that needs to be typed now. When #index === text length,
     * the typing is done (given there's no errors left that need correcting).
     */
    #index = 0;

    /** Text characters with their related typing data. */
    #characters = [];

    /**
     * Function that is called once Typer is finished (only once) passing current
     * Typer instance as a single argument.
     */
    #finishedCallback;

    /** The time at which the Typer instance was created (text is loaded by user). */
    #createdPerf;

    /** The timestamp at which the Typer instance was created (text is loaded by user). */
    #createdAt;

    /**
     * Creates a new instance of Typer.
     * @param {String} text Whole text that needs to be typed.
     * @param {Function} finishedCallback Callback that is called with current
     * Typer as an argument, once the Typer is finished. This is optional, if
     * not specified - nothing is called.
     */
    constructor(text, finishedCallback) {
        this.#text = text;
        this.#finishedCallback = finishedCallback;

        this.#text.split('').forEach(character => {
            const characterSpan = document.createElement('span');
            characterSpan.innerText = character;

            this.#characters.push({
                character: character,
                characterSpan: characterSpan,
                isTyped: false,
                wasTypedWithError: false,
                isTypedWithError: false
            });
        });

        this.#currentCharacter.characterSpan.classList.add('cursor');
        this.#createdPerf = performance.now();
        this.#createdAt = new Date();
    }

    /**
     * When typing is successfully finished (with all mistakes corrected),
     * Typer serves as the data holder of events. No more actions can be performed then.
     */
    get isFinished() { return this.#isFinished; }

    /** Span elements for all text characters. */
    get characterSpans() { return this.#characters.map(x => x.characterSpan); }

    /** Result of typing, including events array, separate event for every key press. */
    get result() {
        if (!this.#isFinished) {
            throw new Error('Typer is not finished yet.');
        }

        return {
            createdPerf: this.#createdPerf,
            createdAt: this.#createdAt,
            events: this.#events
        };
    }

    /** Gets current character information at current #index. */
    get #currentCharacter() { return this.#characters[this.#index]; }

    /**
     * Processes keyboard input. This method should be called as soon as
     * keyboard event occurs, and event.key should be passed here.
     * @param {KeyboardEvent} event KeyboardEvent that has occurred, supports
     * only 'keyup' and 'keydown' events.
     */
    type(event) {
        const perf = performance.now();

        let key = event.key;

        const eventType = event.type;

        if (eventType !== 'keydown' && eventType !== 'keyup') return; // Ignore other event types.
        if (this.#isFinished) return; // Don't do anything once typing has finished.

        this.#events.push({
            key: key === 'Shift' ? event.code : key,
            perf: perf,
            index: this.#index,
            eventType: eventType
        });

        // Releasing keys or pressing shift doesn't alter current typing state.
        if (eventType === 'keyup' || key === 'Shift') return;

        // If it is backspace - decrease index, move cursor back, clear typed state of last character.
        if (key === 'Backspace') {
            this.#handleBackspace();
            return;
        }

        // Do not process weird keys apart from alphabet & punctuation & spaces.
        if (!this.#isKeySymbol(event)) return;

        // Text character key was pressed.
        this.#handleCharacter(event.key);
    }

    /** Returns true when a character is a letter, number, punctuation or space. */
    #isKeySymbol(event) {
        return (event.keyCode >= 48 && event.keyCode <= 57) || (event.keyCode >= 65 && event.keyCode <= 90)
            || (event.keyCode == 32 /* space */) || (event.keyCode >= 106 && event.keyCode <= 111 /* signs */)
            || (event.keyCode >= 186 && event.keyCode <= 192 /* punctuation */) || (event.keyCode >= 219 && event.keyCode <= 222 /* quotes */);
    }

    #handleBackspace() {
        if (this.#index === 0) return; // Nothing to do when we are at the beginning of the text.

        // If all characters have been typed but errors remained, this would not exist, hence ? check.
        this.#currentCharacter?.characterSpan.classList.remove('cursor');
        this.#index--;
        this.#currentCharacter.characterSpan.classList.add('cursor');
        this.#currentCharacter.characterSpan.classList.remove('typed');
        this.#currentCharacter.isTyped = false;

        if (this.#currentCharacter.wasTypedWithError) {
            this.#currentCharacter.characterSpan.classList.remove('typed-error');
            this.#currentCharacter.characterSpan.classList.remove('typed-corrected');
            this.#currentCharacter.isTypedWithError = false;
        }
    }

    #handleCharacter(character) {
        // There are still errors left, please use backspace to get out of this situation :)
        if (this.#index === this.#text.length) return;

        this.#currentCharacter.isTyped = true;
        this.#currentCharacter.characterSpan.classList.remove('cursor');

        if (character === this.#currentCharacter.character) {
            this.#currentCharacter.characterSpan.classList.add('typed');
            if (this.#currentCharacter.wasTypedWithError) {
                this.#currentCharacter.characterSpan.classList.add('typed-corrected');
            }
        } else {
            this.#currentCharacter.isTypedWithError = true;
            this.#currentCharacter.wasTypedWithError = true;
            this.#currentCharacter.characterSpan.classList.add('typed-error');
        }

        this.#index++;
        if (this.#index === this.#text.length) {
            this.#index = this.#text.length;

            // todo: callback on finalize.
            if (this.#characters.every(x => !x.isTypedWithError)) {
                this.#isFinished = true;
                this.#finishedCallback && this.#finishedCallback(this);
            }

            return;
        }

        this.#currentCharacter.characterSpan.classList.add('cursor');
    }
}
