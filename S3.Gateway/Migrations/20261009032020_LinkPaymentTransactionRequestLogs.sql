SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH(N'dbo.PaymentTransactions', N'NotificationPaymentID') IS NULL
        ALTER TABLE dbo.PaymentTransactions ADD NotificationPaymentID nvarchar(max) NULL;

    IF COL_LENGTH(N'dbo.PaymentTransactions', N'RequestCreateQRID') IS NULL
        ALTER TABLE dbo.PaymentTransactions ADD RequestCreateQRID nvarchar(max) NULL;

    EXEC(N'CREATE OR ALTER VIEW dbo.PaymentTransactionRequestLogs AS
        SELECT pt.ID AS PaymentTransactionID,
               CAST(NULL AS int) AS PaymentNotificationID,
               ''CREATE_QR'' AS RequestType,
               rl.ID AS RequestLogID,
               rl.RequestID,
               rl.CreatedAt AS RequestLogCreatedAt,
               rl.Path,
               rl.Method,
               rl.StatusCode,
               rl.Duration,
               rl.RequestHeader,
               rl.RequestBody,
               rl.ResponseBody,
               rl.ErrorMessage,
               rl.StackTrace
        FROM dbo.PaymentTransactions AS pt
        INNER JOIN dbo.RequestLogs AS rl
            ON rl.RequestID = pt.RequestCreateQRID
        UNION ALL
        SELECT pn.PaymentTransactionID,
               pn.ID AS PaymentNotificationID,
               ''NAPAS_NOTIFICATION'' AS RequestType,
               rl.ID AS RequestLogID,
               rl.RequestID,
               rl.CreatedAt AS RequestLogCreatedAt,
               rl.Path,
               rl.Method,
               rl.StatusCode,
               rl.Duration,
               rl.RequestHeader,
               rl.RequestBody,
               rl.ResponseBody,
               rl.ErrorMessage,
               rl.StackTrace
        FROM dbo.PaymentNotifications AS pn
        INNER JOIN dbo.RequestLogs AS rl
            ON rl.RequestID = pn.RequestID;');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
