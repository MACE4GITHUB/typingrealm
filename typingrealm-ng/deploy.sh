#!/usr/bin/env bash
set -Eeuo pipefail

if [ "$1" == "prod" ]; then
    docker-compose -p tyr -f ./docker-compose.prod.yml up -d --build
elif [ "$1" == "dev" ]; then
    docker-compose -p dev-tyr -f ./docker-compose.dev.yml up -d --build
elif [ "$1" == "local" ]; then
    docker-compose -p local-tyr -f ./docker-compose.local.yml up -d --build
fi
