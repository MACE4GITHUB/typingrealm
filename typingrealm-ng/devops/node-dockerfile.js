module.exports = function(config, env, service, backend, fs) {
    if (service.notService || backend.type !== 'node') return;

    const projectName = config.projectName;
    const infraFolder = config.infrastructureDataFolder;

    const lines = [];
    lines.push('FROM node:18');
    lines.push('');

    if (env.name === 'local') {
        lines.push(`WORKDIR /usr/src/app/${service.name}/${backend.type}`);
        lines.push('CMD [ "/bin/bash", "./local-start.sh" ]');
        lines.push('');

        write();
        return;
    }

    if (service.infra.some(x => x.type === 'postgres')) {
        lines.push('RUN curl -fsSL -o /usr/local/bin/dbmate https://github.com/amacneil/dbmate/releases/latest/download/dbmate-linux-amd64');
        lines.push('RUN chmod +x /usr/local/bin/dbmate');
    }

    lines.push('WORKDIR /usr/src/app');
    lines.push(`COPY ${service.name}/${backend.type}/package*.json ./${service.name}/${backend.type}/`);
    lines.push(`COPY framework/${backend.type} ./framework/${backend.type}`);
    lines.push('');

    for (let component of config.nodeComponents) {
        lines.push(`WORKDIR /usr/src/app/framework/${backend.type}/${component}`);
        lines.push('RUN npm ci --only=production');
    }

    lines.push('');
    lines.push(`WORKDIR /usr/src/app/${service.name}/${backend.type}`);
    lines.push('RUN npm ci --only=production');
    lines.push(`COPY ${service.name}/${backend.type}/app ./app/`);
    lines.push(`COPY ${service.name}/${backend.type}/app/config/config-${env.name}.js ./app/config/config.js`);

    if (service.infra.some(x => x.type === 'postgres')) {
        lines.push(`COPY ${service.name}/db ./db`);
    }

    lines.push('');
    lines.push('CMD [ "npm", "run", "start:prod" ]');
    lines.push('');

    write();

    function write() {
        fs.writeFile(`./${config.dockerContext}/${service.name}/${backend.type}/Dockerfile-${env.name}`, lines.join('\n'), err => {
            console.log(err);
        });
    }
}

