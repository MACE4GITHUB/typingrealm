const typingBaseUri = 'http://localhost:30101';
const textsBaseUri = 'http://localhost:30102';
const typingApiUri = typingBaseUri;
const typingApi = {
    uri: typingApiUri,
    typingSubmitEndpoint: `${typingApiUri}/api/typing`,
    typingAnalyzeAllEndpoint: `${typingApiUri}/api/typing/analyze-all`,
    typingGlobalStatisticsEndpoint: `${typingApiUri}/api/typing/global-statistics`
};

const authApiUri = typingBaseUri;
const authApi = {
    uri: authApiUri,
    tokenEndpoint: `${authApiUri}/api/auth/token`
};

const textsApiUri = textsBaseUri;
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
