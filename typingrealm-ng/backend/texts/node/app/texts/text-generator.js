import nodeFetch from 'node-fetch';

export default class TextGenerator {
    #fetch;

    constructor(fetch = nodeFetch) {
        this.#fetch = fetch;
    }

    async generateText() {
        const response = await this.#fetch('https://api.quotable.io/random');
        const json = await response.json();

        if (!json.content) {
            throw new Error('Content is missing.');
        }

        console.log(json);
        return json.content;
    }
}
