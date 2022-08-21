import fs from 'fs/promises';

const configText = await fs.readFile('../config.json', { encoding: 'utf8' });
const config = JSON.parse(configText);

console.log('Read config from file.', config);

if (!config.port) config.port = 80;
if (process.env.PORT) config.port = process.env.PORT;

// Temporarily here.
config.dbConnectionString = process.env.DATABASE_URL ?? config.dbConnectionString;
config.cacheConnectionString = process.env.CACHE_URL ?? config.cacheConnectionString;

export default config;
