import express from 'express';
import cors from 'cors';
import bodyParser from 'body-parser';
import config from './config/config.js';
import registerTypingApp from './typing/index.js';
import registerTextsApp from './texts/index.js';
import registerAuthApp from './auth/index.js';

const app = express();
const port = config.typingApiPort;

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

// In future, separate services might be refactored into multiple separate
// backend servers. For now though, they'll be hosted in one single server, but
// they should be decoupled from each other as much as possible.

app.listen(port, () => {
    console.log('Host started listening.');
});
