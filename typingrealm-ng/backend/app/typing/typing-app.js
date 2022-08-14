import TypingResultRepository from './typing-result-repository.js';

export default function(app) {
    const typingResultRepository = new TypingResultRepository();

    app.get('/api/typing', (req, res) => {
        res.send(typingResultRepository.getAll());
        res.status(200).end();
    });

    app.post('/api/typing', (req, res) => {
        typingResultRepository.save(req.body);
        res.status(201).end();
    });
}
