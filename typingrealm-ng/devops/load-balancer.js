module.exports = function generateLoadBalancer(config, env, service, fs, envPrefix) {
    if (!env.isLoadBalanced || service.notService) return;

    const lines = [];
    lines.push(':80 {');

    const endServices = service.backends
        .flatMap(backend => {
            const hosts = [ `${envPrefix}${config.projectName}-${service.name}-${backend.type}` ];

            for (let i = 2; i <= backend.replicas; i++) {
                hosts.push(`${hosts[0]}-${i}`);
            }

            return hosts.map(host => `${host}:80`);
        })
        .join(' ');

    lines.push(`    reverse_proxy ${endServices} {`);
    lines.push('        lb_policy round_robin');
    lines.push('        health_path /health');
    lines.push('        health_interval 10s');
    lines.push('    }');
    lines.push('}');
    lines.push('');

    fs.writeFile(`./${config.dockerContext}/${service.name}/Caddyfile-${env.name}`, lines.join('\n'), err => {
        console.log(err);
    });
};
