const generateLoadBalancer = require('./load-balancer.js');
const generateNodeDockerfile = require('./node-dockerfile.js');
const generateDotnetDockerFile = require('./dotnet-dockerfile.js');
const generateConfigFile = require('./config.js');

module.exports = function(config, fs) {
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

            generateLoadBalancer(config, env, service, fs, getPrefix(env));
            generateConfigFile(config, env, service, fs);
        }

        fs.writeFile(`./docker-compose.${env.name}.yml`, content.join('\n'), err => {
            console.log(err);
        });
    }

    // Infra docker-compose.
    {
        const content = [];
        content.push("version: '3.4'");
        content.push("");
        content.push("networks:");

        for (let env of config.envs) {
            const network = getExternalNetwork(env.name);
            content.push(`  ${network}:`);
            content.push(`    external: true`);
        }

        // Add caddy.
        content.push("");
        content.push("services:");
        content.push(`  ${projectName}-caddy:`);
        content.push(`    image: caddy`);
        content.push(`    container_name: ${projectName}-caddy`);
        content.push(`    networks:`);

        for (let env of config.envs) {
            const network = getExternalNetwork(env.name);
            content.push(`      - ${network}`);
        }

        content.push(`    ports:`);
        content.push(`      - 80:80`);
        content.push(`      - 443:443`);
        content.push(`    volumes:`);
        content.push(`      - ${config.caddyfile}:/etc/caddy/Caddyfile`);
        content.push(`      - ${infraFolder}/prod/caddy_data:/data`);
        content.push(`    restart: unless-stopped`);
        content.push(`    mem_limit: 1g`);
        content.push(`    mem_reservation: 750m`);
        content.push("");

        fs.writeFile(`./docker-compose.infra.yml`, content.join('\n'), err => {
            console.log(err);
        });
    }

    function getServiceEntries(service, env) {
        let content = [];

        const infraContent = [];
        const envsToAddToEachBackend = [];

        if (service.infra?.length > 0) {
            let headerAdded = false;
            for (let infra of service.infra) {
                if (!headerAdded) {
                    envsToAddToEachBackend.push(`    environment:`);
                    headerAdded = true;
                }

                if (infra.type === 'postgres') {
                    infraContent.push('');
                    addInfra(service, infraContent, infra, env);

                    const databaseName = infra.database ?? service.name;
                    envsToAddToEachBackend.push(`      - DATABASE_URL=postgres://postgres:admin@${env.useInfrastructureFrom ? `${env.useInfrastructureFrom}-` : getPrefix(env)}${projectName}-${service.name}-postgres:5432/${databaseName}?sslmode=disable`);

                    continue;
                }

                if (infra.type === 'redis') {
                    infraContent.push('');
                    addInfra(service, infraContent, infra, env);

                    envsToAddToEachBackend.push(`      - CACHE_URL=redis://${env.useInfrastructureFrom ? `${env.useInfrastructureFrom}-` : getPrefix(env)}${projectName}-${service.name}-redis:6379`);
                    continue;
                }

                throw new Error('Unknown infrastructure type.');
            }
        }

        if (!service.backends) { service.backends = [ {
            serviceId: service.name
        } ] };

        let addedCaddy = false;
        for (let backend of service.backends) {
            generateNodeDockerfile(config, env, service, backend, fs);
            generateDotnetDockerFile(config, env, service, backend, fs);

            /* Temporary hack until I figure out how to debug projects in Docker. */
            if (env.name === 'local' && backend.skipLocal) continue;

            let replicas = backend.replicas ?? 1;
            if (replicas === 0) replicas = 1; // TODO: Implement possibility to specify 0 replicas.
            backend.serviceId = backend.type ? `${service.name}-${backend.type}` : service.name;

            function getReplicaPostfix(replica) {
                if (replica === 0) return '';
                return `-${replica + 1}`;
            }

            if (env.localVolume || env.exposeLocalPorts) replicas = 1;

            if (service.isLoadBalanced && env.isLoadBalanced && !addedCaddy) {
                addedCaddy = true;
                content.push(`  ${getPrefix(env)}${projectName}-${service.name}-api:`);
                content.push(`    image: caddy`);
                content.push(`    container_name: ${getPrefix(env)}${projectName}-${service.name}-api`);
                content.push(`    networks:`);
                content.push(`      - ${getPrefix(env)}${projectName}-net`);
                content.push(`      - ${getPrefix(env)}${projectName}-${service.name}-net`);
                // TODO: Expose local ports here (after balancing) instead of from direct containers.
                content.push(`    volumes:`);
                content.push(`      - ${service.dockerContext ?? config.dockerContext}/${service.name}/Caddyfile-${env.name}:/etc/caddy/Caddyfile`);
                content.push(`    restart: unless-stopped`);
                content.push(`    mem_limit: 1g`);
                content.push('    mem_reservation: 750m');
                content.push("");
            }

            for (let replica = 0; replica < replicas ?? 1; replica++) {
                content.push(`  ${getPrefix(env)}${projectName}-${backend.serviceId}${getReplicaPostfix(replica)}:`);
                content.push(`    image: \${DOCKER_REGISTRY-}${getPrefix(env)}${projectName}-${backend.serviceId}`);
                content.push(`    container_name: ${getPrefix(env)}${projectName}-${backend.serviceId}${getReplicaPostfix(replica)}`);
                content.push(`    networks:`);
                content.push(`      - ${getPrefix(env)}${projectName}-net`);
                content.push(`      - ${getPrefix(env)}${projectName}-${service.name}-net`);

                if (service.infra?.length > 0 && env.useInfrastructureFrom) {
                    const network = getExternalNetwork(env.useInfrastructureFrom);
                    content.push(`      - ${network}`);
                }

                if (env.exposeLocalPorts && backend.localPort) {
                    content.push(`    ports:`);
                    content.push(`      - ${backend.localPort}:80`);
                    if (backend.debuggerPort) {
                        content.push(`      - ${backend.debuggerPort}:9229`);
                    }
                }

                const dockerFile = backend.type
                    ? `${service.name}/${backend.type}/Dockerfile-${env.name}`
                    : `Dockerfile-${env.name}`;

                content.push(`    build:`);
                content.push(`      context: ${service.dockerContext ?? config.dockerContext}`);
                content.push(`      dockerfile: ${dockerFile}`);

                if (env.localVolume && backend.localVolumes?.length > 0) {
                    content.push(`    volumes:`);
                    for (let volume of backend.localVolumes) {
                        content.push(`      - ${volume}`);
                    }
                }

                content.push(`    restart: unless-stopped`);
                content.push(`    mem_limit: 1g`);
                content.push('    mem_reservation: 750m');
                content.push(...envsToAddToEachBackend);
                content.push('');
            }
        }

        content = content.slice(0, -1);
        content.push(...infraContent);

        return content;
    }

    function addInfra(service, content, infra, env) {
        if (env.useInfrastructureFrom) return;
        const infraName = infra.name ?? infra.type;
        const infraImage = infra.name ?? infra.type;

        content.push(`  ${getPrefix(env)}${projectName}-${service.name}-${infraName}:`);
        content.push(`    image: ${infraImage}`);
        content.push(`    container_name: ${getPrefix(env)}${projectName}-${service.name}-${infraName}`);
        content.push(`    networks:`);
        content.push(`      - ${getPrefix(env)}${projectName}-net`);
        content.push(`      - ${getPrefix(env)}${projectName}-${service.name}-net`);

        if (infra.ports && env.exposeInfraPorts) {
            let portsValue = infra.ports;
            if (!env.infraPortPrefix) throw new Error('Infra port prefix should be specified if ports are exposed.');

            let [hostPort, containerPort] = portsValue.split(':');
            hostPort = `${10 + Number(env.infraPortPrefix)}${hostPort.substring(hostPort.length - 3, hostPort.length)}`;
            portsValue = env.infraPortPrefix + portsValue;

            content.push(`    ports:`);
            content.push(`      - ${hostPort}:${containerPort}`);
        }

        const infraType = config.infraTypes.find(x => x.type === infra.type);
        if (!infraType) throw new Error(`Infra with type ${infra.type} is not found.`);

        if (infraType.volume) {
            content.push(`    volumes:`);
            content.push(`      - ${infraFolder}/${env.name}/${service.name}/${infraName}:${infraType.volume}`);
        }

        content.push(`    restart: unless-stopped`);
        content.push(`    mem_limit: 1g`);
        content.push('    mem_reservation: 750m');

        if (infraType.environment) {
            content.push('    environment:');

            for (let envString of infraType.environment) {
                content.push(`      - ${envString}`);
            }
        }
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
}
