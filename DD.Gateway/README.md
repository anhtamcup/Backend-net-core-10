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
{"provider":"NAMABANK","partnerID":"PARTNER","callbackUrl":"partnerapp://ekyc/callback"}
```

The `provider` selects a registered eKYC provider; `NAMABANK` is currently supported. Provider integrations implement `IEkycProvider` and are registered in dependency injection, so provider-specific link creation and callback verification stay outside the controller. The response contains `provider`, `sessionId`, `issuedAt`, and `url`. DD.Gateway generates `sessionID` and `issuedAt`, stores the session and selected provider against the authenticated gateway partner, adds the generated values to the incoming values, and encrypts the resulting four-field JSON object. The partner backend should call this API with its access/secret key, then pass `url` to its mobile app to open through the operating system. The verification request also includes the selected `provider`; the gateway verifies the callback with that provider and confirms it matches the provider stored for the session. `DD.Gateway/appsettings.json` contains the NAB URL and protocol limits (`BaseUrl`, `MaximumPlaintextBytes`, `RsaKeySizeBits`, `CiphertextBytes`, `EncodedCiphertextLength`, and `MaximumCallbackResultLength`). Configure the NAB request-encryption public key through app configuration or environment variables:

```text
NamABank__Ekyc__PublicKeyPath
```

`PublicKeyPath` may be relative to the application's content root. Copy the keys for the current environment into [Keys/NamABank](DD.Gateway/Keys/NamABank/README.md), using the fixed filenames already configured in `appsettings.json`. The files currently included are the provided NAB UAT keys. The custom URL scheme in the request is distinct from `Partners.CallbackUrl`, which is an HTTP/HTTPS endpoint for partner notifications. The gateway rejects a `callbackUrl` that already contains a `result` query parameter. NAB must have registered the exact callback scheme/host/path for this `partnerID`; the gateway cannot determine NAB's registration from the request. Invalid input or a JSON payload over 190 UTF-8 bytes returns HTTP 400; missing or invalid public-key configuration returns HTTP 503. Encryption uses RSA-2048 OAEP-SHA256, and the `data` query parameter is unpadded Base64URL.

After NAB opens the custom callback URL in the Edoidep app, the app checks that the URL matches its registered callback and contains exactly one `result` parameter. It sends that parameter unchanged to its own backend, which calls `POST /api/ekyc/callback/verify` with the selected provider and gateway access/secret key:

```json
{"provider":"NAMABANK","result":"<JWS Compact from callback URL>"}
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

## DEV request log viewer

`GET /api/dev/request-logs` lets developers inspect saved gateway requests and responses, including request headers, bodies, status, duration, IP, errors, and stack traces. It supports filters for request ID, path, a substring in either request or response body, method, status, errors, and UTC time range, with pages of up to 100 records. The viewer page is at `/dev/request-logs.html` and calls the protected API from the browser.

The endpoint requires the `dev.requestlogs.read` scope. This scope is deliberately excluded from `--scopes all`, so existing partner credentials do not receive access. To create a dedicated DEV partner and issue a credential with only this scope in pgAdmin, run [CreateDevRequestLogsCredential.sql](DD.Gateway/Administration/Sql/CreateDevRequestLogsCredential.sql) against `dd_gateway`. The final query accepts the partner code, name, `uat`/`prod` environment, and optional expiry date; it creates the partner if needed and returns the access key and secret key. Running it again issues another credential.

The same page also has a **Chọn bảng SQL** tab for browsing database tables and views, searching a keyword across each selected table's rows, and paging through 50 rows at a time. Its API requires the separate `dev.database.read` scope. To use both tabs with one credential, run [CreateDevDatabaseExplorerCredential.sql](DD.Gateway/Administration/Sql/CreateDevDatabaseExplorerCredential.sql); the resulting credential has `dev.requestlogs.read,dev.database.read`. This permission can read sensitive data from every non-system table or view visible to the gateway's database account, so issue it only to trusted developers. Responses from this explorer are omitted from `RequestLogs` to avoid copying queried table contents into the log table.

Alternatively, create a credential for an existing active developer partner from the repository root:

```powershell
dotnet run --project DD.Gateway/DD.Gateway.csproj -- credential create --partner DEV-TOOLS --environment prod --scopes dev.requestlogs.read --expires 2026-12-31
```

This credential can read sensitive data from `RequestLogs`, including the `X-Secret-Key` request header values already stored there. Share it only with trusted developers. The browser page keeps the credential in memory for the current tab and does not save it to local storage.
