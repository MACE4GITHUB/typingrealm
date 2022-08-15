import authFactory from './auth.js';
import listen from './listener.js';

window.onload = async function () {
    const auth = authFactory(google.accounts.id, '400839590162-k6q520pk6lqs3vee6u18cdfhvtd07hf7');
    await auth.getToken();
    listen();
}
