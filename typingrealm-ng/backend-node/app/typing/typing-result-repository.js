export default class TypingResultRepository {
    #pool

    constructor(pgPool) {
        this.#pool = pgPool;
    }

    async getAll() {
        const client = await this.#pool.connect();

        try {
            const result = await client.query('select * from typing_bundle');

            return result.rows;
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

            const response = await client.query(
                'insert into typing_bundle(created_at, text, profile_id) values($1, $2, $3) returning id;',
                [typingResult.createdAt, typingResult.text, profile]);
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
            console.log(sql);

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
