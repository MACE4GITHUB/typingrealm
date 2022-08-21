module.exports = function(config, env, service, backend, fs) {
    if (service.notService || backend.type !== 'dotnet') return;

    // TODO: Create local docker files as well.
    if (env.name === 'local') return;

    const projectName = config.projectName;
    const infraFolder = config.infrastructureDataFolder;

    // TODO: Incorporate DBMATE if postgres is present in infra.

    const lines = [];
    lines.push('FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base');
    lines.push('WORKDIR /app');
    lines.push('EXPOSE 80');
    lines.push('');

    lines.push('FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build');
    lines.push('WORKDIR /src');
    lines.push('COPY .editorconfig .');
    lines.push('COPY Directory.Build.props .');

    for (let component of config.dotnetComponents) {
        lines.push(`COPY ["framework/${backend.type}/${component}/${component}.csproj", "framework/${backend.type}/${component}/"]`);
    }
    // TODO: Universalize: tarball all csproj files to copy only them in all the folders.
    for (let project of backend.projects) {
        lines.push(`COPY ["${service.name}/${backend.type}/${project}/${project}.csproj", "${service.name}/${backend.type}/${project}/"]`);
    }

    lines.push(`WORKDIR /src/${service.name}/${backend.type}`);
    lines.push(`RUN dotnet restore "${backend.hostProject}/${backend.hostProject}.csproj"`);
    lines.push('WORKDIR /src');
    lines.push(`COPY framework/${backend.type} framework/${backend.type}/`);
    lines.push(`COPY ${service.name}/${backend.type} ${service.name}/${backend.type}/`);
    lines.push(`WORKDIR "/src/${service.name}/${backend.type}/${backend.hostProject}"`)
    lines.push(`RUN dotnet build "${backend.hostProject}.csproj" -c Release -o /app/build`);

    lines.push('');
    lines.push('FROM build AS publish');
    lines.push(`RUN dotnet publish "${backend.hostProject}.csproj" -c Release -o /app/publish`);
    lines.push('');

    lines.push('FROM base AS final');
    lines.push('WORKDIR /app');
    lines.push('COPY --from=publish /app/publish .');
    lines.push(`ENTRYPOINT ["dotnet", "${backend.hostProject}.dll"]`);
    lines.push('');

    write();

    function write() {
        fs.writeFile(`./${config.dockerContext}/${service.name}/${backend.type}/Dockerfile-${env.name}`, lines.join('\n'), err => {
            console.log(err);
        });
    }
}

