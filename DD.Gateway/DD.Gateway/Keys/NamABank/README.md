# Nam A Bank eKYC public keys

Place the public keys for the current environment in this folder using these filenames:

- `NAB_Request_Public.pem`: NAB public key for encrypting the deeplink request.
- `NAB_Callback_Public.pem`: NAB public key for verifying the signed callback JWS.

Both paths are configured in `DD.Gateway/appsettings.json` relative to the application's content root. The files currently included are the UAT keys supplied by NAB. When deploying to another environment, replace **both** files with that environment's corresponding public keys and set `kid` in `appsettings.json` to the exact value supplied by NAB. The PEM itself does not contain the `kid`.

The project copies `*.pem` files in this folder to build and publish output. Store public keys only; do not place private keys here.
