# Docker usage guide

This is a short guide on useful commands & TypingRealm-related commands to run to build proper Docker images.

## Common commands

```
docker ps
docker container ls -a
docker container prune

docker network ls
docker network create NETWORK_NAME
docker network prune

docker image ls
docker image prune -a

docker system prune -a

docker kill $(docker ps -q)

docker exec -it CONTAINER_NAME /bin/sh
```



## Build

By default tag is "latest".

```
-t IMAGE_NAME (-t typingrealm-profiles)
-t IMAGE_NAME:TAG (-t typingrealm-profiles:dev)
```

### Production

```
docker network create tyr_typingrealm-net
docker build -t typingrealm-identityserver -f TypingRealm.IdentityServer.Host/Dockerfile .
docker build -t typingrealm-profiles -f TypingRealm.Profiles.Api/Dockerfile .
docker build -t typingrealm-data -f TypingRealm.Data.Api/Dockerfile .
docker build -t typingrealm-web-ui -f typingrealm-web/Dockerfile ./typingrealm-web
```

### Development

```
docker network create dev-tyr_typingrealm-net
docker build -t typingrealm-identityserver:dev -f TypingRealm.IdentityServer.Host/Dockerfile .
docker build -t typingrealm-profiles:dev -f TypingRealm.Profiles.Api/Dockerfile .
docker build -t typingrealm-data:dev -f TypingRealm.Data.Api/Dockerfile .
```



## Run

### Production

This needs to be added to every .NET project RUN (except IdentityServer):

```
    -e SERVICE_AUTHORITY=https://typingrealm-identityserver/
    -e PROFILES_URL=https://typingrealm-profiles
    -e DATA_URL=https://typingrealm-data

    --memory="1g" --memory-reservation="750m"
```

```
docker run -d --net tyr_typingrealm-net --restart unless-stopped --name typingrealm-identityserver typingrealm-identityserver
docker run -d --net tyr_typingrealm-net --restart unless-stopped --name typingrealm-profiles typingrealm-profiles

docker run -d --net tyr_typingrealm-net --restart unless-stopped --name typingrealm-data typingrealm-data
    --e ConnectionStrings:DataConnection="Server=typingrealm-postgres; Port=5432; User Id=postgres; Password=admin; Database=typingrealm_data"
    --name typingrealm-data typingrealm-data

docker run -d --net tyr_typingrealm-net --restart unless-stopped --name typingrealm-web-ui typingrealm-web-ui



docker run -d --net tyr_typingrealm-net --restart unless-stopped --memory="1g" --memory-reservation="750m" --name typingrealm-identityserver typingrealm-identityserver
docker run -d --net tyr_typingrealm-net --restart unless-stopped --memory="1g" --memory-reservation="750m" -e SERVICE_AUTHORITY=http://typingrealm-identityserver/ -e PROFILES_URL=http://typingrealm-profiles -e DATA_URL=http://typingrealm-data --name typingrealm-profiles typingrealm-profiles
docker run -d --net tyr_typingrealm-net --restart unless-stopped --memory="1g" --memory-reservation="750m" -e SERVICE_AUTHORITY=http://typingrealm-identityserver/ -e PROFILES_URL=http://typingrealm-profiles -e DATA_URL=http://typingrealm-data --e ConnectionStrings:DataConnection="Server=typingrealm-postgres; Port=5432; User Id=postgres; Password=admin; Database=typingrealm_data" --name typingrealm-data typingrealm-data
docker run -d --net tyr_typingrealm-net --restart unless-stopped --memory="1g" --memory-reservation="750m" --name typingrealm-web-ui typingrealm-web-ui

/* Infrastructure */
docker run -d --net tyr_typingrealm-net --restart unless-stopped --memory="2g" --memory-reservation="1.75g" -e POSTGRES_PASSWORD=admin --name typingrealm-postgres postgres

/* For debugging - add a port to be able to attach to the database & make backups of data */
docker run -d --net tyr_typingrealm-net -p 5430:5432 -e POSTGRES_PASSWORD=admin --name typingrealm-postgres postgres
```



