import fetch from 'node-fetch';

export default class TextGenerator {
    // eslint-disable-next-line class-methods-use-this
    async generateText() {
        const response = await fetch('https://api.quotable.io/random');
        const json = await response.json();
        return json.content;
    }
}
