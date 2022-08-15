import { tokenProvider } from './auth.js';
import config from './config/index.js';

async function submitTypingResultMock(typer) {
    console.log('MOCK submitted typing results', typer.result);
}

export default async function submitTypingResult(typer) {
    const uri = config.typingApi.typingSubmitEndpoint;

    const body = typer.result;
    body.createdAtOffset = body.createdAt.getTimezoneOffset();

    const token = await tokenProvider();
    const result = await fetch(uri, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(body)
    });

    console.log('Submitted typing result', typer.result, result);
}
