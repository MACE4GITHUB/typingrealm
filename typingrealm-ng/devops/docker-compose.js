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

        if (service.infra?.length > 0 && !env.useInfrastructureFrom) {
            for (let infra of service.infra) {
                if (infra.type === 'postgres') {
                    infraContent.push('');
                    addPostgres(service, infraContent, infra, env);

                    // TODO: Make sure 'environment' line is not added twice.
                    envsToAddToEachBackend.push(`    environment:`);
                    const databaseName = infra.database ?? service.name;
                    envsToAddToEachBackend.push(`      - DATABASE_URL=postgres://postgres:admin@${getPrefix(env)}${projectName}-${service.name}-postgres:5432/${databaseName}?sslmode=disable`);

                    continue;
                }

                throw new Error('Unknown infrastructure type.');
            }
        }

        if (!service.backends) { service.backends = [ {
            serviceId: service.name
        } ] };

        for (let backend of service.backends) {
            let replicas = backend.replicas ?? 1;
            if (replicas === 0) replicas = 1; // TODO: Implement possibility to specify 0 replicas.
            backend.serviceId = backend.servicePath ?? service.name;

            function getReplicaPostfix(replica) {
                if (replica === 0) return '';
                return `-${replica + 1}`;
            }

            if (env.localVolume || env.exposeLocalPorts) replicas = 1;

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
                }

                content.push(`    build:`);
                content.push(`      context: ${service.dockerContext}`);
                content.push(`      dockerfile: ${backend.servicePath ? `${backend.servicePath}/` : ''}Dockerfile-${env.name}`);

                if (env.localVolume && backend.localVolume) {
                    content.push(`    volumes:`);
                    content.push(`      - ${service.dockerContext}${backend.servicePath ? `/${backend.servicePath}` : ''}:${backend.localVolume}`);
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
        content.push('    environment:');
        content.push('      - POSTGRES_PASSWORD=admin');
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
