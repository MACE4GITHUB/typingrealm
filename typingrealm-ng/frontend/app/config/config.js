const baseUri = 'http://localhost:30101';
const typingApiUri = baseUri;
const typingApi = {
    uri: typingApiUri,
    typingSubmitEndpoint: `${typingApiUri}/api/typing`
};

const authApiUri = typingApiUri;
const authApi = {
    uri: authApiUri,
    tokenEndpoint: `${authApiUri}/api/auth/token`
};

const textsApiUri = typingApiUri;
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
