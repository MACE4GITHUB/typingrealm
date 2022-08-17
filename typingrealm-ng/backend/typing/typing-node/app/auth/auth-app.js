import jwtDecode from 'jwt-decode';

/** This is a MOCK Auth app, it will be replaced by IdentityServer. */
export default function(app) {
    const map = new Map();

    app.post('/api/auth/token', (req, res) => {
        const token = req.body.token;
        const decoded = jwtDecode(token);
        const sub = decoded.sub;

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
