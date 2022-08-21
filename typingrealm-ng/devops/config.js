module.exports = function(config, env, service, fs) {
    if (service.notService) return;

    const projectName = config.projectName;
    const infraFolder = config.infrastructureDataFolder;

    const globalConfigFile = fs.readFileSync('./deploy-config.json', 'utf8');
    const globalConfig = JSON.parse(globalConfigFile);

    const serviceConfig = {};
    serviceConfig.corsOrigins = globalConfig.corsOrigins;

    if (!env.isDefaultConfig) {
        if (globalConfig[`corsOrigins:${env.name}`]) {
            serviceConfig.corsOrigins = globalConfig[`corsOrigins:${env.name}`];
        }
    }

    const serviceConfigContent = JSON.stringify(serviceConfig, null, 4) + '\n';
    const envPostfix = env.isDefaultConfig ? '' : `-${env.name}`;

    fs.writeFile(`./${config.dockerContext}/${service.name}/config${envPostfix}.json`, serviceConfigContent, err => {
        console.log(err);
    });
}
