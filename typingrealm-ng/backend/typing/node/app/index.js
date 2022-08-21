import startHost from '@typingrealm/host';
import registerTypingApp from './typing/index.js';
import registerAuthApp from './auth/index.js';

startHost(app => {
    registerTypingApp(app);
    registerAuthApp(app);
});
