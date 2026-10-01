/*
    Tạo Partner và credential full quyền trong cùng transaction.
    Chỉ thay đổi các biến ở khối cấu hình bên dưới.
    SecretKey được trả về ở SELECT cuối và lưu tại PartnerCredentials.SecretKey.
    Full quyền bao gồm cả payment.history.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @PartnerCode nvarchar(50) = N'S3Local';
DECLARE @PartnerName nvarchar(200) = N'S3Local';
DECLARE @Environment varchar(10) = 'uat'; -- uat | prod
DECLARE @ExpiresAt datetime2 = NULL;       -- Ví dụ: '2027-12-31 23:59:59'

IF NULLIF(LTRIM(RTRIM(@PartnerCode)), N'') IS NULL
    THROW 50001, 'PartnerCode is required.', 1;

IF NULLIF(LTRIM(RTRIM(@PartnerName)), N'') IS NULL
    THROW 50002, 'PartnerName is required.', 1;

IF @PartnerCode LIKE N'%[^A-Za-z0-9_-]%'
    THROW 50003, 'PartnerCode contains invalid characters.', 1;

IF @Environment NOT IN ('uat', 'prod')
    THROW 50004, 'Environment must be uat or prod.', 1;

IF EXISTS (SELECT 1 FROM dbo.Partners WHERE Code = UPPER(@PartnerCode))
    THROW 50005, 'PartnerCode already exists.', 1;

DECLARE @AccessKey varchar(100) =
    CONCAT('s3_', @Environment, '_', LOWER(CONVERT(varchar(32), CRYPT_GEN_RANDOM(16), 2)));
DECLARE @SecretKey varchar(64) =
    LOWER(CONVERT(varchar(64), CRYPT_GEN_RANDOM(32), 2));
DECLARE @SecretHash varbinary(32) =
    HASHBYTES('SHA2_256', @SecretKey);
DECLARE @SecretHashBase64 varchar(100) =
    CAST(N'' AS xml).value(
        'xs:base64Binary(sql:variable("@SecretHash"))',
        'varchar(100)');
DECLARE @Scopes nvarchar(1000) =
    N'ekyc.deeplink.create,ekyc.merchant.create,ekyc.merchant.query,ekyc.merchant.update,payment.history,payment.investigation,payment.qr.create,payment.refund';
DECLARE @Now datetime2 = SYSDATETIME();
DECLARE @PartnerID int;

BEGIN TRANSACTION;

INSERT INTO dbo.Partners
(
    Code,
    Name,
    Status,
    CreatedAt,
    UpdatedAt
)
VALUES
(
    UPPER(@PartnerCode),
    @PartnerName,
    1, -- ACTIVE
    @Now,
    @Now
);

SET @PartnerID = CONVERT(int, SCOPE_IDENTITY());

INSERT INTO dbo.PartnerCredentials
(
    PartnerID,
    AccessKey,
    SecretKeyHash,
    SecretKey,
    Scopes,
    Status,
    ExpiresAt,
    LastUsedAt,
    RevokedAt,
    CreatedAt,
    UpdatedAt
)
VALUES
(
    @PartnerID,
    @AccessKey,
    @SecretHashBase64,
    @SecretKey,
    @Scopes,
    1, -- ACTIVE
    @ExpiresAt,
    NULL,
    NULL,
    @Now,
    @Now
);

COMMIT TRANSACTION;

SELECT
    @PartnerID AS PartnerID,
    UPPER(@PartnerCode) AS PartnerCode,
    @AccessKey AS AccessKey,
    @SecretKey AS SecretKey,
    @Scopes AS Scopes,
    @ExpiresAt AS ExpiresAt,
    N'SecretKey is shown once. Store it securely.' AS Important;
