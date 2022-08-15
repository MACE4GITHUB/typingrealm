export default class TypingResultRepository {
    #typingResults = [];

    getAll() {
        return this.#typingResults;
    }

    save(typingResult) {
        this.#typingResults.push(typingResult);
    }
}
