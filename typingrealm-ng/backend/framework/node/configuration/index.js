import fs from 'fs/promises';

const configText = await fs.readFile('../config.json', { encoding: 'utf8' });
const config = JSON.parse(configText);

console.log('Read config from file.', config);

if (!config.port) config.port = 80;
if (process.env.PORT) config.port = process.env.PORT;

for (const key in config) {
    if (typeof config[key] === 'string' && config[key].startsWith('env:')) {
        config[key] = process.env[config[key].split(':')[1]];
    }
}

export default config;
