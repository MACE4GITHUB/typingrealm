const baseUri = 'https://api.typingrealm.com';
const typingApiUri = `${baseUri}/typing`;
const typingApi = {
    uri: typingApiUri,
    typingSubmitEndpoint: `${typingApiUri}/api/typing`,
    typingAnalyzeAllEndpoint: `${typingApiUri}/api/typing/analyze-all`,
    typingGlobalStatisticsEndpoint: `${typingApiUri}/api/typing/global-statistics`
};

const authApiUri = typingApiUri;
const authApi = {
    uri: authApiUri,
    tokenEndpoint: `${authApiUri}/api/auth/token`
};

const textsApiUri = `${baseUri}/texts`;
const textsApi = {
    uri: textsApiUri,
    generateTextEndpoint: `${textsApiUri}/api/texts`
};

const config = {
    typingApi,
    authApi,
    textsApi
};

export default config;
