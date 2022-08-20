import express from 'express';
import cors from 'cors';
import bodyParser from 'body-parser';

const app = express();

export default function startHost(port, corsOrigins, configureCallback) {
    if (!port) port = 80;

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

    console.log('test this is library');
    app.listen(port, () => {
        console.log('Host started listening.');
    });
};
