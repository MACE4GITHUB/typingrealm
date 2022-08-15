const authAreaElement = document.getElementById('auth');

export default class Auth {
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

        console.log('handled auth', this.#idToken);
    }

    #isExpiring() {
        // TODO: Check expiration.
        return !this.#idToken;
    }
}
