import fetch from 'node-fetch';

export default class TextGenerator {
    async generateText() {
        const response = await fetch('https://api.quotable.io/random');
        const json = await response.json();
        return json.content;
    }
}
