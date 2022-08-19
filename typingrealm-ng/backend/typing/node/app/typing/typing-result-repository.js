export default class TypingResultRepository {
    #pool

    constructor(pgPool) {
        this.#pool = pgPool;
    }

    async getAllProfiles() {
        const client = await this.#pool.connect();

        try {
            const result = await client.query(`
select distinct profile_id
from typing_bundle
`);

            return result.rows.map(row => row.profile_id);
        } finally {
            client.release();
        }
    }

    async getAll(profile, sinceId) {
        const client = await this.#pool.connect();

        try {
            let result = [];
            if (sinceId) {
                result = (await client.query(`
select id, text
from typing_bundle
where profile_id = $1
and id >= $2
order by id
`, [ profile, sinceId ])).rows;
            } else {
                result = (await client.query(`
select id, text
from typing_bundle
where profile_id = $1
order by id
`, [ profile ])).rows;
            }

            const data = [];
            for (let row of result) {
                const id = row.id;
                const text = row.text;
                const eventsResult = await client.query(`
select order_index, index, event_type, key, perf
from event
where typing_bundle_id = $1
`, [ id ]);

                const events = eventsResult.rows
                    .sort((a, b) => a.order_index - b.order_index)
                    .map(r => {
                        return {
                            index: r.index,
                            eventType: r.event_type,
                            key: r.key,
                            perf: r.perf / 1000
                        };
                    });

                data.push({
                    id: id,
                    text: text,
                    events: events
                });
            }

            return data;
        } finally {
            client.release();
        }
    }

    async save(typingResult, profile) {
        const client = await this.#pool.connect();

        // TODO: Move this re-calculation logic on the level above, to business layer.
        // Or even consider doing it on the client (javascript) side.
        // Also consider sending finishedAt datetime just in case perf calculations lose precision.
        const createdPerf = typingResult.createdPerf;

        try {
            await client.query('begin');

            const bundleArguments = [new Date().toISOString(), typingResult.text, profile, typingResult.clientCreatedAt, typingResult.clientFinishedAt, typingResult.clientTimezone, typingResult.clientTimezoneOffset];
            const response = await client.query(
                'insert into typing_bundle(submitted_at, text, profile_id, client_created_at, client_finished_at, client_timezone, client_timezone_offset) values($1, $2, $3, $4, $5, $6, $7) returning id;',
                bundleArguments);
            const bundleId = response.rows[0].id;

            const eventValuesArray = typingResult.events.map((e, index) => {
                return [
                    bundleId,
                    index,
                    e.index,
                    e.eventType,
                    e.key,
                    Math.round((e.perf - createdPerf) * 1000)
                ];
            });

            const valuesString = generateValuesString(eventValuesArray.length, eventValuesArray[0].length);
            function generateValuesString(length, size) {
                let string = '';
                for (let j = 0; j < length; j++) {
                    string += '(';
                    for (let i = 1; i <= size; i++) {
                        string += `\$${i + j * size},`;
                    }
                    string = string.slice(0, -1);
                    string += '),';
                }
                string = string.slice(0, -1);

                return string;
            }
            const sql = `insert into event(typing_bundle_id, order_index, index, event_type, key, perf) values${valuesString}`;

            await client.query(sql, eventValuesArray.flatMap(x => x));

            await client.query('commit');
        } catch (e) {
            await client.query('rollback');
            throw e;
        } finally {
            client.release();
        }
    }
}
