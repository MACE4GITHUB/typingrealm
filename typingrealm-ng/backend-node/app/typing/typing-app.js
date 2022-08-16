import TypingResultRepository from './typing-result-repository.js';

import pg from 'pg';
import config from '../config/config.js';
const { Pool } = pg; // Singleton throughout the whole typing app, consider refactoring.

export default function(app) {
    const pool = new Pool({
        connectionString: config.typingDbConnectionString
    });

    const typingResultRepository = new TypingResultRepository(pool);

    app.get('/api/typing', async (req, res) => {
        res.send(await typingResultRepository.getAll());
        res.status(200).end();
    });

    app.post('/api/typing', async (req, res) => {
        const profile = req.headers.authorization.split('Bearer ')[1].split('_')[2];
        await typingResultRepository.save(req.body, profile); // TODO: Make the profile information available in the context of repository?
        res.status(201).end();
    });
}
