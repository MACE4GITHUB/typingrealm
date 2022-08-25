import { createClient } from 'redis';
import config from '@typingrealm/configuration';

const client = createClient({
    url: config.cacheConnectionString
});
client.on('error', error => console.log('Redis error', error));
await client.connect();

export default class RedisCache {
    get(key) {
        return client.get(key);
    }

    set(key, value) {
        return client.set(key, value);
    }
}
