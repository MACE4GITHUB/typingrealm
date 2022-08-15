import retrieveText from './text.js';
import submitTypingResult from './submit-typing-result.js';
import Typer from './typer.js';

const textElement = document.getElementById('text');
let typer = null;

export default function() {
    textElement.innerHTML = 'Press ENTER to start...';
    document.addEventListener('keydown', processKeyDown);
    document.addEventListener('keyup', processKeyUp);
}

async function processKeyDown(event) {
    if (typer && !typer.isFinished) {
        typer.type(event);
        return;
    }

    if (event.key === 'Enter') {
        // Show loader.
        textElement.innerHTML = '<div class="loader"></div>';

        // Retrieve next text from the API.
        const text = await retrieveText();

        // Mock loading for viewing loader animation.
        //await new Promise(r => setTimeout(r, 2000));

        // Create new Typer from this text, set up callback to log the Typer results when we finish typing.
        // Assign it to an intermediatory variable so that we don't start handling keyboard input yet.
        let localTyper = new Typer(text, submitTypingResult);

        // Remove loader and add Typer characters to the text element.
        textElement.innerHTML = '';
        textElement.append(...localTyper.characterSpans);

        // Set the new typer so it can start accepting keyboard input.
        typer = localTyper;
    }
}

function processKeyUp(event) {
    if (typer && !typer.isFinished) {
        typer.type(event);
    }
}
