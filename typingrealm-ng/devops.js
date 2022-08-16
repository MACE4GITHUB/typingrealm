const fs = require('fs');
const config = JSON.parse(fs.readFileSync('./envs.config.json', 'utf8'));

const generateDockerCompose = require('./devops/docker-compose.js');
generateDockerCompose(config, fs);
