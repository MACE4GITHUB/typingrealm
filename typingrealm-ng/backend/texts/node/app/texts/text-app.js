import TextGenerator from './text-generator.js';

class AheadOfTimeTextGenerator {
    #maxSize = 100;
    #generator;
    #cache = [];

    constructor(generator) {
        this.#generator = generator;
        setInterval(() => this.#getNextText(), 2000);
    }

    async generateText() {
        const text = this.#cache.pop();
        if (text) return text;

        return await this.#generator.generateText();
    }

    async #getNextText() {
        if (this.#cache.length >= this.#maxSize) {
            return;
        }

        this.#cache.push(await this.#generator.generateText());
        console.log('Got next text');
    }
}

export default function textsApp(app) {
    const textGenerator = new AheadOfTimeTextGenerator(
        new TextGenerator());

    app.get('/api/texts', async (req, res) => {
        let text = '';
        while (text.length < 100) {
            /* eslint-disable no-await-in-loop */
            text += `${await textGenerator.generateText()} `;
            /* eslint-enable no-await-in-loop */
        }
        text = text.slice(0, -1);

        res.send({
            text
        });
        res.status(200).end();
    });
}
