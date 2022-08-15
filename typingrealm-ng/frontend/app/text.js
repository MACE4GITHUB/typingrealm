import config from './config/index.js';

/** Use this for local testing. */
async function generateTextMock() {
    return 'It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout.';
}

async function generateText() {
    const url = config.textsApi.generateTextEndpoint;

    const response = await fetch(url);
    const json = await response.json();

    return json.text;
}

export default generateText;
