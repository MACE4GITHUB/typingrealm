export default {
    cors: [
        'http://localhost:8080',
        'http://localhost:30080',
        'https://typingrealm.com',
        'https://dev.typingrealm.com'
    ],
    typingApiPort: 80,
    typingDbConnectionString: process.env.DATABASE_URL
}
