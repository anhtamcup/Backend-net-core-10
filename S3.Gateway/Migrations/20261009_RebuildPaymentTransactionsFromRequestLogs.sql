-- Clear temp tables in a separate batch. SQL Server otherwise compiles the next
-- SELECT against an old #Notifications schema still present in this session.
DROP TABLE IF EXISTS #Notifications;
DROP TABLE IF EXISTS #PaymentRequestLogs;
DROP TABLE IF EXISTS #QrRequests;
DROP TABLE IF EXISTS #QrLogKeys;
DROP TABLE IF EXISTS #RoutingEvents;
DROP TABLE IF EXISTS #RoutingCandidates;
GO

-- Preview first. Change to 1 only after PaymentTransactions and PaymentNotifications are cleared.
-- Pause incoming payment requests while applying. Keep RequestLogs, CallbackRoutings,
-- and CallbackRoutingLogs unchanged while this runs.
DECLARE @Apply bit = 0;

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF @Apply = 1
BEGIN
    IF COL_LENGTH(N'dbo.PaymentTransactions', N'RequestCreateQRID') IS NULL
       OR COL_LENGTH(N'dbo.PaymentTransactions', N'NotificationPaymentID') IS NULL
        THROW 51000, 'Apply the request-log schema SQL before rebuilding transactions.', 1;

    DECLARE @ExistingTransactions bigint =
        (SELECT COUNT_BIG(*) FROM dbo.PaymentTransactions);
    DECLARE @ExistingNotifications bigint =
        (SELECT COUNT_BIG(*) FROM dbo.PaymentNotifications);

    IF @ExistingTransactions > 0 OR @ExistingNotifications > 0
    BEGIN
        SELECT @ExistingTransactions AS ExistingPaymentTransactions,
               @ExistingNotifications AS ExistingPaymentNotifications;
        THROW 51001, 'PaymentTransactions and PaymentNotifications must both be empty.', 1;
    END;
END;

-- Read RequestLogs once for both notification payloads and Gen QR log keys.
SELECT rl.ID, rl.RequestID, rl.CreatedAt, rl.Path, rl.Method, rl.StatusCode,
       CASE WHEN rl.Path = N'/payment/napas/apg/notification'
            THEN rl.RequestBody ELSE NULL END AS RequestBody,
       CASE WHEN rl.Path = N'/payment/napas/apg/notification'
            THEN rl.ResponseBody ELSE NULL END AS ResponseBody
INTO #PaymentRequestLogs
FROM dbo.RequestLogs AS rl
WHERE rl.Path IN (N'/payment/napas/apg/notification', N'/payment/pmnp/qr');
RAISERROR(N'RequestLogs staged', 10, 1) WITH NOWAIT;

SELECT rl.ID AS RequestLogID,
       rl.RequestID,
       rl.CreatedAt AS NotificationAt,
       rl.RequestBody,
       rl.ResponseBody,
       NULLIF(COALESCE(JSON_VALUE(payload.JsonText, '$.id'),
                       JSON_VALUE(payload.JsonText, '$.Id')), N'') AS NapasID,
       COALESCE(JSON_VALUE(payload.JsonText, '$.refId'),
                JSON_VALUE(payload.JsonText, '$.RefId'), N'') AS NapasRefID,
       COALESCE(JSON_VALUE(payload.JsonText, '$.status'),
                JSON_VALUE(payload.JsonText, '$.Status'), N'') AS ProviderStatus,
       TRY_CONVERT(decimal(18, 2), REPLACE(COALESCE(
           JSON_VALUE(payload.JsonText, '$.amount'),
           JSON_VALUE(payload.JsonText, '$.Amount')), N',', N'')) AS Amount,
       COALESCE(JSON_VALUE(payload.JsonText, '$.beneficiaryBank'),
                JSON_VALUE(payload.JsonText, '$.BeneficiaryBank'), N'') AS BankID,
       COALESCE(JSON_VALUE(payload.JsonText, '$.realMerchantAccount'),
                JSON_VALUE(payload.JsonText, '$.RealMerchantAccount'), N'') AS RealMerchantAccount,
       COALESCE(JSON_VALUE(payload.JsonText, '$.customerName'),
                JSON_VALUE(payload.JsonText, '$.CustomerName'), N'') AS CustomerName,
       COALESCE(JSON_VALUE(response.JsonText, '$.code'),
                JSON_VALUE(response.JsonText, '$.Code'), N'') AS ResponseCode,
       COALESCE(JSON_VALUE(response.JsonText, '$.message'),
                JSON_VALUE(response.JsonText, '$.Message'), N'') AS ResponseMessage
