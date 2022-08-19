export default {
    cors: [
        'http://localhost:8080',
        'http://localhost:30080',
        'https://localhost',
        'https://typingrealm.com',
        'https://dev.typingrealm.com'
    ],
    //port: 80, // TODO: Specify custom prot here for local debug, but be able to deploy it as Local docker container.
    typingDbConnectionString: 'postgres://postgres:admin@host.docker.internal:12432/typing?sslmode=disable',
    cacheConnectionString: 'redis://host.docker.internal:12379'
}
