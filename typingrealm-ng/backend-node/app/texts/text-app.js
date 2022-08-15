import TextGenerator from './text-generator.js';

export default function(app) {
    const textGenerator = new TextGenerator();

    app.get('/api/texts', async (req, res) => {
        const text = await textGenerator.generateText();
        res.send({
            text: text
        });
        res.status(200).end();
    });
}
