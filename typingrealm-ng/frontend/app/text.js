import config from './config/index.js';

async function generateText() {
    const url = config.textsApi.generateTextEndpoint;

    const response = await fetch(url);
    const json = await response.json();

    return json.text;
}

export default generateText;
