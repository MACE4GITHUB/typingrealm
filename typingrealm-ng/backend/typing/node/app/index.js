import startHost from '@typingrealm/host';
import config from './config/config.js';
import registerTypingApp from './typing/index.js';
import registerAuthApp from './auth/index.js';

startHost(config.port, config.corsOrigins, app => {
    registerTypingApp(app);
    registerAuthApp(app);
});
