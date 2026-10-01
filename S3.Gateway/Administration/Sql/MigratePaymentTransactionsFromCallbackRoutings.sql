/*
    Migrate historical payment records from CallbackRoutings to PaymentTransactions.

    Safety:
      - Idempotent: a CallbackRouting is migrated only once.
      - Default is preview mode (ROLLBACK).
      - Set @Commit = 1 after checking the preview result.

    Scope:
      - Target = 1 (Napas)
      - ActionHistory starts with GEN QR
      - RequestPayload contains QR creation data
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Commit bit = 0;
DECLARE @Inserted int = 0;

BEGIN TRANSACTION;

;WITH CandidateRoutings AS
(
    SELECT
        cr.ID AS CallbackRoutingID,
        cr.PartnerID,
        cr.RequestID AS GatewayRequestID,
        cr.ClientRequestID,
        cr.RefID,
        cr.CreatedAt,
        cr.UpdatedAt,
        cr.RequestPayload,
        cr.ActionHistory,
        JSON_VALUE(cr.RequestPayload, '$.QRInfo.AccountNumber') AS AccountNumber,
        JSON_VALUE(cr.RequestPayload, '$.QRInfo.BankID') AS BankID,
        JSON_VALUE(cr.RequestPayload, '$.QRInfo.TransactionCurrency') AS Currency,
        JSON_VALUE(cr.RequestPayload, '$.Amount') AS RequestedAmount,
        JSON_VALUE(cr.RequestPayload, '$.BillNumber') AS BillNumber,
        JSON_VALUE(cr.RequestPayload, '$.Description') AS Description,
        CASE
            WHEN JSON_VALUE(cr.RequestPayload, '$.BillNumber') = 'NAPGSQR'
                THEN 'STATIC'
            ELSE 'DYNAMIC'
        END AS QRType
    FROM dbo.CallbackRoutings cr
    WHERE cr.Target = 1
      AND cr.ActionHistory LIKE 'GEN QR%'
      AND ISJSON(cr.RequestPayload) = 1
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.PaymentTransactions pt
          WHERE pt.CallbackRoutingID = cr.ID
      )
),
LatestNotifications AS
(
    SELECT
        c.CallbackRoutingID,
        JSON_VALUE(log.RequestPayload, '$.payload.id') AS ProviderTransactionID,
        JSON_VALUE(log.RequestPayload, '$.payload.refId') AS ProviderReferenceID,
        JSON_VALUE(log.RequestPayload, '$.payload.status') AS ProviderStatus,
        JSON_VALUE(log.RequestPayload, '$.payload.amount') AS NotificationAmount,
        ROW_NUMBER() OVER
        (
            PARTITION BY c.CallbackRoutingID
            ORDER BY log.CreatedAt DESC, log.ID DESC
        ) AS RowNumber
    FROM CandidateRoutings c
    INNER JOIN dbo.CallbackRoutingLogs log
        ON log.Action LIKE 'FORWARD%'
       AND ISJSON(log.RequestPayload) = 1
       AND JSON_VALUE(log.RequestPayload, '$.payload.id') = c.RefID
),
DataToInsert AS
(
    SELECT
        c.CallbackRoutingID,
        c.PartnerID,
        c.GatewayRequestID,
        c.ClientRequestID,
        c.RefID,
        c.AccountNumber,
        c.BankID,
        c.Currency,
        c.RequestedAmount,
        c.BillNumber,
        c.Description,
        c.QRType,
        c.RequestPayload,
        c.CreatedAt,
        c.UpdatedAt,
        n.ProviderTransactionID,
        n.ProviderReferenceID,
        n.ProviderStatus,
        n.NotificationAmount
    FROM CandidateRoutings c
    LEFT JOIN LatestNotifications n
        ON n.CallbackRoutingID = c.CallbackRoutingID
       AND n.RowNumber = 1
)
INSERT INTO dbo.PaymentTransactions
(
    PartnerID,
    Provider,
    PaymentMethod,
    ClientRequestID,
    GatewayRequestID,
    ProviderTransactionID,
    ProviderReferenceID,
    CallbackRoutingID,
    AccountNumber,
    BankID,
    Amount,
    Currency,
    BillNumber,
    Description,
    QRType,
    Status,
    ProviderStatus,
    ProviderCode,
    ProviderMessage,
    PaidAt,
    LastNotificationAt,
    RawCreateRequest,
    CreatedAt,
    UpdatedAt
)
SELECT
    d.PartnerID,
    'NAPAS',
    'QR',
    ISNULL(d.ClientRequestID, ''),
    ISNULL(d.GatewayRequestID, ''),
    ISNULL(NULLIF(d.ProviderTransactionID, ''), d.RefID),
    ISNULL(NULLIF(d.ProviderReferenceID, ''), d.RefID),
    d.CallbackRoutingID,
    ISNULL(d.AccountNumber, ''),
    ISNULL(d.BankID, ''),
    ISNULL
    (
        TRY_CONVERT(decimal(18, 2), NULLIF(d.NotificationAmount, '')),
        ISNULL(TRY_CONVERT(decimal(18, 2), NULLIF(d.RequestedAmount, '')), 0)
    ),
    ISNULL(d.Currency, ''),
    ISNULL(d.BillNumber, ''),
    ISNULL(d.Description, ''),
    d.QRType,
    CASE
        WHEN UPPER(ISNULL(d.ProviderStatus, '')) IN ('00', 'SUCCESS', 'PAID')
            THEN 3 -- SUCCESS
        ELSE 1 -- PENDING
    END,
    ISNULL(d.ProviderStatus, ''),
    '',
    '',
    CASE
        WHEN UPPER(ISNULL(d.ProviderStatus, '')) IN ('00', 'SUCCESS', 'PAID')
            THEN d.UpdatedAt
        ELSE NULL
    END,
    CASE
        WHEN d.ProviderStatus IS NOT NULL THEN d.UpdatedAt
        ELSE NULL
    END,
    ISNULL(d.RequestPayload, ''),
    d.CreatedAt,
    d.UpdatedAt
FROM DataToInsert d;

SET @Inserted = @@ROWCOUNT;

SELECT
    @Inserted AS InsertedRows,
    CASE WHEN @Commit = 1 THEN 'COMMIT' ELSE 'ROLLBACK (preview only)' END AS ExecutionMode;

IF @Commit = 1
    COMMIT TRANSACTION;
ELSE
    ROLLBACK TRANSACTION;
