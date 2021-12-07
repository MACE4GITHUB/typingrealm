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

docker exec CONTAINER_NAME env
```



## Manual Build & Run

Since anyone who has Docker can also have Docker-Compose, I have opted out to have Compose profiles as a single source of truth. Any Docker build / run commands can be reverse-engineered from there, so here we only have a short list of useful commands to build and run images manually.

By default tag is "latest".

```
-t IMAGE_NAME (-t typingrealm-profiles)
-t IMAGE_NAME:TAG (-t typingrealm-profiles:dev)
```

```
docker network create tyr_typingrealm-net
docker build -t typingrealm-identityserver -f TypingRealm.IdentityServer.Host/Dockerfile .
```

This needs to be added to every .NET project RUN:

```
    --net NETWORK_NAME
    -p PORT:80
    --env-file .env.prod | .env.dev | .env.debug | .env.local
    --memory="1g" --memory-reservation="750m"
    --restart unless-stopped
    --name PROJECT_NAME
    -v (volumes if needed) ./local-folder-or-file:/container-folder-or-file
```



## Docker Compose and Environments

There are 5 types of build currently:

- Strict Prod (production)
- Prod (or Host)
- Dev (development)
- Local
- Debug

### Strict production

```.env.prod```

Total production mode, no debug features, no open ports. Should be deployed to the final production cluster, the only way to troubleshoot issues is logging.

It requires a host with open 80, 443 ports and domain typingrealm.com mapped to it.

```
docker-compose -p tyr -f docker-compose.strict-prod.yml -d --build
docker-compose -p tyr down
```

URLs:

```
https://typingrealm.com - DEV Web UI
https://api.localhost/data/ - Data API
https://api.localhost/profiles/ - Profiles API
```

### Production / Host

```.env.prod```

This is the version of compose that is deployed to my local machine (typingrealm.com). It has some relaxations around network (like opened port on postgres database so that it can be accessed from local machine), and it has reverse-proxy that also handles requests to DEV and Local environments so that we can have multiple environments. In future, this environment can be called Staging. It uses production Auth0 environment though so users are shared between prod-strict and prod/host envs.

It requires a host with open 80, 443 ports and domain typingrealm.com mapped to it.

```
docker-compose -p tyr -f docker-compose.prod.yml -d --build
docker-compose -p tyr down
```

URLs:

```
https://typingrealm.com - DEV Web UI
https://api.localhost/data/ - Data API
https://api.localhost/profiles/ - Profiles API
```

### Development

```.env.dev```

The development environment accepts requests from URLs like localhost and ports like 4200. This environment uses separate Auth0 connection and accepts locally generated tokens as well. It should be used for all development.

```
docker-compose -p dev-tyr -f docker-compose.dev.yml -d --build
docker-compose -p dev-tyr down
```

URLs:

```
https://dev.typingrealm.com - DEV Web UI
https://dev.api.localhost/data/ - Data API
https://dev.api.localhost/profiles/ - Profiles API
```

### Local

```.env.local```

This environment should primarily be used for Web UI development whenever the main typingrealm.com server is unavailable, so you can run everything locally. It doesn't take any PC ports as well, you can just spin it up and it should work with local proxy ready to go (with self-signed SSL certs).

```
docker-compose -p local-tyr -f docker-compose.local.yml -d --build
docker-compose -p local-tyr down
```

URLs (when HOST reverse-proxy is spinned up):

```
https://localhost - points to your http://localhost:4200
https://api.localhost/data/ - Data API
https://api.localhost/profiles/ - Profiles API
```

### Debug

```.env.debug```

This profile is being used by Visual Studio when spinning up VS Docker-Compose project or running separate docker containers. You can also spin it up manually.

```
docker-compose -p debug-tyr -f docker-compose.yml -d --build
docker-compose -p debug-tyr
```

URLs:

```
http://localhost:30400/ - Data API
http://localhost:30103/ - Profiles API
```