INTO #Notifications
FROM #PaymentRequestLogs AS rl
CROSS APPLY (VALUES (CASE WHEN ISJSON(rl.RequestBody) = 1
                         THEN rl.RequestBody ELSE N'{}' END)) AS body(JsonText)
CROSS APPLY (VALUES (COALESCE(JSON_QUERY(body.JsonText, '$.payload'),
                             JSON_QUERY(body.JsonText, '$.Payload'), N'{}'))) AS payload(JsonText)
CROSS APPLY (VALUES (CASE WHEN ISJSON(rl.ResponseBody) = 1
                         THEN rl.ResponseBody ELSE N'{}' END)) AS response(JsonText)
WHERE rl.Path = N'/payment/napas/apg/notification'
  AND rl.Method = N'POST'
  AND rl.StatusCode = 200;

-- Read CallbackRoutingLogs once. The source table has no RequestID index, so
-- querying it separately for every QR request and notification is very slow.
SELECT log.ID, CONVERT(nvarchar(450), log.RequestID) AS RequestID,
       CONVERT(nvarchar(32), log.Action) AS Action,
       log.IsSuccess, log.CreatedAt
INTO #RoutingEvents
FROM dbo.CallbackRoutingLogs AS log
WHERE log.Action IN (N'GEN QR', N'FORWARD COMPLETED', N'FORWARD ERROR')
  AND log.RequestID IS NOT NULL
  AND LEN(log.RequestID) <= 450;

CREATE INDEX IX_RoutingEvents_RequestID ON #RoutingEvents (RequestID)
    INCLUDE (ID, Action, IsSuccess, CreatedAt);
RAISERROR(N'CallbackRoutingLogs staged', 10, 1) WITH NOWAIT;

SELECT routing.ID AS RoutingID,
       routing.RefID,
       routing.RequestID AS CreateRequestID,
       routing.ClientRequestID,
       routing.PartnerID,
       routing.CreatedAt,
       routing.RequestPayload,
       gen.IsSuccess AS GenQrSuccess,
       TRY_CONVERT(decimal(18, 2), COALESCE(
           JSON_VALUE(body.JsonText, '$.Amount'),
           JSON_VALUE(body.JsonText, '$.amount'))) AS Amount,
       COALESCE(JSON_VALUE(qr.JsonText, '$.AccountNumber'),
                JSON_VALUE(qr.JsonText, '$.accountNumber'), N'') AS AccountNumber,
       COALESCE(JSON_VALUE(qr.JsonText, '$.BankID'),
                JSON_VALUE(qr.JsonText, '$.bankID'), N'') AS BankID,
       COALESCE(JSON_VALUE(qr.JsonText, '$.TransactionCurrency'),
                JSON_VALUE(qr.JsonText, '$.transactionCurrency'), N'704') AS Currency,
       COALESCE(JSON_VALUE(body.JsonText, '$.BillNumber'),
                JSON_VALUE(body.JsonText, '$.billNumber'), N'') AS BillNumber,
       COALESCE(JSON_VALUE(body.JsonText, '$.Description'),
                JSON_VALUE(body.JsonText, '$.description'), N'') AS Description
INTO #QrRequests
FROM dbo.CallbackRoutings AS routing
CROSS APPLY (VALUES (CASE WHEN ISJSON(routing.RequestPayload) = 1
                         THEN routing.RequestPayload ELSE N'{}' END)) AS body(JsonText)
CROSS APPLY (VALUES (COALESCE(JSON_QUERY(body.JsonText, '$.QRInfo'),
                             JSON_QUERY(body.JsonText, '$.qrInfo'), N'{}'))) AS qr(JsonText)
