import express from 'express';
import cors from 'cors';
import bodyParser from 'body-parser';
import config from '@typingrealm/configuration';

const app = express();

export default function startHost(configureCallback) {
    const { port, corsOrigins } = config;

    console.log('Applying CORS policy', corsOrigins);
    app.use(cors({
        origin: corsOrigins
    }));

    app.use(bodyParser.json());

    configureCallback(app);

    let healthCheckCount = 0;
    app.get('/health', (_, res) => {
        res.send({ count: healthCheckCount++ });
        console.log('Triggered healthcheck', healthCheckCount);
        res.status(200).end();
    });

    app.listen(port, () => {
        console.log('Host started listening.');
    });
}
