/*
  Run in pgAdmin Query Tool against dd_gateway after CreateGatewayTables.sql.
  Creates the DEV-TOOLS partner if absent, then issues a credential with
  only the dev.requestlogs.read scope. Running again issues a new credential.
  Copy the AccessKey and SecretKey from the result into the DEV log viewer.
*/

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE OR REPLACE FUNCTION pg_temp.create_dev_request_logs_credential(
    p_partner_code text DEFAULT 'DEV-TOOLS',
    p_partner_name text DEFAULT 'Gateway DEV Tools',
    p_environment text DEFAULT 'prod',
    p_expires_on date DEFAULT NULL
)
RETURNS TABLE (
    "PartnerId" integer,
    "PartnerCode" text,
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

    IF p_expires_on IS NOT NULL THEN
        v_expires_at := (p_expires_on + 1)::timestamp AT TIME ZONE 'UTC';
        IF v_expires_at <= v_now THEN
            RAISE EXCEPTION 'Expiry date must be today or later (UTC).';
        END IF;
    END IF;

    INSERT INTO "Partners" ("Code", "Name", "CallbackUrl", "Status", "CreatedAt", "UpdatedAt")
    VALUES (v_code, v_name, NULL, 1, v_now, v_now)
    ON CONFLICT ("Code") DO NOTHING
    RETURNING "Id" INTO v_partner_id;

    IF v_partner_id IS NULL THEN
        SELECT partner."Id", partner."Status"
        INTO v_partner_id, v_partner_status
        FROM "Partners" AS partner
        WHERE partner."Code" = v_code;

        IF v_partner_id IS NULL OR v_partner_status <> 1 THEN
            RAISE EXCEPTION 'Partner % is missing or disabled.', v_code;
        END IF;
    END IF;

    LOOP
        v_access_key := 'dd_' || v_environment || '_' || encode(gen_random_bytes(16), 'hex');
        EXIT WHEN NOT EXISTS (
            SELECT 1 FROM "PartnerCredentials" AS credential
            WHERE credential."AccessKey" = v_access_key
        );
    END LOOP;

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
        'dev.requestlogs.read',
        1,
        v_expires_at,
        NULL,
        NULL,
        v_now,
        v_now
    )
    RETURNING "Id" INTO v_credential_id;

    RETURN QUERY
    SELECT v_partner_id, v_code, v_credential_id, v_access_key,
           v_secret_key, 'dev.requestlogs.read'::text, v_expires_at;
END;
$function$;

-- Edit only these arguments. Use NULL for a credential without expiry.
SELECT * FROM pg_temp.create_dev_request_logs_credential(
    'DEV-TOOLS',
    'Gateway DEV Tools',
    'prod',
    NULL
);
