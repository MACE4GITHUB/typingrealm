import startHost from '@typingrealm/host';
import config from './config/config.js';
import registerTextsApp from './texts/index.js';

startHost(config.port, config.corsOrigins, registerTextsApp);
