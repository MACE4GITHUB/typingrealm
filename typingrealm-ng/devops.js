const fs = require('fs');
const { env } = require('process');

const config = JSON.parse(fs.readFileSync('./envs.config.json', 'utf8'));
const projectName = config.projectName;
const infraFolder = config.infrastructureDataFolder;

// Generate docker-compose files.
for (let env of config.envs) {
    const content = [];

    content.push("version: '3.4'");
    content.push("");
    content.push("networks:");
    content.push(...getNetworkEntries(env, config.services));
    content.push("");
    content.push("services:");

    for (let service of config.services) {
        content.push(...getServiceEntries(service, env));
        content.push("");
    }

    console.log(content.join('\n'));
    fs.writeFile(`./docker-compose.${env.name}.yml`, content.join('\n'), err => {
        console.log(err);
    });
}

function getServiceEntries(service, env) {
    const content = [];

    content.push(`  ${getPrefix(env)}${projectName}-${service.name}:`);
    content.push(`    image: \${DOCKER_REGISTRY-}${getPrefix(env)}${projectName}-${service.name}`);
    content.push(`    container_name: ${getPrefix(env)}${projectName}-${service.name}`);
    content.push(`    networks:`);
    content.push(`      - ${getPrefix(env)}${projectName}-net`);
    content.push(`      - ${getPrefix(env)}${projectName}-${service.name}-net`);

    if (service.infra?.length > 0 && env.useInfrastructureFrom) {
        const network = getExternalNetwork(env.useInfrastructureFrom);
        content.push(`      - ${network}`);
    }

    if (env.exposeLocalPorts && service.localPort) {
        content.push(`    ports:`);
        content.push(`      - ${service.localPort}:80`);
    }

    content.push(`    build:`);
    content.push(`      context: ${service.dockerContext}`);
    content.push(`      dockerfile: Dockerfile-${env.name}`);
    content.push(`    restart: unless-stopped`);
    content.push(`    mem_limit: 1g`);
    content.push('    mem_reservation: 750m');

    if (service.infra?.length > 0 && !env.useInfrastructureFrom) {
        for (let infra of service.infra) {
            if (infra.type === 'postgres') {
                content.push('');
                addPostgres(service, content, infra, env);
                continue;
            }

            throw new Error('Unknown infrastructure type.');
        }
    }

    return content;
}

function addPostgres(service, content, infra, env) {
    const infraName = infra.name ?? infra.type;
    const infraImage = infra.name ?? infra.type;

    content.push(`  ${getPrefix(env)}${projectName}-${service.name}-${infraName}:`);
    content.push(`    image: ${infraImage}`);
    content.push(`    container_name: ${getPrefix(env)}${projectName}-${service.name}-${infraName}`);
    content.push(`    networks:`);
    content.push(`      - ${getPrefix(env)}${projectName}-net`);
    content.push(`      - ${getPrefix(env)}${projectName}-${service.name}-net`);

    if (infra.ports && env.exposeInfraPorts) {
        content.push(`    ports:`);
        content.push(`      - ${infra.ports}`);
    }

    content.push(`    volumes:`);
    content.push(`      - ${infraFolder}/${env.name}/${service.name}/${infraName}:/var/lib/postgresql/data`);
    content.push(`    restart: unless-stopped`);
    content.push(`    mem_limit: 1g`);
    content.push('    mem_reservation: 750m');
}

function getExternalNetwork(envWithInfrastructure) {
    const infraEnv = config.envs.find(x => x.name === envWithInfrastructure);
    if (!infraEnv) throw new Error(`Environment ${envWithInfrastructure} is not found.`);

    return `${getPrefix(infraEnv)}tyr_${getPrefix(infraEnv)}${projectName}-net`;
}

function getNetworkEntries(env, services) {
    const content = [];
    content.push(`  ${getPrefix(env)}${projectName}-net:`);

    for (let service of services) {
        content.push(`  ${getPrefix(env)}${projectName}-${service.name}-net:`);
    }

    if (env.useInfrastructureFrom) {
        const network = getExternalNetwork(env.useInfrastructureFrom);

        content.push(`  ${network}:`);
        content.push(`    external: true`);
    }

    return content;
}

function getPrefix(env) {
    if (env.noPrefix) return "";

    return `${env.name}-`;
}
