export default class TypingResultRepository {
    #typingResults = new Map();

    getAll() {
        console.log(this.#typingResults);
        return this.#typingResults;
    }

    save(typingResult, profile) {
        if (!this.#typingResults.has(profile)) {
            this.#typingResults.set(profile, []);
        }

        this.#typingResults.get(profile).push(typingResult);
    }
}
