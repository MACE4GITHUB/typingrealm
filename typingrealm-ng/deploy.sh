#!/usr/bin/env bash
#set -Eeuo pipefail

if [ "$1" == "prod" ]; then
    docker-compose -p tyr -f ./docker-compose.production.yml up -d --build
elif [ "$1" == "dev" ]; then
    docker-compose -p dev-tyr -f ./docker-compose.dev.yml up -d --build
elif [ "$1" == "local" ]; then
    if [ "$2" == "stop" ]; then
        docker-compose -p local-tyr -f ./docker-compose.local.yml stop
        docker-compose -p local-tyr -f ./docker-compose.local.yml down --volumes
    else
        docker-compose -p local-tyr -f ./docker-compose.local.yml up -d --build
    fi
elif [ "$1" == "infra" ]; then
    docker-compose -p infra-tyr -f ./docker-compose.infra.yml up -d --build
fi
