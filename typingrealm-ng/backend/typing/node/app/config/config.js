export default {
    cors: [
        'http://localhost:8080',
        'http://localhost:30080',
        'https://localhost',
        'https://typingrealm.com',
        'https://dev.typingrealm.com'
    ],
    typingApiPort: 80,
    typingDbConnectionString: 'postgres://postgres:admin@host.docker.internal:12432/typing?sslmode=disable',
    cacheConnectionString: 'redis://host.docker.internal:12379'
}
