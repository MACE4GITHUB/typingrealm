import TextGenerator from './text-generator.js';

describe('TextGenerator', () => {
    test('Should get non-empty text', async () => {
        const sut = new TextGenerator();
        const text = await sut.generateText();
        console.log(text);

        expect(text.length >= 3).toBeTruthy();
    });
});
