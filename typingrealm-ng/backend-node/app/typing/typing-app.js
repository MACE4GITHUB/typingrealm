import TypingResultRepository from './typing-result-repository.js';

export default function(app) {
    const typingResultRepository = new TypingResultRepository();

    app.get('/api/typing', (req, res) => {
        res.send(typingResultRepository.getAll());
        res.status(200).end();
    });

    app.post('/api/typing', (req, res) => {
        const profile = req.headers.authorization.split('Bearer ')[1].split('_')[2];
        typingResultRepository.save(req.body, profile); // TODO: Make the profile information available in the context of repository?
        res.status(201).end();
    });
}
