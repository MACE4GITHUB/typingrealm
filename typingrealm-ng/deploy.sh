#!/usr/bin/env bash
#set -Eeuo pipefail

if [ "$1" == "all" ]; then
    if [ "$2" == "down" ]; then
        docker-compose -p local-tyr -f ./docker-compose.local.yml down
        docker-compose -p dev-tyr -f ./docker-compose.dev.yml down
        docker-compose -p tyr -f ./docker-compose.production.yml down
    else
        docker-compose -p tyr -f ./docker-compose.production.yml up -d --build
        docker-compose -p dev-tyr -f ./docker-compose.dev.yml up -d --build
        docker-compose -p local-tyr -f ./docker-compose.local.yml up -d --build
    fi
elif [ "$1" == "prod" ]; then
    if [ "$2" == "down" ]; then
        docker-compose -p tyr -f ./docker-compose.production.yml down
    else
        docker-compose -p tyr -f ./docker-compose.production.yml up -d --build
    fi
elif [ "$1" == "dev" ]; then
    if [ "$2" == "down" ]; then
        docker-compose -p dev-tyr -f ./docker-compose.dev.yml down
    else
        docker-compose -p dev-tyr -f ./docker-compose.dev.yml up -d --build
    fi
elif [ "$1" == "local" ]; then
    if [ "$2" == "down" ]; then
        docker-compose -p local-tyr -f ./docker-compose.local.yml down
    else
        docker-compose -p local-tyr -f ./docker-compose.local.yml up -d --build
    fi
elif [ "$1" == "infra" ]; then
    if [ "$2" == "down" ]; then
        docker-compose -p infra-tyr -f ./docker-compose.infra.yml down
    else
        docker-compose -p infra-tyr -f ./docker-compose.infra.yml up -d --build
    fi
fi
