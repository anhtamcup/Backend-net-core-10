/*
  Run in pgAdmin Query Tool against dd_gateway after applying migrations.
  Change the partner code and callback URL below. Use NULL to clear the URL.
*/

UPDATE "Partners"
SET "CallbackUrl" = 'https://your-domain.example/ekyc/callback',
    "UpdatedAt" = now()
WHERE "Code" = 'DOIDEP'
RETURNING "Id", "Code", "Name", "CallbackUrl", "UpdatedAt";
