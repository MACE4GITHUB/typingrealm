export default {
    corsOrigins: [
        'https://typingrealm.com'
    ],
    typingApiPort: 80,
    typingDbConnectionString: process.env.DATABASE_URL,
    cacheConnectionString: process.env.CACHE_URL
}
