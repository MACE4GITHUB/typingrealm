import retrieveText from './text.js';
import submitTypingResult from './submit-typing-result.js';
import Typer from './typer.js';

const textElement = document.getElementById('text');
const statisticsElement = document.getElementById('statistics');

let typer = null;

export default function() {
    statisticsElement.innerHTML = '<div class="statistics-entry">Press ENTER to start typing next text</div>';
    statisticsElement.classList.remove('hidden');
    document.addEventListener('keydown', processKeyDown);
    document.addEventListener('keyup', processKeyUp);
}

async function processKeyDown(event) {
    if (typer && !typer.isFinished) {
        typer.type(event);
        return;
    }

    if (event.key === 'Enter') {
        // Hide statistics.
        statisticsElement.classList.add('hidden');
        textElement.classList.remove('hidden');

        // Show loader.
        textElement.innerHTML = '<div class="loader"></div>';

        // Retrieve next text from the API.
        const text = await retrieveText();

        // Mock loading for viewing loader animation.
        //await new Promise(r => setTimeout(r, 2000));

        // Create new Typer from this text, set up callback to log the Typer results when we finish typing.
        // Assign it to an intermediatory variable so that we don't start handling keyboard input yet.
        let localTyper = new Typer(text, async (typer) => {
            const statisticsResult = await submitTypingResult(typer);
            textElement.classList.add('hidden');

            statisticsElement.innerHTML = '';

            // TODO: Fill statistics element with data.
            addAnalyticsEntry('Total characters typed', statisticsResult.current.totalCharactersCount);
            addAnalyticsEntry('Total errors', statisticsResult.current.errorCharactersCount);
            addAnalyticsEntry('Total time taken', `${(statisticsResult.current.totalTimeMs / 1000).toFixed(2)} s`);
            addAnalyticsEntry('Average speed', `${statisticsResult.current.speedWpm.toFixed(2)} WPM`);
            addAnalyticsEntry('Precision', `${statisticsResult.current.precision.toFixed(2)} %`);
            statisticsElement.innerHTML += '<div class="statistics-entry" style="margin-top: 50px">Press ENTER to start typing next text</div>';
            statisticsElement.innerHTML += '<div class="statistics-entry" style="margin-bottom: 50px; font-size: 1.5rem">Below is the all-time statistics</div>';
            addAnalyticsEntry('Total characters typed', statisticsResult.allTime.totalCharactersCount);
            addAnalyticsEntry('Total errors', statisticsResult.allTime.errorCharactersCount);
            addAnalyticsEntry('Total time taken', `${(statisticsResult.allTime.totalTimeMs / 1000).toFixed(2)} s`);
            addAnalyticsEntry('Average speed', `${statisticsResult.allTime.speedWpm.toFixed(2)} WPM`);
            addAnalyticsEntry('Precision', `${statisticsResult.allTime.precision.toFixed(2)} %`);

            function addAnalyticsEntry(name, value) {
                const element = document.createElement('div');
                element.classList.add('statistics-entry');

                const nameElement = document.createElement('span');
                nameElement.classList.add('statistics-entry-name');
                nameElement.innerHTML = name;

                const valueElement = document.createElement('span');
                valueElement.classList.add('statistics-entry-value');
                valueElement.innerHTML = value;

                element.appendChild(nameElement);
                element.appendChild(valueElement);

                statisticsElement.appendChild(element);
            }

            statisticsElement.classList.remove('hidden');
        });

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
