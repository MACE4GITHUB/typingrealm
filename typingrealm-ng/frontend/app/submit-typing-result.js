import { tokenProvider } from './auth.js';

async function submitTypingResultMock(typer) {
    console.log('MOCK submitted typing results', typer.result);
}

export default async function submitTypingResult(typer) {
    const url = 'http://localhost:30101/api/typing';

    const body = typer.result;
    body.createdAtOffset = body.createdAt.getTimezoneOffset();

    const token = await tokenProvider();
    const result = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(body)
    });

    console.log('Submitted typing result', typer.result, result);
}
