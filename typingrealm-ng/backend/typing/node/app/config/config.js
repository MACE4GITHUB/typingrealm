import fs from 'fs/promises';

const configText = await fs.readFile('../config.json', { encoding: 'utf8' });
const config = JSON.parse(configText);

console.log('read config from file', config);

// Temporarily here.
config.typingApiPort = 80;
config.typingDbConnectionString = process.env.DATABASE_URL;
config.cacheConnectionString = process.env.CACHE_URL;

export default config;
