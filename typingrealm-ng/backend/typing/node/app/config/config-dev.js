export default {
    corsOrigins: [
        'http://localhost:8080',
        'http://localhost:30080',
        'https://localhost',
        'https://typingrealm.com',
        'https://dev.typingrealm.com'
    ],
    typingApiPort: 80,
    typingDbConnectionString: process.env.DATABASE_URL,
    cacheConnectionString: process.env.CACHE_URL
}
