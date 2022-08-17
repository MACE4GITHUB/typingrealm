import express from 'express';
import cors from 'cors';
import bodyParser from 'body-parser';
import config from './config/config.js';
import registerTypingApp from './typing/index.js';
import registerTextsApp from './texts/index.js';
import registerAuthApp from './auth/index.js';

const app = express();
const port = config.typingApiPort;

console.log('Applying CORS policy', config.cors);
app.use(cors({
    origin: config.cors
}));

// Parse request body as JSON.
app.use(bodyParser.json());

// Register /api/typing service.
registerTypingApp(app);

// Register /api/texts service.
registerTextsApp(app);

// Register mock Auth service.
registerAuthApp(app);

// Health check endpoint with a counter.
let healthCheckCount = 0;
app.get('/health', (req, res) => {
    res.send({ count: healthCheckCount++ });
    console.log('Triggered healthcheck', healthCheckCount);
    res.status(200).end();
});

// In future, separate services might be refactored into multiple separate
// backend servers. For now though, they'll be hosted in one single server, but
// they should be decoupled from each other as much as possible.

app.listen(port, () => {
    console.log('Host started listening.');
});
