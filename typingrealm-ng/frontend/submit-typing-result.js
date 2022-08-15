async function submitTypingResultMock(typer) {
    console.log('MOCK submitted typing results', typer.result);
}

export default async function submitTypingResult(typer) {
    const url = 'http://localhost:30101/api/typing';

    const result = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(typer.result)
    });

    console.log('Submitted typing result', result);
}