OUTER APPLY (
    SELECT TOP (1) log.IsSuccess
    FROM #RoutingEvents AS log
    WHERE log.RequestID = CONVERT(nvarchar(450), routing.RequestID)
      AND log.Action = N'GEN QR'
    ORDER BY log.ID DESC
) AS gen
WHERE routing.Target = 1
  AND gen.IsSuccess IS NOT NULL;
RAISERROR(N'Gen QR routings staged', 10, 1) WITH NOWAIT;

SELECT DISTINCT CONVERT(nvarchar(450), rl.RequestID) AS RequestID
INTO #QrLogKeys
FROM #PaymentRequestLogs AS rl
WHERE rl.Path = N'/payment/pmnp/qr'
  AND rl.RequestID IS NOT NULL
  AND LEN(rl.RequestID) <= 450;

CREATE UNIQUE CLUSTERED INDEX IX_QrLogKeys_RequestID ON #QrLogKeys (RequestID);
RAISERROR(N'QR RequestLog keys staged', 10, 1) WITH NOWAIT;

-- Prefer an exact Payload.Id match. Only fall back to the encoded routing
-- suffix when the full ID differs from the QR ID saved during generation.
SELECT n.RequestLogID, qr.RoutingID,
       CASE WHEN qr.RefID COLLATE Latin1_General_100_BIN2 = n.NapasID
            THEN 0 ELSE 1 END AS MatchPriority,
       ROW_NUMBER() OVER (
           PARTITION BY n.RequestLogID
           ORDER BY CASE WHEN qr.RefID COLLATE Latin1_General_100_BIN2 = n.NapasID
                         THEN 0 ELSE 1 END,
                    qr.CreatedAt, qr.RoutingID
       ) AS ChoiceRank,
       COUNT(*) OVER (
           PARTITION BY n.RequestLogID,
                        CASE WHEN qr.RefID COLLATE Latin1_General_100_BIN2 = n.NapasID
                             THEN 0 ELSE 1 END
       ) AS SamePriorityCount
INTO #RoutingCandidates
FROM #Notifications AS n
JOIN #QrRequests AS qr
  ON qr.CreatedAt <= n.NotificationAt
 AND (qr.RefID COLLATE Latin1_General_100_BIN2 = n.NapasID OR
      (LEFT(RIGHT(n.NapasID, 7), 1) COLLATE Latin1_General_100_BIN2 = N'F'
       AND RIGHT(qr.RefID, 7) COLLATE Latin1_General_100_BIN2 = RIGHT(n.NapasID, 7)));

CREATE INDEX IX_RoutingCandidates_Log ON #RoutingCandidates (RequestLogID, ChoiceRank)
    INCLUDE (RoutingID, MatchPriority, SamePriorityCount);
RAISERROR(N'Notification routing candidates staged', 10, 1) WITH NOWAIT;

SELECT N'Gen QR requests' AS Item, COUNT_BIG(*) AS [RowCount] FROM #QrRequests
UNION ALL
SELECT N'Successful notification requests', COUNT_BIG(*) FROM #Notifications
UNION ALL
SELECT N'Notification requests with non-200 status', COUNT_BIG(*) FROM #PaymentRequestLogs
WHERE Path = N'/payment/napas/apg/notification' AND Method = N'POST' AND StatusCode <> 200
UNION ALL
SELECT N'Existing PaymentTransactions', COUNT_BIG(*) FROM dbo.PaymentTransactions
UNION ALL
SELECT N'Existing PaymentNotifications', COUNT_BIG(*) FROM dbo.PaymentNotifications
UNION ALL
SELECT N'Invalid notification payloads', COUNT_BIG(*) FROM #Notifications
WHERE NapasID IS NULL OR LEN(NapasID) < 7 OR LEN(NapasID) > 450
   OR RequestID IS NULL OR LEN(RequestID) > 450 OR Amount IS NULL
UNION ALL
SELECT N'Invalid Gen QR payloads', COUNT_BIG(*) FROM #QrRequests
WHERE NULLIF(RefID, N'') IS NULL OR NULLIF(CreateRequestID, N'') IS NULL
   OR LEN(RefID) > 450 OR LEN(ClientRequestID) > 450
   OR Amount IS NULL OR NULLIF(AccountNumber, N'') IS NULL
