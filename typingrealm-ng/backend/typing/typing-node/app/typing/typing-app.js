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
        const profile = req.headers.authorization.split('Bearer ')[1].split('_')[2];
        res.send(await typingResultRepository.getAll(profile));
        res.status(200).end();
    });

    app.post('/api/typing', async (req, res) => {
        const profile = req.headers.authorization.split('Bearer ')[1].split('_')[2];
        await typingResultRepository.save(req.body, profile); // TODO: Make the profile information available in the context of repository?

        const all = await typingResultRepository.getAll(profile);
        let totalCharactersTyped = 0;
        all.forEach(result => totalCharactersTyped += result.text.length);

        const analytics = analyze(req.body);

        res.send(analytics);
        res.status(201).end();
    });

    app.get('/api/typing/analyze-all', async (req, res) => {
        const profile = req.headers.authorization.split('Bearer ')[1].split('_')[2];
        const allData = await typingResultRepository.getAll(profile);

        const aggregated = allData.map(analyze).reduce((result, data) => {
            return {
                totalCharactersCount: result.totalCharactersCount + data.totalCharactersCount,
                errorCharactersCount: result.errorCharactersCount + data.errorCharactersCount,
                totalTimeMs: result.totalTimeMs + data.totalTimeMs
            };
        }, {
            totalCharactersCount: 0,
            errorCharactersCount: 0,
            totalTimeMs: 0
        });

        aggregated.speedWpm = (60 * 1000 * aggregated.totalCharactersCount / aggregated.totalTimeMs) / 5,
        aggregated.precision = 100 * (aggregated.totalCharactersCount - aggregated.errorCharactersCount) / aggregated.totalCharactersCount

        res.send(aggregated);
        res.status(201).end();
    })
}

function analyze(result) {
    const text = result.text;

    let started = false;
    let index = 0;

    for (let event of result.events) {
        if (event.key === text[0]) {
            started = true;
            break;
        }

        index++;
        continue;
    }

    let events = result.events.slice(index);
    const firstPerf = events[0].perf;
    events.forEach(e => e.perf = e.perf - firstPerf);

    index = 0;
    let errorIndex = null;
    let wasErrors = 0;
    let lastEvent = null;

    for (let event of events) {
        if (event.eventType === 'keyup') continue;

        if (event.key === 'Backspace' && index > 0) {
            index--;
            if (errorIndex === index) {
                errorIndex = null;
            }
        }

        if (!isKeySymbol(event.key)) {
            continue;
        }

        if (!errorIndex && event.key !== text[index]) {
            errorIndex = index;
            wasErrors++;
        }
        index++;

        if (index >= text.length) {
            index = text.length;
            lastEvent = event;

            if (!errorIndex) {
                break;
            }
        }
    }

    const totalCharactersCount = text.length;
    const totalTimeMs = lastEvent.perf;
    const analytics = {
        totalCharactersCount: totalCharactersCount,
        errorCharactersCount: wasErrors,
        totalTimeMs: totalTimeMs,
        speedWpm: (60 * 1000 * totalCharactersCount / totalTimeMs) / 5,
        precision: 100 * (totalCharactersCount - wasErrors) / totalCharactersCount
    }

    return analytics;
}

// Move this function to libraries.
function isKeySymbol(key) {
    return key.length === 1;
}
