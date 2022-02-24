(async function main() {

    // For quicker access for sending messages.
    let authUserIdKey;

    /* INITIALIZATION */

    // TODO: Make sure AbsoluteDelay is 0 before sending data to server.
    // Data shouldn't be reset/modified after typing has been completed and before data has been sent.

    // Unique message id.
    // It should probably be unique every time, as just incrementing it will lead to the same values
    // in case user refreshes the page multiple times.
    let uniqueMessageIdCounter = 0;

    const createAuth0Client = window.createAuth0Client;
    const signalR = window.signalR;
    let auth0 = undefined;

    const url_string = window.location.href;
    const url = new URL(url_string);

    let profile = url.searchParams.get('profile');

    let PROFILES_URL = 'https://api.typingrealm.com/profiles';
    let DATA_URL = 'https://api.typingrealm.com/typing';
    let TYPINGDUELS_URL = 'https://api.typingrealm.com/typingduels';
    let IDENTITY_SERVER_URL = 'https://api.typingrealm.com/identityserver';
    let useDevAuth0Client = true;
    let env = url.searchParams.get('env');
    let language = url.searchParams.get('language');
    if (language == undefined) language = "en";
    setUrls();

    let maxShouldContainErrors = url.searchParams.get('maxShouldContainErrors');
    if (!maxShouldContainErrors) maxShouldContainErrors = 10;
    let maxShouldContainSlow = url.searchParams.get('maxShouldContainSlow');
    if (!maxShouldContainSlow) maxShouldContainSlow = 10;

    function setUrls() {
        if (!env || env == 'prod') {
            env = 'prod';
            useDevAuth0Client = false;
            return;
        }

        if (env == 'dev') {
            PROFILES_URL = 'https://dev.api.typingrealm.com/profiles';
            DATA_URL = 'https://dev.api.typingrealm.com/typing';
            TYPINGDUELS_URL = 'https://dev.api.typingrealm.com/typingduels';
            IDENTITY_SERVER_URL = 'https://dev.api.typingrealm.com/identityserver';
        }

        if (env == 'local') {
            PROFILES_URL = 'https://api.localhost/profiles';
            DATA_URL = 'https://api.localhost/typing';
            TYPINGDUELS_URL = 'https://api.localhost/typingduels';
            IDENTITY_SERVER_URL = 'https://api.localhost/identityserver';
        }

        if (env == 'debug') {
            PROFILES_URL = 'http://localhost:30103';
            DATA_URL = 'http://localhost:30403';
            TYPINGDUELS_URL = 'http://127.0.0.1:30404';
            IDENTITY_SERVER_URL = 'http://127.0.0.1:30000';
        }
    }

    let ackResolves = {};
    async function sendWithAck(msg) {
        let messageId = (uniqueMessageIdCounter++).toString(); // should be unique every time;
        if (uniqueMessageIdCounter > 10000) {
            uniqueMessageIdCounter = 0;
        }

        let ackResolve;
        msg.metadata = {
            messageId: messageId,
            acknowledgementType: 2 // TODO: Find out why 3 doesn't work from backend side.
        };

        let cancelReject;
        const myPromise = new Promise((resolve, reject) => {
            ackResolve = resolve;
            cancelReject = setTimeout(() => {
                reject();
                console.error('rejected, did not succeed to ack in time.');
            }, 3000);
        });

        ackResolves[messageId] = {
            resolve: ackResolve,
            cancelReject: () => clearTimeout(cancelReject)
        };

        await connection.invoke("Send", msg);
        try {
            await myPromise;
            console.log('successfully accepted ack');
        }
        catch (error) {
            console.error('failed to get ack');
            console.error(error);
        }
    }

    async function connectToTypingDuels() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(`${TYPINGDUELS_URL}/hub`,  { accessTokenFactory: () => getRealtimeToken() })
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // TODO: Subscribe earlier, as soon as connection is established.
        connection.on("Send", (message) => {
            console.log(message);

            if (message.typeId == "TypingRealm.Messaging.Messages.AcknowledgeReceived") {
                if (message.metadata && message.metadata.messageId) {
                    let resolveData = ackResolves[message.metadata.messageId];
                    if (resolveData) {
                        resolveData.resolve();
                        resolveData.cancelReject();
                        console.log('accepted ack and canceled reject');
                    }
                }
            }

            handle(message);
        });

        async function start() {
            try {
                let user = await auth0.getUser();
                authUserIdKey = user.sub.replace('|', '_');

                await connection.start();

                console.log("SignalR Connected.");

                var accessToken = await getToken();
                await sendWithAck({
                    data: JSON.stringify({
                        accessToken: accessToken
                    }),
                    typeId: "TypingRealm.Authentication.Service.Messages.Authenticate"
                });
                console.log('finished waiting for sending for ack');
                console.log("TypingDuels Authenticated.");
            } catch (err) {
                console.error(err);
                setTimeout(start, 5000);
            }
        };

        connection.onclose(async () => {
            await start();
        });

        // Start the connection.
        await start();
    }

    async function getRealtimeToken() {
        let token = await getToken();
        let response = await fetch(`${TYPINGDUELS_URL}/api/realtime-auth/generate`, {
            method: "POST",
            headers: {
                'Authorization': `Bearer ${token}`
            }
        })
            .then(response => response.text());

        return response;
    }

    const TEXT_GENERATION_URL = `${DATA_URL}/api/texts/generate`;

    let length = url.searchParams.get('length');
    let textType = url.searchParams.get('textStructure');
    if (textType != 'words' && textType != 'text') {
        textType = 'text';
    }

    async function forceLogin() {
        await auth0.loginWithPopup();
    }

    async function getProfileFromToken() {
        const user = await auth0.getUser();
        profile = user.sub;

        return profile;
    }

    if (!profile) {
        if (env == 'prod') {
            auth0 = await createAuth0Client({
                domain: 'typingrealm.us.auth0.com',
                client_id: 'usmQTpTvmVrxC4QtYMYj6R7aIa6Ambck',
                useRefreshTokens: true
            });
        } else {
            auth0 = await createAuth0Client({
                domain: 'dev-typingrealm.eu.auth0.com',
                client_id: 'MmL3eIAJPW7wweAWajjqgWRM8xaVqRn2',
                useRefreshTokens: true
            });
        }

        await forceLogin();
        profile = await getProfileFromToken();
    }

    if (!length) {
        // Set default length as 100 characters.
        length = 100;
    }

    if (!profile || !length)
        throw new Error("Cannot continue without specifying 'profile' and 'length'.");

    console.log(`Using profile "${profile}" and minimum text length ${length}. ${textType} mode.`);

    /* Global variables */

    const duelsElement = document.getElementById('duels');
    const textElement = document.getElementById('text');
    const speedElement = document.getElementById('speed');
    const simulationSpeedMultiplierElement = document.getElementById('simulationSpeedMultiplier');
    const hintElement = document.getElementById('hint');
    const reportElement = document.getElementById('report');
    const textRequestUrl = `${TEXT_GENERATION_URL}?length=${length}&textStructure=${textType}&language=${language}&maxShouldContainErrors=${maxShouldContainErrors}&maxShouldContainSlow=${maxShouldContainSlow}`;
    //const authTokenRequestUrl = `${PROFILES_URL}/api/local-auth/profile-token?sub=${profile}`;
    const authTokenRequestUrl = `${IDENTITY_SERVER_URL}/api/local/user-token?sub=${profile}`;
    const dataSubmitUrl = `${DATA_URL}/api/usersessions/result`;
    const getOverallReportUrl = `${DATA_URL}/api/usersessions/statistics/readable?language=${language}`;

    const typingSessionsApi = `${DATA_URL}/api/typingsessions`;
    const userSessionsApi = `${DATA_URL}/api/usersessions`;

    function getTextUrl(textId) {
        return `${DATA_URL}/api/texts/${textId}`;
    }

    // If it's false - any input doesn't affect the state.
    let allowInput = false;

    // Initial text value. Used to simulate everything from scratch at the end.
    // Contains:
    // text - text value.
    // startTime - initialized once typing has begun.
    // startPerf - initialized once typing has begun.
    let textData = {};

    // A list of characters of the given text where every span is an element of DOM where classes can be applied.
    let characterSpans = [];

    // A list of objects for every character that holds information about typing state of the character.
    // Should be separate from characterSpans because characterSpans is DOM concern, and textToType is business logic concern.
    // Contains:
    // character - text representation of the character that we need to type.
    // typed - boolean indicating whether this character has been typed CORRECTLY (without error)
    // span - current character span, for convenience
    // error - number of times this character has been typed incorrectly: if it's more than 0 - it's forever tainted with special color.
    // isCurrentError - boolean indicating that it needs to be corrected, it's wrong right now at this moment; alternative of "typed"
    let textToType = [];

    // Index of the character where the caret at right now (what character are we going to type when we press a key).
    let index = 0;

    // Main core feature of this tool. Contains the whole list of all events occurred during typing.
    // Contains:
    // key - symbol that has been typed, or "backspace", or "shift"
    // perf - performance metric
    // index - index of the character on which it has been typed / released
    // keyAction - 'Press' or 'Release'
    let events = [];

    // Events saved for future: for simulation purposes.
    let simulationEvents = [];

    // When simulation is happening - this is set to true.
    let isSimulation = false;
    let stopSimulation = false; // This is a flag to set when we want to interrupt simulation.
    let simulationSpeedMultiplier = 1;

    const clientWidths = {};




    let userSessionId = url.searchParams.get('userSessionId');
    if (!userSessionId) {
        let typingSessionId = url.searchParams.get('typingSessionId');
        if (!typingSessionId) {
            // Generate typing session.

            let token = await getToken();
            const response = await fetch(typingSessionsApi, {
                method: "POST",
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                })
            }).then(r => r.json());

            typingSessionId = response.typingSessionId;

            console.log('Successfully submitted typing result.');
            console.log(response);



            if (window.location.search === '') {
                window.location = window.location.origin + `?typingSessionId=${typingSessionId}`
            } else {
                window.location = window.location.origin + window.location.search + `&typingSessionId=${typingSessionId}`
            }

            return;
        }

        // Generate user session.

        let token = await getToken();
        const response = await fetch(userSessionsApi, {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                typingSessionId: typingSessionId,
                userTimeZoneOffsetMinutes: 0 // TODO: Supply proper value.
            })
        }).then(r => r.json());

        userSessionId = response.userSessionId;



        if (window.location.search === '') {
            window.location = window.location.origin + `?userSessionId=${userSessionId}`
        } else {
            window.location = window.location.origin + window.location.search + `&userSessionId=${userSessionId}`
        }

        return;
    }






    document.addEventListener('keydown', processKeyDown);
    document.addEventListener('keyup', processKeyUp);


    let connection;
    connectToTypingDuels();
    await renderNewText();

    async function handle(message) {
        if (message.typeId == 'TypingRealm.Authentication.Service.Messages.TokenExpired') {
            console.log('Re-sending a new token.');

            let accessToken = await getToken();
            await connection.invoke('Send', {
                data: JSON.stringify({
                    accessToken: accessToken
                }),
                typeId: 'TypingRealm.Authentication.Service.Messages.Authenticate'
            })
        }

        if (message.typeId == 'TypingRealm.TypingDuels.Typed') {
            let data = JSON.parse(message.data);

            drawTypedStatus(data.clientId.replace('|', '_'), data.typedCharactersCount);
        }
    }

    function drawTypedStatus(clientId, typedCharactersCount) {
        clientWidths[clientId] = typedCharactersCount / 3;

        for (var key in clientWidths) {
            var value = clientWidths[key];

            let element = duelsElement.querySelector(`#${key}`);
            if (!element) {
                element = document.createElement('div');
                element.innerText = key;
                element.style.width = `${value}%`;
                element.style.backgroundColor = 'red';
                element.id = key;
                duelsElement.appendChild(element);
            } else {
                element.style.width = `${value}%`;
            }
        }

        console.log('updated status for client ' + clientId);
    }

    function isKeySymbol(event) {
        return (event.keyCode >= 48 && event.keyCode <= 57) || (event.keyCode >= 65 && event.keyCode <= 90)
            || (event.keyCode == 32 /* space */) || (event.keyCode >= 106 && event.keyCode <= 111 /* signs */)
            || (event.keyCode >= 186 && event.keyCode <= 192 /* punctuation */) || (event.keyCode >= 219 && event.keyCode <= 222 /* quotes */);
    }

    function isKeyBackspace(event) {
        return event.keyCode == 8;
    }

    function isKeyTab(event) {
        return event.keyCode == 9;
    }

    function isKeyEnter(event) {
        return event.keyCode == 13;
    }

    function isKeyShift(event) {
        return event.keyCode == 16;
    }

    function GetKeyCode(key) {
        if (key == "backspace") {
            return 8;
        }

        if (key == "tab") {
            return 9;
        }

        if (key == "enter") {
            return 13;
        }

        if (key == "shift") {
            return 16;
        }

        return 50;
    }

    async function generateText() {
        let token = await getToken();

        let response = await fetch(textRequestUrl, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        let textId = await response.text();
        return textId;
    }

    async function getText(textId) {
        let token = await getToken();

        let textResponse = await fetch(getTextUrl(textId), {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        let text = await textResponse.json();

        return text;
    }

    async function renderNewText() {
        if (allowInput) {
            throw new Error("Can't render new text during allowed input period. First finish typing the current one.");
        }

        textData = {};
        characterSpans = [];
        textToType = [];
        index = 0;
        events = [];
        simulationEvents = [];

        textElement.innerHTML = '<div class="loader"></div>';
        speedElement.innerHTML = '';
        reportElement.innerHTML = '';
        simulationSpeedMultiplierElement.innerHTML = '';
        hintElement.classList.add('hidden');

        let textId = await generateText();
        textData.text = await getText(textId);

        textElement.innerHTML = '';
        textData.text.value.split('').forEach((character, index) => {
            const characterSpan = document.createElement('span');
            characterSpan.innerText = character;
            textElement.appendChild(characterSpan);
            characterSpans.push(characterSpan);

            textToType.push({
                character: character,
                typed: false,
                span: characterSpan,
                error: 0,
                isCurrentError: false
            });
        });

        // A hack to include one more hidden span at the end of the text, so that the caret can move there
        // once the text has been typed but some errors are still present, and we can press backspace to go back.
        const endCaret = document.createElement('span');
        endCaret.classList.add('hidden');
        textElement.appendChild(endCaret);
        characterSpans.push(endCaret);

        // Indicates that everything has been set up and now user input can start to be processed.
        allowInput = true;
    }

    function getTextSimulationSpeedMultiplier() {
        if (simulationSpeedMultiplier == 1) {
            return "Normal speed"
        }

        if (simulationSpeedMultiplier > 1) {
            return `${simulationSpeedMultiplier}x times slower`;
        }

        if (simulationSpeedMultiplier == 0.9) {
            return '11% faster';
        }

        if (simulationSpeedMultiplier == 0.8) {
            return '25% faster';
        }

        if (simulationSpeedMultiplier == 0.666) {
            return '50% faster';
        }

        if (simulationSpeedMultiplier == 0.5) {
            return '2x faster';
        }
    }

    async function processKeyDown(event, simulation) {
        if (isSimulation && !allowInput) {
            if (isKeyEnter(event)) {
                stopSimulation = true;
            }

            // 1 - regular speed.
            // 1.5 - 1.5x times slower.
            // 2 - 2x times slower.
            // 2.5 - 2.5x times slower.
            // 3 - 3x times slower.
            if (event.key == 's' && !simulation) {
                simulationSpeedMultiplier = calculateSlowerMultiplier(simulationSpeedMultiplier);
                simulationSpeedMultiplierElement.innerHTML = getTextSimulationSpeedMultiplier();
            }

            // 1 - regular speed.
            // 0.9 - 11% faster.
            // 0.8 - 25% faster.
            // 0.666 - 50% faster.
            // 0.5 - 2x times faster.
            if (event.key == 'f' && !simulation) {
                simulationSpeedMultiplier = calculateFasterMultiplier(simulationSpeedMultiplier);
                simulationSpeedMultiplierElement.innerHTML = getTextSimulationSpeedMultiplier();
            }

            function calculateFasterMultiplier(simulationSpeedMultiplier) {
                if (simulationSpeedMultiplier == 1) return 0.9;
                if (simulationSpeedMultiplier == 0.9) return 0.8;
                if (simulationSpeedMultiplier == 0.8) return 0.666;
                if (simulationSpeedMultiplier == 0.666) return 0.5;

                if (simulationSpeedMultiplier > 1) return simulationSpeedMultiplier - 0.5;

                return simulationSpeedMultiplier;
            }

            function calculateSlowerMultiplier(simulationSpeedMultiplier) {
                if (simulationSpeedMultiplier == 0.5) return 0.666;
                if (simulationSpeedMultiplier == 0.666) return 0.8;
                if (simulationSpeedMultiplier == 0.8) return 0.9;
                if (simulationSpeedMultiplier == 0.9) return 1;

                if (simulationSpeedMultiplier >= 1 && simulationSpeedMultiplier < 3) return simulationSpeedMultiplier + 0.5;

                return simulationSpeedMultiplier;
            }
        }

        if (!allowInput && !simulation) {
            // !!! Do not allow ANY kind of input once we set allowInput = false,
            // and when simulation is not sending true here.
            return;
        }

        let perf = performance.now();

        if (isKeySymbol(event)) {
            if (!textData.startTime) {
                startTyping(perf);
            }

            await typeSymbol(event.key, perf);
            if (!simulation) {
                sendRealtimeThrottled();
            }
            return;
        }

        if (isKeyShift(event)) {
            if (!textData.startTime) {
                startTyping(perf);
            }

            pressShift(perf);
            return;
        }

        if (textData.startTime) {
            if (isKeyBackspace(event)) {
                pressBackspace(perf);
                if (!simulation) {
                    sendRealtimeThrottled();
                }
                return;
            }
        }
    }

    // Do not await when calling this, best performance.
    let timerId = undefined;
    async function sendRealtimeThrottled() {
        if (timerId) return;

        timerId = setTimeout(async () => {
            timerId = undefined;
            await sendRealtime();
        }, 100);
    }

    // Do not await when calling this, best performance.
    async function sendRealtime() {

        await connection.invoke("Send", {
            data: JSON.stringify({
                typedCharactersCount: index
            }),
            typeId: "TypingRealm.TypingDuels.Typed"
        });

        // Draw self status.
        drawTypedStatus(authUserIdKey, index);
    }

    function processKeyUp(event, simulation) {
        if (!allowInput && !simulation) {
            // !!! Do not allow ANY kind of input once we set allowInput = false,
            // and when simulation is not sending true here.
            return;
        }

        if (!textData.startTime || isSimulation) {
            // Do not process any keyup events until typing has started, or whenever there is a simulation going on.
            return;
        }

        let perf = performance.now();

        if (isKeySymbol(event)) {
            releaseKey(event.key, perf);
            return;
        }

        if (isKeyBackspace(event)) {
            releaseBackspace(perf);
            return;
        }

        if (isKeyShift(event)) {
            releaseShift(perf);
            return;
        }
    }

    function startTyping(perf) {
        textData.startTime = Date.now();
        textData.startPerf = perf;

        // Put a caret on the first character.
        characterSpans[index].classList.add('cursor');
    }

    async function typeSymbol(symbol, perf) {
        if (index == textToType.length) {
            return; // Do not log any character presses once the text has been typed till the end.
        }

        let currentSymbol = textToType[index];
        let currentSpan = currentSymbol.span;

        events.push({
            key: symbol,
            perf: perf,
            index: index,
            keyAction: 'Press'
        });

        // Move to the next character.
        currentSpan.classList.remove('cursor');

        if (currentSymbol.character == symbol) {
            currentSymbol.typed = true;

            // Remove wrong classes if any.
            if (currentSymbol.error > 0) {
                if (currentSymbol.character == " ") {
                    currentSpan.classList.remove('space-was-wrong');
                } else {
                    currentSpan.classList.remove('was-wrong');
                }
                currentSpan.classList.remove('wrong');

                currentSpan.classList.add('corrected');
            } else {
                currentSpan.classList.add('typed');
            }
        } else {
            currentSymbol.typed = false;
            currentSymbol.error++;
            currentSymbol.isCurrentError = true;

            currentSpan.classList.remove('typed');
            currentSpan.classList.remove('corrected');
            if (currentSymbol.character == " ") {
                currentSpan.classList.remove('space-was-wrong');
            } else {
                currentSpan.classList.remove('was-wrong');
            }
            currentSpan.classList.add('wrong');
        }

        index++;
        if (index == textToType.length && textToType.filter(x => x.isCurrentError).length == 0) {
            if (isSimulation) {
                // Continue simulation. Do not send data again.
                simulateTyping();
                return;
            }

            allowInput = false; // As soon as we finish typing - do not allow input anymore.

            const statistics = makeSaveDataRequest();

            reportElement.innerHTML = '<div class="loader"></div>';
            sendData(statistics).then(() => {
                getOverallReport().then(data => {
                    reportElement.innerHTML = '';
                    let lines = data.split('\n');
                    for (let i =0; i < lines.length; i++) {
                        const lineDiv = document.createElement('div');
                        const line = lines[i];
                        lineDiv.innerHTML = line;
                        reportElement.appendChild(lineDiv);
                    }
                }, err => {
                    reportElement.innerHTML = '';
                });
            });

            var speedCpm = (60000 * textToType.length / statistics.events.at(-1).absoluteDelay).toFixed(2);
            var speedWpm = (speedCpm / 5).toFixed(2);

            speedElement.innerHTML = `${speedCpm} CPM (${speedWpm})`;
            simulationSpeedMultiplierElement.innerHTML = getTextSimulationSpeedMultiplier();
            hintElement.classList.remove('hidden');

            simulateTyping();

            return;
        }

        // Move cursor to the next character.
        currentSpan = characterSpans[index];
        currentSpan.classList.add('cursor');
    }

    function releaseKey(symbol, perf) {
        events.push({
            key: symbol,
            perf: perf,
            index: index,
            keyAction: 'Release'
        });
    }

    function pressBackspace(perf) {
        if (index == 0) {
            events.push({
                key: 'backspace',
                perf: perf,
                index: 0,
                keyAction: 'Press'
            });

            return;
        }

        // Taking from characterSpans instead of textToType because we might be taking the last hidden span here.
        let currentSpan = characterSpans[index];

        events.push({
            key: 'backspace',
            perf: perf,
            index: index,
            keyAction: 'Press'
        });

        // Remove cursor from the current character to move it to the previous.
        currentSpan.classList.remove('cursor');

        index--;

        let currentSymbol = textToType[index];
        currentSpan = currentSymbol.span;

        if (currentSymbol.isCurrentError) {
            // Remove error: we are going to type it again.
            currentSymbol.isCurrentError = false;
        }

        // Add cursor to the new current span.
        currentSpan.classList.add('cursor');

        if (currentSymbol.error > 0) {
            currentSpan.classList.remove('wrong');
            currentSpan.classList.remove('corrected');

            if (currentSymbol.character == " ") {
                currentSpan.classList.add('space-was-wrong');
            } else {
                currentSpan.classList.add('was-wrong');
            }
        } else {
            currentSpan.classList.remove('typed');
        }
    }

    function releaseBackspace(perf) {
        events.push({
            key: 'backspace',
            perf: perf,
            index: index,
            keyAction: 'Release'
        });
    }

    function pressShift(perf) {
        events.push({
            key: 'shift',
            perf: perf,
            index: index,
            keyAction: 'Press'
        });
    }

    function releaseShift(perf) {
        events.push({
            key: 'shift',
            perf: perf,
            index: index,
            keyAction: 'Release'
        });
    }

    async function simulateTyping() {
        allowInput = false;
        isSimulation = true;

        if (simulationEvents.length == 0) {
            // Fill it only once when starting simulation first time, then just reuse.

            let lastAbsoluteDelay = 0;
            for (var i = 0; i < events.length; i++) {
                let event = events[i];

                simulationEvents.push({
                    key: event.key,
                    delay: event.absoluteDelay - lastAbsoluteDelay,
                    index: event.index,
                    keyAction: event.keyAction
                });

                lastAbsoluteDelay = event.absoluteDelay;
            }
        }

        await renderAgain();

        for (const e of simulationEvents) {
            await new Promise(resolve => setTimeout(resolve, e.delay * simulationSpeedMultiplier));

            if (e.keyAction == "Press") {
                await processKeyDown({
                    key: e.key,
                    keyCode: GetKeyCode(e.key)
                }, true);
            } else {
                processKeyUp({
                    key: e.key,
                    keyCode: GetKeyCode(e.key)
                }, true);
            }

            if (stopSimulation) {
                await renderNewText();
                stopSimulation = false;
                isSimulation = false;
                return;
            }
        }
    }

    async function renderAgain() {
        textData = {
            text: textData.text
        };

        characterSpans = [];
        textToType = [];
        index = 0;
        events = [];

        textElement.innerHTML = '';

        textData.text.value.split('').forEach((character, index) => {
            const characterSpan = document.createElement('span');
            characterSpan.innerText = character;
            textElement.appendChild(characterSpan);
            characterSpans.push(characterSpan);

            textToType.push({
                character: character,
                typed: false,
                span: characterSpan,
                error: 0,
                isCurrentError: false
            });
        });

        const endCaret = document.createElement('span');
        endCaret.classList.add('hidden');
        textElement.appendChild(endCaret);
        characterSpans.push(endCaret);

        // Put a caret on the first character.
        characterSpans[index].classList.add('cursor');
    }

    async function getToken() {
        if (auth0) {
            try {
                return await auth0.getTokenSilently();
            } catch {
                await forceLogin();
                return await auth0.getTokenSilently();
            }
        }

        let response = await fetch(authTokenRequestUrl, {
            method: "POST"
        })
            .then(response => response.json());

        return response.access_token;
    }

    async function sendData(data) {
        let token = await getToken();

        console.log(data);
        const response = await fetch(dataSubmitUrl, {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(data)
        }).then(r => r.json());

        console.log('Successfully submitted typing result.');
        console.log(response);
    }

    async function getOverallReport() {
        let token = await getToken();

        const response = await fetch(getOverallReportUrl, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        }).then(x => x.text());

        return response;
    }

    function makeSaveDataRequest() {
        let request = {
            textId: textData.text.textId,
            startedTypingUtc: new Date(textData.startTime).toISOString(),
            userTimeZoneOffsetMinutes: new Date(textData.startTime).getTimezoneOffset() * -1,

            events: events.map(function (event) {
                event.perf = event.perf - textData.startPerf;
                return event;
            })
        };

        for (var i = 0; i < request.events.length; i++) {
            let event = request.events[i];

            event.absoluteDelay = event.perf;

            delete event.perf;
        }

        return request;
    }
})();