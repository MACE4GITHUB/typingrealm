import TextGenerator from './text-generator.js';

export default function(app) {
    const textGenerator = new TextGenerator();

    app.get('/api/texts', async (req, res) => {
        let text = '';
        while (text.length < 100) {
            text += await textGenerator.generateText() + ' ';
        }
        text = text.slice(0, -1);

        res.send({
            text: text
        });
        res.status(200).end();
    });
}
