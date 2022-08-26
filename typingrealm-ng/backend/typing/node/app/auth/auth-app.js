import { OAuth2Client } from 'google-auth-library';

const clientId = '400839590162-k6q520pk6lqs3vee6u18cdfhvtd07hf7';
const client = new OAuth2Client(clientId);
async function verify(token) {
    const ticket = await client.verifyIdToken({
        idToken: token,
        requiredAudience: clientId
    });
    const payload = ticket.getPayload();
    console.log('verified', payload);
    return payload.sub;
}

/** This is a MOCK Auth app, it will be replaced by IdentityServer. */
export default function auth(app) {
    const map = new Map();

    app.post('/api/auth/token', async (req, res) => {
        const { token } = req.body;
        const sub = await verify(token);

        let accessToken;
        if (map.has(token.sub)) {
            accessToken = map.get(sub);
            res.status(200);
        } else {
            accessToken = `mock_token_${sub}`;
            map.set(sub, accessToken);
            res.status(201);
        }

        res.send({
            access_token: accessToken
        });
    });
}