UNION ALL
SELECT N'Gen QR requests without matching RequestLog', COUNT_BIG(*)
FROM #QrRequests AS qr
WHERE NOT EXISTS (SELECT 1 FROM #QrLogKeys AS rl
                  WHERE rl.RequestID = qr.CreateRequestID)
UNION ALL
SELECT N'Notifications without Gen QR', COUNT_BIG(*)
FROM #Notifications AS n
WHERE NOT EXISTS (SELECT 1 FROM #RoutingCandidates AS c
                  WHERE c.RequestLogID = n.RequestLogID);

-- For a reused static ID, notifications use the earliest Gen QR request.
SELECT n.RequestLogID, n.NapasID, COUNT(*) AS MatchingQrRequests,
       MIN(qr.CreatedAt) AS FirstQrRequestAt
FROM #Notifications AS n
JOIN #RoutingCandidates AS c ON c.RequestLogID = n.RequestLogID
JOIN #QrRequests AS qr ON qr.RoutingID = c.RoutingID
WHERE RIGHT(n.NapasID, 7) COLLATE Latin1_General_100_BIN2 = N'NAPGSQR'
  AND c.MatchPriority = 0
GROUP BY n.RequestLogID, n.NapasID
HAVING COUNT(*) > 1;

-- Inspect the requests that will create standalone PaymentTransactions.
SELECT TOP (100) n.RequestLogID, n.RequestID, n.NapasID, n.NotificationAt
FROM #Notifications AS n
WHERE NOT EXISTS (SELECT 1 FROM #RoutingCandidates AS c
                  WHERE c.RequestLogID = n.RequestLogID)
ORDER BY n.NotificationAt, n.RequestLogID;

-- A non-static notification is ambiguous only when its best match still has
-- more than one candidate. Show the IDs before refusing to write.
SELECT TOP (100) n.RequestLogID, n.NapasID, c.RoutingID, qr.RefID,
       qr.CreateRequestID, c.MatchPriority, c.SamePriorityCount
FROM #RoutingCandidates AS c
JOIN #Notifications AS n ON n.RequestLogID = c.RequestLogID
JOIN #QrRequests AS qr ON qr.RoutingID = c.RoutingID
WHERE RIGHT(n.NapasID, 7) COLLATE Latin1_General_100_BIN2 <> N'NAPGSQR'
  AND EXISTS (SELECT 1 FROM #RoutingCandidates AS best
              WHERE best.RequestLogID = c.RequestLogID
                AND best.ChoiceRank = 1
                AND best.SamePriorityCount > 1)
ORDER BY n.RequestLogID, c.MatchPriority, c.RoutingID;

-- These QR transactions can be rebuilt from CallbackRoutings, but their
-- RequestCreateQRID will not resolve to a RequestLogs row until that log exists.
SELECT TOP (100) qr.RoutingID, qr.CreateRequestID, qr.ClientRequestID,
       qr.RefID, qr.CreatedAt
FROM #QrRequests AS qr
WHERE NOT EXISTS (SELECT 1 FROM #QrLogKeys AS rl
                  WHERE rl.RequestID = qr.CreateRequestID)
ORDER BY qr.CreatedAt, qr.RoutingID;

IF @Apply = 0
    RETURN;

IF EXISTS (SELECT 1 FROM #Notifications
           WHERE NapasID IS NULL OR LEN(NapasID) < 7 OR LEN(NapasID) > 450
              OR RequestID IS NULL OR LEN(RequestID) > 450 OR Amount IS NULL)
   OR EXISTS (SELECT 1 FROM #QrRequests
              WHERE NULLIF(RefID, N'') IS NULL OR NULLIF(CreateRequestID, N'') IS NULL
                 OR LEN(RefID) > 450 OR LEN(ClientRequestID) > 450
                 OR Amount IS NULL OR NULLIF(AccountNumber, N'') IS NULL)
    THROW 51002, 'Some request payloads cannot be parsed; review the preview output.', 1;

