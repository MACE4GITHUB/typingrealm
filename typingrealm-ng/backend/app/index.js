import express from 'express';
import cors from 'cors';
import bodyParser from 'body-parser';
import registerTypingApp from './typing/index.js';
import registerTextsApp from './texts/index.js';

const app = express();
const port = 30101; // TODO: Get this value from the configuration.

app.use(cors({
    origin: 'http://localhost:8080'
}));

// Parse request body as JSON.
app.use(bodyParser.json());

// Register /api/typing service.
registerTypingApp(app);

// Register /api/texts service.
registerTextsApp(app);

// In future, separate services might be refactored into multiple separate
// backend servers. For now though, they'll be hosted in one single server, but
// they should be decoupled from each other as much as possible.

app.listen(port, () => {
    console.log('Host started listening.');
});
