/*
  Run in pgAdmin Query Tool against the dd_gateway database after
  CreateGatewayTables.sql. Change only the arguments in the final SELECT.

  Existing active partner: reuse it and issue a new credential.
  New partner: create it and issue its first credential.
  The final result displays the secret key. Both the secret key and its
  SHA-256/Base64 hash are stored in PartnerCredentials.
*/

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE OR REPLACE FUNCTION pg_temp.create_partner_credential(
    p_partner_code text,
    p_partner_name text,
    p_environment text,
    p_scopes text DEFAULT 'all',
    p_expires_on date DEFAULT NULL,
    p_callback_url text DEFAULT NULL
)
RETURNS TABLE (
    "PartnerId" integer,
    "PartnerCode" text,
    "CallbackUrl" text,
    "CredentialId" integer,
    "AccessKey" text,
    "SecretKey" text,
    "Scopes" text,
    "ExpiresAt" timestamp with time zone
)
LANGUAGE plpgsql
AS $function$
DECLARE
    v_code text := upper(btrim(p_partner_code));
    v_name text := btrim(p_partner_name);
    v_environment text := lower(btrim(p_environment));
    v_callback_url text := btrim(p_callback_url);
    v_scopes text;
    v_partner_id integer;
    v_partner_status integer;
    v_credential_id integer;
    v_access_key text;
    v_secret_key text;
    v_expires_at timestamp with time zone;
    v_now timestamp with time zone := now();
BEGIN
    IF v_code IS NULL OR v_code !~ '^[A-Z0-9_-]{1,50}$' THEN
        RAISE EXCEPTION 'Partner code must contain 1-50 letters, digits, hyphens or underscores.';
    END IF;
    IF v_name IS NULL OR length(v_name) NOT BETWEEN 1 AND 200 THEN
        RAISE EXCEPTION 'Partner name must contain 1-200 characters.';
    END IF;
    IF v_environment IS NULL OR v_environment NOT IN ('uat', 'prod') THEN
        RAISE EXCEPTION 'Environment must be uat or prod.';
    END IF;
    IF p_callback_url IS NOT NULL AND (
        v_callback_url IS NULL OR v_callback_url = '' OR length(v_callback_url) > 2048
        OR v_callback_url !~* '^https?://[^[:space:]/?#]+'
    ) THEN
        RAISE EXCEPTION 'Callback URL must be an absolute HTTP/HTTPS URL of at most 2048 characters.';
    END IF;

    IF p_scopes IS NULL OR lower(btrim(p_scopes)) = 'all' THEN
        v_scopes := 'ekyc.deeplink.create';
    ELSE
        IF EXISTS (
            SELECT 1
            FROM regexp_split_to_table(p_scopes, ',') AS scope_name
            WHERE btrim(scope_name) NOT IN (
                'ekyc.deeplink.create'
            )
        ) THEN
            RAISE EXCEPTION 'Scopes must contain only supported eKYC scopes.';
        END IF;

        SELECT string_agg(DISTINCT btrim(scope_name), ',' ORDER BY btrim(scope_name))
        INTO v_scopes
        FROM regexp_split_to_table(p_scopes, ',') AS scope_name;

        IF v_scopes IS NULL OR length(v_scopes) > 1000 THEN
            RAISE EXCEPTION 'At least one valid scope is required.';
        END IF;
    END IF;

    IF p_expires_on IS NOT NULL THEN
        -- Expires at 00:00 UTC on the day after the chosen date.
        v_expires_at := (p_expires_on + 1)::timestamp AT TIME ZONE 'UTC';
        IF v_expires_at <= v_now THEN
            RAISE EXCEPTION 'Expiry date must be today or later (UTC).';
        END IF;
    END IF;

    INSERT INTO "Partners" ("Code", "Name", "CallbackUrl", "Status", "CreatedAt", "UpdatedAt")
    VALUES (v_code, v_name, v_callback_url, 1, v_now, v_now)
    ON CONFLICT ("Code") DO NOTHING
    RETURNING "Id" INTO v_partner_id;

    IF v_partner_id IS NULL THEN
        SELECT p."Id", p."Status"
        INTO v_partner_id, v_partner_status
        FROM "Partners" AS p
        WHERE p."Code" = v_code;

        IF v_partner_id IS NULL OR v_partner_status <> 1 THEN
            RAISE EXCEPTION 'Partner % is missing or disabled.', v_code;
        END IF;

        IF v_callback_url IS NOT NULL THEN
            UPDATE "Partners"
            SET "CallbackUrl" = v_callback_url, "UpdatedAt" = v_now
            WHERE "Id" = v_partner_id;
        END IF;
    END IF;

    SELECT p."CallbackUrl" INTO v_callback_url
    FROM "Partners" AS p WHERE p."Id" = v_partner_id;

    v_access_key := 'dd_' || v_environment || '_' || encode(gen_random_bytes(16), 'hex');
    v_secret_key := encode(gen_random_bytes(32), 'hex');

    INSERT INTO "PartnerCredentials" (
        "PartnerId", "AccessKey", "SecretKey", "SecretKeyHash", "Scopes", "Status",
        "ExpiresAt", "LastUsedAt", "RevokedAt", "CreatedAt", "UpdatedAt"
    )
    VALUES (
        v_partner_id,
        v_access_key,
        v_secret_key,
        encode(digest(convert_to(v_secret_key, 'UTF8'), 'sha256'), 'base64'),
        v_scopes,
        1,
        v_expires_at,
        NULL,
        NULL,
        v_now,
        v_now
    )
    RETURNING "Id" INTO v_credential_id;

    RETURN QUERY
    SELECT v_partner_id, v_code, v_callback_url, v_credential_id, v_access_key,
           v_secret_key, v_scopes, v_expires_at;
END;
$function$;

-- Edit these arguments: partner code, name, environment, scopes, expiry date, callback URL.
-- Use NULL for no expiry or callback URL. Use 'all' for all supported eKYC scopes.
SELECT * FROM pg_temp.create_partner_credential(
    'DOIDEP',
    'Doidep',
    'uat',
    'all',
    NULL,
    NULL -- Replace with your HTTPS callback URL in quotes, for example 'https://your-domain.example/callback'.
);
