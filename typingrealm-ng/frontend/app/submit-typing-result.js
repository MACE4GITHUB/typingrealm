import { tokenProvider } from './auth.js';
import config from './config/index.js';

async function submitTypingResultMock(typer) {
    console.log('MOCK submitted typing results', typer.result);
}

export default async function submitTypingResult(typer) {
    const uri = config.typingApi.typingSubmitEndpoint;
    const timezone = Intl.DateTimeFormat().resolvedOptions().timeZone;

    const body = {
        createdPerf: typer.result.createdPerf,
        text: typer.result.text,
        events: typer.result.events,
        clientCreatedAt: typer.result.createdAt,
        clientFinishedAt: typer.result.finishedAt,
        clientTimezone: timezone,
        clientTimezoneOffset: typer.result.createdAt.getTimezoneOffset()
    };

    const token = await tokenProvider();
    const result = await fetch(uri, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(body)
    });
    const json = await result.json();

    console.log('Submitted typing result', typer.result, json);

    const allTimeResult = await fetch(config.typingApi.typingAnalyzeAllEndpoint, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    const allTimeJson = await allTimeResult.json();

    return {
        current: json,
        allTime: allTimeJson
    };
}
