import config from './config/index.js';

const authAreaElement = document.getElementById('auth');

const params = new Proxy(new URLSearchParams(window.location.search), {
    get: (searchParams, prop) => searchParams.get(prop),
});
const isBot = params.bot;

let authInstance;
export default function(google, clientId) {
    if (!authInstance) {
        authInstance = isBot
            ? { getToken: () => 'bot' }
            : new Auth(new GoogleAuth(google, clientId));
    }

    return authInstance;
}

export function tokenProvider() {
    return authInstance.getToken();
};

/** Decorator around GoogleAuth. */
class Auth {
    #auth;
    #token;

    constructor(auth) {
        this.#auth = auth;
    }

    async getToken() {
        // TODO: Implement expiration.
        if (this.#token) return this.#token;

        const idToken = await this.#auth.getToken();
        const accessTokenResponse = await fetch(config.authApi.tokenEndpoint, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                token: idToken
            })
        });

        this.#token = (await accessTokenResponse.json()).access_token;
        return this.#token;
    }
}

class GoogleAuth {
    #google;
    #idToken = null;
    #promiseResolves = [];

    constructor(google, clientId) {
        this.#google = google;
        this.#google.initialize({
            client_id: clientId,
            callback: (auth) => this.#handleAuth(auth),
            auto_select: true
        });
        this.#google.renderButton(
            document.getElementById('authButton'),
            { theme: 'outline', size: 'large' });
        this.prompt();
    }

    getToken() {
        if (this.#isExpiring()) {
            authAreaElement.style.display = 'flex';
            this.#google.prompt();

            const promise = new Promise(resolve => {
                this.#promiseResolves.push(resolve);
            });

            return promise;
        }

        return Promise.resolve(this.#idToken);
    }

    prompt() {
        this.#google.prompt();
    }

    #handleAuth(auth) {
        authAreaElement.style.display = 'none';

        this.#idToken = auth.credential;

        for (let resolve; resolve = this.#promiseResolves.pop(); ) {
            resolve(this.#idToken);
        }
    }

    #isExpiring() {
        // TODO: Check expiration.
        return !this.#idToken;
    }
}
