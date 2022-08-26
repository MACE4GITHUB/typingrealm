import { createClient } from 'redis';
import config from '@typingrealm/configuration';

const client = createClient({
    url: config.cacheConnectionString
});
client.on('error', error => console.log('Redis error', error));
await client.connect();

export default class RedisCache {
    /* eslint-disable class-methods-use-this */
    get(key) {
    /* eslint-enable class-methods-use-this */
        return client.get(key);
    }

    /* eslint-disable class-methods-use-this */
    set(key, value) {
    /* eslint-enable class-methods-use-this */
        return client.set(key, value);
    }
}
