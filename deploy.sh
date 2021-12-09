#!/usr/bin/env bash

set -Eeuo pipefail

if [ "$1" == "prod" ]; then
    docker-compose -p tyr -f ./docker-compose.prod.yml up -d --build
elif [ "$1" == "all" ]; then
    docker-compose -p tyr -f ./docker-compose.prod.yml up -d --build
    docker-compose -p local-tyr -f ./docker-compose.local.yml up -d --build
    docker-compose -p dev-tyr -f ./docker-compose.dev.yml up -d --build
else
    docker-compose -p $1-tyr -f ./docker-compose.$1.yml up -d --build
fi

