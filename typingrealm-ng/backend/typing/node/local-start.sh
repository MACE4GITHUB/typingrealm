#!/bin/bash

(
    cd ../../framework/node/configuration
    npm install
)
(
    cd ../../framework/node/host
    npm install
)

npm install
npm run start:mon