IF EXISTS (SELECT 1 FROM #Notifications GROUP BY RequestID HAVING COUNT(*) > 1)
    THROW 51003, 'More than one notification log has the same RequestID.', 1;

IF EXISTS (SELECT 1 FROM #QrRequests GROUP BY CreateRequestID HAVING COUNT(*) > 1)
    THROW 51006, 'More than one Gen QR routing has the same RequestID.', 1;

IF EXISTS (
    SELECT 1
    FROM #RoutingCandidates AS c
    JOIN #Notifications AS n ON n.RequestLogID = c.RequestLogID
    WHERE c.ChoiceRank = 1
      AND c.SamePriorityCount > 1
      AND RIGHT(n.NapasID, 7) COLLATE Latin1_General_100_BIN2 <> N'NAPGSQR'
)
    THROW 51004, 'A non-static notification has multiple equally ranked Gen QR requests; inspect the candidate rows.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM dbo.PaymentTransactions WITH (TABLOCKX, HOLDLOCK))
       OR EXISTS (SELECT 1 FROM dbo.PaymentNotifications WITH (TABLOCKX, HOLDLOCK))
        THROW 51001, 'PaymentTransactions and PaymentNotifications must both be empty.', 1;

    INSERT INTO dbo.PaymentTransactions
        (PartnerID, Provider, PaymentMethod, ClientRequestID, GatewayRequestID,
         RequestCreateQRID, ProviderTransactionID, ProviderReferenceID,
         CallbackRoutingID, AccountNumber, BankID, Amount, Currency,
         BillNumber, Description, QRType, CustomerName, Status,
         PartnerCallbackStatus, RawCreateRequest, CreatedAt, UpdatedAt)
    SELECT qr.PartnerID, N'NAPAS', N'QR', COALESCE(qr.ClientRequestID, N''),
           qr.CreateRequestID, qr.CreateRequestID, qr.RefID, qr.RefID,
           qr.RoutingID, qr.AccountNumber, qr.BankID, qr.Amount, qr.Currency,
           qr.BillNumber, qr.Description,
           CASE WHEN RIGHT(qr.RefID, 7) COLLATE Latin1_General_100_BIN2 = N'NAPGSQR'
                THEN N'STATIC' ELSE N'DYNAMIC' END,
           N'', CASE WHEN qr.GenQrSuccess = 1 THEN 1 ELSE 4 END,
           1, qr.RequestPayload, qr.CreatedAt, qr.CreatedAt
    FROM #QrRequests AS qr;
    RAISERROR(N'Gen QR PaymentTransactions inserted', 10, 1) WITH NOWAIT;

    DECLARE @LogID int, @RequestID nvarchar(450), @At datetime2,
            @NapasID nvarchar(450), @RefID nvarchar(max), @Status nvarchar(max),
            @Amount decimal(18, 2), @BankID nvarchar(max),
            @RealMerchantAccount nvarchar(max), @CustomerName nvarchar(max),
            @Payload nvarchar(max), @ResponseCode nvarchar(max), @ResponseMessage nvarchar(max);
    DECLARE @RoutingID int, @OriginalID int, @TransactionID int,
            @CreateRequestID nvarchar(max), @PartnerID int, @ClientRequestID nvarchar(max),
            @AccountNumber nvarchar(max), @QrBankID nvarchar(max), @Currency nvarchar(max),
            @BillNumber nvarchar(max), @Description nvarchar(max), @RawCreateRequest nvarchar(max),
            @CallbackSuccess bit, @CallbackAt datetime2, @PaymentStatus int,
            @CallbackStatus int, @ProcessingStatus int, @ErrorMessage nvarchar(max);
    DECLARE @ProcessedNotifications int = 0;

    DECLARE notifications CURSOR LOCAL FAST_FORWARD FOR
        SELECT RequestLogID, RequestID, NotificationAt, NapasID, NapasRefID,
               ProviderStatus, Amount, BankID, RealMerchantAccount,
               CustomerName, RequestBody,
               ResponseCode, ResponseMessage
        FROM #Notifications
        ORDER BY NotificationAt, RequestLogID;

    OPEN notifications;
    FETCH NEXT FROM notifications INTO
        @LogID, @RequestID, @At, @NapasID, @RefID, @Status, @Amount,
        @BankID, @RealMerchantAccount, @CustomerName, @Payload,
        @ResponseCode, @ResponseMessage;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT @RoutingID = NULL, @OriginalID = NULL, @TransactionID = NULL,
               @CreateRequestID = NULL, @PartnerID = NULL, @ClientRequestID = NULL,
               @AccountNumber = NULL, @QrBankID = NULL, @Currency = NULL,
               @BillNumber = NULL, @Description = NULL, @RawCreateRequest = NULL,
               @CallbackSuccess = NULL, @CallbackAt = NULL;

        SELECT TOP (1) @RoutingID = qr.RoutingID,
               @CreateRequestID = qr.CreateRequestID,
               @PartnerID = qr.PartnerID,
               @ClientRequestID = qr.ClientRequestID,
               @AccountNumber = qr.AccountNumber,
               @QrBankID = qr.BankID,
               @Currency = qr.Currency,
               @BillNumber = qr.BillNumber,
               @Description = qr.Description,
               @RawCreateRequest = qr.RequestPayload
        FROM #RoutingCandidates AS c
        JOIN #QrRequests AS qr ON qr.RoutingID = c.RoutingID
        WHERE c.RequestLogID = @LogID AND c.ChoiceRank = 1;

        IF @RoutingID IS NOT NULL
        BEGIN
            SELECT @OriginalID = pt.ID
            FROM dbo.PaymentTransactions AS pt
            WHERE pt.CallbackRoutingID = @RoutingID
              AND pt.GatewayRequestID = @CreateRequestID;

            SELECT TOP (1) @CallbackSuccess = log.IsSuccess,
                   @CallbackAt = log.CreatedAt
            FROM #RoutingEvents AS log
            WHERE log.RequestID = @RequestID
              AND log.Action IN (N'FORWARD COMPLETED', N'FORWARD ERROR')
            ORDER BY log.ID DESC;

            IF RIGHT(@NapasID, 7) COLLATE Latin1_General_100_BIN2 <> N'NAPGSQR'
                SET @TransactionID = @OriginalID;
            ELSE IF @OriginalID IS NOT NULL
                    AND EXISTS (SELECT 1 FROM dbo.PaymentTransactions
                                WHERE ID = @OriginalID AND NotificationPaymentID IS NULL)
                SET @TransactionID = @OriginalID;
        END;

        SET @PaymentStatus = CASE WHEN UPPER(@Status) IN
            (N'00', N'SUCCESS', N'PAID', N'ACSP') THEN 3 ELSE 2 END;
        SET @CallbackStatus = CASE WHEN @RoutingID IS NULL THEN 1
                                   WHEN @CallbackSuccess = 1 THEN 2
                                   WHEN @CallbackSuccess = 0 THEN 3 ELSE 1 END;
        SET @ProcessingStatus = CASE WHEN @RoutingID IS NULL THEN 2
                                     WHEN @CallbackSuccess = 1 THEN 2
                                     WHEN @CallbackSuccess = 0 THEN 4 ELSE 1 END;
        SET @ErrorMessage = CASE WHEN @ProcessingStatus = 4
                                 THEN N'Forward callback thất bại' ELSE N'' END;

        IF @TransactionID IS NULL
        BEGIN
            INSERT INTO dbo.PaymentTransactions
                (PartnerID, Provider, PaymentMethod, ClientRequestID, GatewayRequestID,
                 RequestCreateQRID, ProviderTransactionID, ProviderReferenceID,
                 CallbackRoutingID, AccountNumber, BankID, Amount, Currency,
                 BillNumber, Description, QRType, CustomerName, Status, ProviderStatus,
                 PaidAt, LastNotificationAt, PartnerCallbackStatus, RawCreateRequest,
                 CreatedAt, UpdatedAt)
            VALUES
                (@PartnerID, N'NAPAS', N'QR', COALESCE(@ClientRequestID, N''), @RequestID,
                 @CreateRequestID, @NapasID, @RefID, @RoutingID,
                 COALESCE(NULLIF(@AccountNumber, N''),
                          NULLIF(@RealMerchantAccount, N''),
                          LEFT(@NapasID, LEN(@NapasID) - 7)),
                 COALESCE(NULLIF(@BankID, N''), @QrBankID, N''), @Amount,
                 COALESCE(NULLIF(@Currency, N''), N'704'),
                 COALESCE(NULLIF(@BillNumber, N''),
                          CASE WHEN RIGHT(@NapasID, 7) COLLATE Latin1_General_100_BIN2 = N'NAPGSQR'
                               THEN N'NAPGSQR' ELSE N'' END),
                 COALESCE(@Description, N''),
                 CASE WHEN RIGHT(@NapasID, 7) COLLATE Latin1_General_100_BIN2 = N'NAPGSQR'
                      THEN N'STATIC' ELSE N'DYNAMIC' END,
                 COALESCE(@CustomerName, N''), @PaymentStatus, @Status,
                 CASE WHEN @PaymentStatus = 3 THEN @At ELSE NULL END,
                 @At, @CallbackStatus, COALESCE(@RawCreateRequest, N''), @At, @At);

            SET @TransactionID = CONVERT(int, SCOPE_IDENTITY());
        END;

        UPDATE pt
        SET Amount = @Amount,
            ProviderTransactionID = @NapasID,
            ProviderReferenceID = @RefID,
            ProviderStatus = @Status,
            CustomerName = COALESCE(NULLIF(@CustomerName, N''), pt.CustomerName),
            Status = @PaymentStatus,
            PaidAt = CASE WHEN @PaymentStatus = 3 THEN COALESCE(pt.PaidAt, @At) ELSE pt.PaidAt END,
            LastNotificationAt = @At,
            NotificationPaymentID = @RequestID,
            PartnerCallbackStatus = @CallbackStatus,
            PartnerCallbackCode = COALESCE(@ResponseCode, N''),
            PartnerCallbackMessage = COALESCE(@ResponseMessage, N''),
            PartnerCallbackAt = @CallbackAt,
            PartnerCallbackError = CASE WHEN @ProcessingStatus = 4
                THEN N'HTTP status hoặc response.code không hợp lệ' ELSE N'' END,
            UpdatedAt = @At
        FROM dbo.PaymentTransactions AS pt
        WHERE pt.ID = @TransactionID;

        INSERT INTO dbo.PaymentNotifications
            (PaymentTransactionID, RequestID, Provider, ProviderTransactionID,
             ProviderReferenceID, ProviderStatus, Payload, ProcessingStatus,
             IsDuplicate, ReceivedAt, ProcessedAt, ErrorMessage, CreatedAt, UpdatedAt)
        VALUES
            (@TransactionID, @RequestID, N'NAPAS', @NapasID,
             @RefID, @Status, @Payload, @ProcessingStatus,
             0, @At, CASE WHEN @ProcessingStatus = 1 THEN NULL ELSE @At END,
             @ErrorMessage, @At, @At);

        SET @ProcessedNotifications += 1;
        IF @ProcessedNotifications % 50 = 0
            RAISERROR(N'Notifications processed: %d', 10, 1,
                      @ProcessedNotifications) WITH NOWAIT;

        FETCH NEXT FROM notifications INTO
            @LogID, @RequestID, @At, @NapasID, @RefID, @Status, @Amount,
            @BankID, @RealMerchantAccount, @CustomerName, @Payload,
            @ResponseCode, @ResponseMessage;
    END;

    CLOSE notifications;
    DEALLOCATE notifications;

    IF (SELECT COUNT_BIG(*) FROM dbo.PaymentNotifications) <>
       (SELECT COUNT_BIG(*) FROM #Notifications)
        THROW 51005, 'Notification row count does not match the source logs.', 1;

    COMMIT TRANSACTION;
    RAISERROR(N'Rebuild committed', 10, 1) WITH NOWAIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT N'PaymentTransactions' AS TableName, COUNT_BIG(*) AS [RowCount]
FROM dbo.PaymentTransactions
UNION ALL
SELECT N'PaymentNotifications', COUNT_BIG(*) FROM dbo.PaymentNotifications;
GO