#### Reverse proxy

```
(Need to check this)
${PWD} should work on Windows in Powershell.

docker run -d
    -p 80:80 -p 443:443
    --net tyr_typingrealm-net
    --restart unless-stopped
    --memory="1g" --memory-reservation="750m"
    -v (ABSOLUTE_PATH_TO_REPOSITORY)/reverse-proxy/Caddyfile:/etc/caddy/Caddyfile
    -v (ABSOLUTE_PATH_TO_REPOSITORY)/reverse-proxy/caddy_data:/data
    --name typingrealm-caddy caddy

docker run -d
    -p 80:80 -p 443:443
    --net tyr_typingrealm-net
    --restart unless-stopped
    --memory="1g" --memory-reservation="750m"
    -v /d/Projects/typingrealm/reverse-proxy/Caddyfile:/etc/caddy/Caddyfile
    -v /d/Projects/typingrealm/reverse-proxy/caddy_data:/data
    --name typingrealm-caddy caddy
```



#### Docker Compose

```
docker-compose -p tyr -f docker-compose.yml -f docker-compose.production.yml up -d --build
docker-compose -p tyr -f docker-compose.yml -f docker-compose.production.yml up -d

docker-compose -p tyr -f docker-compose.yml -f docker-compose.production.yml down

/* Restart service */
docker-compose -p tyr -f docker-compose.yml -f docker-compose.production.yml up -d --build SERVICE_NAME
```



### Development

```
docker run -d
    --net dev-tyr_typingrealm-net
    -p 30000:80
    -e ASPNETCORE_ENVIRONMENT=Development
    --name dev-typingrealm-identityserver typingrealm-identityserver:dev

docker run -d
    --net dev-tyr_typingrealm-net
    -p 30103:80
    -e SERVICE_AUTHORITY=http://host.docker.internal:30000/
    -e PROFILES_URL=http://host.docker.internal:30103
    -e DATA_URL=http://host.docker.internal:30400
    -e ASPNETCORE_ENVIRONMENT=Development
    --name dev-typingrealm-profiles typingrealm-profiles:dev

docker run -d
    --net dev-tyr_typingrealm-net
    -p 30400:80
    -e SERVICE_AUTHORITY=http://host.docker.internal:30000/
    -e PROFILES_URL=http://host.docker.internal:30103
    -e DATA_URL=http://host.docker.internal:30400
    -e ASPNETCORE_ENVIRONMENT=Development
    --name dev-typingrealm-data typingrealm-data:dev

docker-compose -p dev-tyr -f docker-compose.yml -f docker-compose.override.yml up -d --build
docker-compose -p dev-tyr -f docker-compose.yml -f docker-compose.override.yml down -d --build

docker-compose -p dev-tyr -f docker-compose.yml -f docker-compose.override.yml up -d
docker-compose -p dev-tyr -f docker-compose.yml -f docker-compose.override.yml down -d



/* Infrastructure */
docker run -d --net dev-tyr_typingrealm-net -p 5432:5432 -e POSTGRES_PASSWORD=admin --name typingrealm-postgres-dev postgres
```

### When running separate Docker containers from VS

For example, for Profiles API project:

```
Image name: typingrealmprofilesapi:dev
Container name: TypingRealm.Profiles.Api
```



## URLs

```
=== For localhost debugging ===
LocalhostIssuer: http://localhost:30000/
ServiceDiscovery, Services: http://localhost:30500/, http://localhost:PORT/


=== For VS Docker (or VS Compose) debugging ===
LocalhostIssuer: http://host.docker.internal:30000/
ServiceDiscovery, Services: http://host.docker.internal:30500/, http://host.docker.internal:PORT/


=== For Docker (or Compose) deployment ===
LocalhostIssuer: http://typingrealm-identityserver/
ServiceDiscovery, Services: http://typingrealm-servicediscovery/, http://typingrealm-SERVICENAME/
```
