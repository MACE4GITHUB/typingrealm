import config from '@typingrealm/configuration';
import pg from 'pg';
import TypingResultRepository from './typing-result-repository.js';
import Cache from './caching/index.js';

const { Pool } = pg, // Singleton throughout the whole typing app, consider refactoring.
    pool = new Pool({
        connectionString: config.dbConnectionString
    }),
    typingResultRepository = new TypingResultRepository(pool),
    cache = new Cache();

export default function typingApp(app) {
    app.get('/api/typing', async (req, res) => {
        const profile = req.headers.authorization.split('Bearer ')[1].split('_')[2];
        res.send(await typingResultRepository.getAll(profile));
        res.status(200).end();
    });

    app.post('/api/typing', async (req, res) => {
        const profile = req.headers.authorization.split('Bearer ')[1].split('_')[2];

        // TODO: Make the profile information available in the context of repository?
        await typingResultRepository.save(req.body, profile);

        const analytics = analyze(req.body);

        res.send(analytics);
        res.status(201).end();
    });

    app.get('/api/typing/analyze-all', async (req, res) => {
        const profile = req.headers.authorization.split('Bearer ')[1].split('_')[2],
            aggregated = await getStatisticsForProfile(profile);

        res.send(aggregated);
        res.status(200).end();
    });

    app.get('/api/typing/global-statistics', async (req, res) => {
        const profiles = await typingResultRepository.getAllProfiles();

        const stats = [];
        /* eslint-disable no-await-in-loop */
        // TODO: Consider enabling eslint rule and refactoring the code.
        for (const profile of profiles) {
            stats.push(await getStatisticsForProfile(profile));
        }
        /* eslint-enable no-await-in-loop */

        const aggregated = stats.reduce((result, stat) => ({
            totalCharactersCount: result.totalCharactersCount + stat.totalCharactersCount,
            errorCharactersCount: result.errorCharactersCount + stat.errorCharactersCount,
            totalTimeMs: result.totalTimeMs + stat.totalTimeMs
        }), {
            totalCharactersCount: 0,
            errorCharactersCount: 0,
            totalTimeMs: 0
        });

        /* eslint-disable max-len */
        aggregated.speedWpm = ((60 * 1000 * aggregated.totalCharactersCount) / aggregated.totalTimeMs) / 5;
        aggregated.precision = ((aggregated.totalCharactersCount - aggregated.errorCharactersCount) * 100) / aggregated.totalCharactersCount;

        res.send(aggregated);
        res.status(200).end();
    });
}

async function getStatisticsForProfile(profile) {
    let cachedAnalytics = await cache.get(`analytics_all_time_${profile}`);
    console.log('Read cached', cachedAnalytics);

    let lastProcessedId = 0;
    try {
        if (cachedAnalytics) {
            cachedAnalytics = JSON.parse(cachedAnalytics);
            lastProcessedId = cachedAnalytics.lastProcessedId ?? 0;

            if (!isValid(cachedAnalytics)) {
                console.log('Cached value is not valid, re-analyzing.');
                cachedAnalytics = null;
                lastProcessedId = 0;
            }
        }
    } catch (error) {
        console.log('Error when reading cache. Resetting cache.', error);
        lastProcessedId = 0;
    }

    console.log('Getting records since', Number(lastProcessedId) + 1);
    const unprocessedData = await typingResultRepository.getAll(profile, Number(lastProcessedId) + 1);
    console.log(`Got ${unprocessedData.length} records from the database`);

    const aggregated = unprocessedData.map(analyze).reduce((result, data) => ({
        totalCharactersCount: result.totalCharactersCount + data.totalCharactersCount,
        errorCharactersCount: result.errorCharactersCount + data.errorCharactersCount,
        totalTimeMs: result.totalTimeMs + data.totalTimeMs
    }), cachedAnalytics ?? {
        totalCharactersCount: 0,
        errorCharactersCount: 0,
        totalTimeMs: 0
    });

    lastProcessedId = unprocessedData[unprocessedData.length - 1]?.id ?? 0;

    aggregated.speedWpm = ((60 * 1000 * aggregated.totalCharactersCount) / aggregated.totalTimeMs) / 5;
    aggregated.precision = ((aggregated.totalCharactersCount - aggregated.errorCharactersCount) * 100) / aggregated.totalCharactersCount;

    const cachedObject = { lastProcessedId, ...aggregated };
    console.log('Caching', cachedObject);
    await cache.set(`analytics_all_time_${profile}`, JSON.stringify(cachedObject));

    return aggregated;
}

function isValid(cachedAnalytics) {
    /* eslint-disable no-restricted-syntax */
    for (const field in cachedAnalytics) {
    /* eslint-enable no-restricted-syntax */
        if (cachedAnalytics[field] == null) return false;
    }

    return true;
}

function analyze(result) {
    const { text } = result;

    let index = 0;

    for (const event of result.events) {
        if (event.key === text[0]) {
            break;
        }

        index++;
        continue;
    }

    const events = result.events.slice(index),
        firstPerf = events[0].perf;

    events.forEach(e => { e.perf -= firstPerf; });

    index = 0;
    let errorIndex = null,
        wasErrors = 0,
        lastEvent = null;

    for (const event of events) {
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

    const totalCharactersCount = text.length,
        totalTimeMs = lastEvent.perf,
        analytics = {
            totalCharactersCount,
            totalTimeMs,
            errorCharactersCount: wasErrors,
            speedWpm: ((60 * 1000 * totalCharactersCount) / totalTimeMs) / 5,
            precision: ((totalCharactersCount - wasErrors) * 100) / totalCharactersCount
        };

    return analytics;
}

// Move this function to libraries.
function isKeySymbol(key) {
    return key.length === 1;
}
