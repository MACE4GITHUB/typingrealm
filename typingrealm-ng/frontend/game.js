import authFactory from './app/auth.js';
import listen from './app/listener.js';

// TODO: move onload & google objects to global.d.ts.
// eslint-disable-next-line no-undef
window.onload = async function game() {
    // eslint-disable-next-line no-undef
    const auth = authFactory(google.accounts.id, '400839590162-k6q520pk6lqs3vee6u18cdfhvtd07hf7');
    await auth.getToken();

    await listen();
};
