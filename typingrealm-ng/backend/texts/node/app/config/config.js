import fs from 'fs/promises';

const configText = await fs.readFile('../config.json', { encoding: 'utf8' });
const config = JSON.parse(configText);

console.log('read config from file', config);
export default config;
