# DD.Gateway

ASP.NET Core gateway with PostgreSQL persistence, Swagger UI, request logging, and partner access-key authentication.

## Database setup

Set `ConnectionStrings__DefaultConnection` to the PostgreSQL connection string for your environment. The configured connection in `DD.Gateway/appsettings.json` targets a local `dd_gateway` database.

Apply the included EF Core migration before serving traffic:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12 # if dotnet ef is not installed
dotnet ef database update --project DD.Gateway/DD.Gateway.csproj
```

The migrations create the `RequestLogs`, `Partners`, `PartnerCredentials`, and `EkycSessions` tables. Configure the database user with permission to apply migrations and write request logs.

## Partner authentication

API requests use the same header pair as S3.Gateway: `X-Access-Key` and `X-Secret-Key`. Create a partner and issue a credential from the repository root:

```powershell
dotnet run --project DD.Gateway/DD.Gateway.csproj -- partner create --code DEMO --name "Demo Partner" --callback-url "https://partner.example.com/ekyc/callback"
dotnet run --project DD.Gateway/DD.Gateway.csproj -- credential create --partner DEMO --environment uat --scopes all
```

`--callback-url` is optional when creating a partner. This is an HTTP/HTTPS notification URL, distinct from the NAB app callback custom scheme. To set or change it later, run:

```powershell
dotnet run --project DD.Gateway/DD.Gateway.csproj -- partner update --code DEMO --callback-url "https://partner.example.com/ekyc/callback"
```

The credential command prints the access key and secret key. Both `SecretKey` and `SecretKeyHash` are stored in `PartnerCredentials`; existing credentials created before this change retain a null `SecretKey` because their original secret cannot be recovered from the hash. The optional `--expires yyyy-MM-dd` sets an expiry date. Both eKYC routes currently use the `ekyc.deeplink.create` scope, so existing credentials with this scope continue to work.

For pgAdmin, first run [CreateGatewayTables.sql](DD.Gateway/Administration/Sql/CreateGatewayTables.sql) in Query Tool on the `dd_gateway` database. It creates any missing gateway tables, including `EkycSessions`, even when tables were manually dropped but EF migration history remains. Then open [CreatePartnerAndCredential.sql](DD.Gateway/Administration/Sql/CreatePartnerAndCredential.sql), edit the arguments in the final `SELECT` (partner code, name, `uat`/`prod`, scopes, optional expiry date, and optional HTTP callback URL), and execute it. It returns `AccessKey` and `SecretKey`, both of which remain available in the database. Running the second file again for an active partner issues an additional credential and updates its HTTP callback URL if one is supplied.

To update only the callback URL of an existing partner from pgAdmin, edit and run [UpdatePartnerCallbackUrl.sql](DD.Gateway/Administration/Sql/UpdatePartnerCallbackUrl.sql). It does not issue a new credential.

All controller endpoints require authentication by default. Use `[Authorize(Policy = PartnerScopes.<scope>)]` on eKYC actions that need a specific scope. Swagger UI provides fields for both headers.

## eKYC module

NAB's document describes app-to-app links, not backend APIs. `EkycController` provides two partner-facing gateway APIs under `/api/ekyc`.

`POST /api/ekyc/deeplinks` requires the `ekyc.deeplink.create` scope and accepts the NAB-issued `partnerID` and registered custom `callbackUrl` from the caller:

```json
{"partnerID":"PARTNER","callbackUrl":"partnerapp://ekyc/callback"}
```

The response contains `sessionId`, `issuedAt`, and `url`. DD.Gateway generates `sessionID` and `issuedAt`, stores the session against the authenticated gateway partner, adds them to the incoming values, and encrypts the resulting four-field JSON object. The partner backend should call this API with its access/secret key, then pass `url` to its mobile app to open through the operating system. `DD.Gateway/appsettings.json` contains the NAB URL and protocol limits (`BaseUrl`, `MaximumPlaintextBytes`, `RsaKeySizeBits`, `CiphertextBytes`, `EncodedCiphertextLength`, and `MaximumCallbackResultLength`). Configure the NAB request-encryption public key through app configuration or environment variables:

```text
NamABank__Ekyc__PublicKeyPath
```

`PublicKeyPath` may be relative to the application's content root. Copy the keys for the current environment into [Keys/NamABank](DD.Gateway/Keys/NamABank/README.md), using the fixed filenames already configured in `appsettings.json`. The files currently included are the provided NAB UAT keys. The custom URL scheme in the request is distinct from `Partners.CallbackUrl`, which is an HTTP/HTTPS endpoint for partner notifications. The gateway rejects a `callbackUrl` that already contains a `result` query parameter. NAB must have registered the exact callback scheme/host/path for this `partnerID`; the gateway cannot determine NAB's registration from the request. Invalid input or a JSON payload over 190 UTF-8 bytes returns HTTP 400; missing or invalid public-key configuration returns HTTP 503. Encryption uses RSA-2048 OAEP-SHA256, and the `data` query parameter is unpadded Base64URL.

After NAB opens the custom callback URL in the partner app, the app checks that the URL matches its registered callback and contains exactly one `result` parameter. It sends that parameter unchanged to its own backend, which calls `POST /api/ekyc/callback/verify` with the gateway access/secret key:

```json
{"result":"<JWS Compact from callback URL>"}
```

The gateway verifies RS256 with the NAB **callback-signing public key**, checks the `kid` against the configured value, validates the `sessionID` and status/code pair, and matches the session to the authenticated partner. It stores the first verified result; a different subsequent result returns HTTP 409. The endpoint does not receive a direct NAB webhook. Configure each environment's callback verification `kid` and key path separately from the request-encryption key, for example:

```json
"NamABank": {
  "Ekyc": {
    "kid": "<kid-issued-by-NAB>",
    "CallbackPublicKeyPath": "Keys/NamABank/NAB_Callback_Public.pem"
  }
}
```

The callback verification public key path is set in `appsettings.json`. The PDF gives `nab-ekyc-sign-uat-01` only as an example `kid`; configure the actual `kid` supplied by NAB before using the verify endpoint. With no configured `kid`, it returns HTTP 503. The PDF does not define a backend result-query API or server-to-server callback, so a callback lost by the mobile app cannot be recovered through these two APIs alone.

## Run

```powershell
dotnet run --project DD.Gateway/DD.Gateway.csproj
```

Swagger UI is available at `/swagger`. Each incoming request writes a row to `RequestLogs`, including the path, method, all header names and values, text body, response, status, duration, IP address, and exception details. Text bodies are limited to 64 KiB; binary bodies are omitted.

Because request headers are stored without masking, `X-Secret-Key` values sent by clients are also stored in `RequestLogs`.
