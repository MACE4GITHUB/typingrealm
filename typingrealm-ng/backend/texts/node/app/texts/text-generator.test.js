import TextGenerator from './text-generator.js';

function fetchMockFactory(data, validUrl) {
    return function fetchMock(url) {
        if (url === validUrl) {
            return Promise.resolve({
                json: () => Promise.resolve(data)
            });
        }

        return Promise.reject();
    };
}

describe('TextGenerator', () => {
    test('should get proper content', async () => {
        const sut = new TextGenerator(fetchMockFactory({
            content: 'content'
        }, 'https://api.quotable.io/random'));
        const text = await sut.generateText();

        expect(text).toBe('content');
    });

    describe('should throw', () => {
        test('when API content is missing', async () => {
            const sut = new TextGenerator(fetchMockFactory({
                someOtherProperty: 'content'
            }, 'https://api.quotable.io/random'));
            await expect(sut.generateText())
                .rejects
                .toThrow();
        });

        test('when API content is empty', async () => {
            const sut = new TextGenerator(fetchMockFactory({
                content: ''
            }, 'https://api.quotable.io/random'));
            await expect(sut.generateText())
                .rejects
                .toThrow();
        });
    });
});
