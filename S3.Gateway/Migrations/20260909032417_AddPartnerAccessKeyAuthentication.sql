/*
    Idempotent deployment script for AddPartnerAccessKeyAuthentication.
    Safe to rerun after a partially applied deployment.
    Run with SQL Server Management Studio or sqlcmd.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH(N'dbo.RequestLogs', N'RequestHeader') IS NULL
        EXEC(N'ALTER TABLE [dbo].[RequestLogs] ADD [RequestHeader] nvarchar(max) NULL;');

    IF COL_LENGTH(N'dbo.CallbackRoutings', N'ClientRequestID') IS NULL
        EXEC(N'ALTER TABLE [dbo].[CallbackRoutings] ADD [ClientRequestID] nvarchar(100) NULL;');

    IF COL_LENGTH(N'dbo.CallbackRoutings', N'PartnerCredentialID') IS NULL
        EXEC(N'ALTER TABLE [dbo].[CallbackRoutings] ADD [PartnerCredentialID] int NULL;');

    IF COL_LENGTH(N'dbo.CallbackRoutings', N'PartnerID') IS NULL
        EXEC(N'ALTER TABLE [dbo].[CallbackRoutings] ADD [PartnerID] int NULL;');

    IF COL_LENGTH(N'dbo.CallbackRoutingLogs', N'RefID') IS NULL
        EXEC(N'ALTER TABLE [dbo].[CallbackRoutingLogs] ADD [RefID] nvarchar(max) NULL;');

    IF COL_LENGTH(N'dbo.ApiLogs', N'RequestHeader') IS NULL
        EXEC(N'ALTER TABLE [dbo].[ApiLogs] ADD [RequestHeader] nvarchar(max) NULL;');

    IF OBJECT_ID(N'dbo.Partners', N'U') IS NULL
    BEGIN
        EXEC(N'
            CREATE TABLE [dbo].[Partners]
            (
                [ID] int NOT NULL IDENTITY,
                [Code] nvarchar(50) NOT NULL,
                [Name] nvarchar(200) NOT NULL,
                [Status] int NOT NULL,
                [CreatedAt] datetime2 NOT NULL,
                [UpdatedAt] datetime2 NOT NULL,
                CONSTRAINT [PK_Partners] PRIMARY KEY ([ID])
            );');
    END;

    IF OBJECT_ID(N'dbo.PartnerCredentials', N'U') IS NULL
    BEGIN
        EXEC(N'
            CREATE TABLE [dbo].[PartnerCredentials]
            (
                [ID] int NOT NULL IDENTITY,
                [PartnerID] int NOT NULL,
                [AccessKey] nvarchar(100) NOT NULL,
                [SecretKeyHash] nvarchar(100) NOT NULL,
                [Scopes] nvarchar(1000) NULL,
                [Status] int NOT NULL,
                [ExpiresAt] datetime2 NULL,
                [LastUsedAt] datetime2 NULL,
                [RevokedAt] datetime2 NULL,
                [CreatedAt] datetime2 NOT NULL,
                [UpdatedAt] datetime2 NOT NULL,
                CONSTRAINT [PK_PartnerCredentials] PRIMARY KEY ([ID])
            );');
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_CallbackRoutings_PartnerCredentialID'
          AND object_id = OBJECT_ID(N'dbo.CallbackRoutings')
    )
    BEGIN
        EXEC(N'
            CREATE INDEX [IX_CallbackRoutings_PartnerCredentialID]
            ON [dbo].[CallbackRoutings] ([PartnerCredentialID]);');
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_CallbackRoutings_PartnerID_Target_ClientRequestID'
          AND object_id = OBJECT_ID(N'dbo.CallbackRoutings')
    )
    BEGIN
        EXEC(N'
            CREATE UNIQUE INDEX [IX_CallbackRoutings_PartnerID_Target_ClientRequestID]
            ON [dbo].[CallbackRoutings] ([PartnerID], [Target], [ClientRequestID])
            WHERE [PartnerID] IS NOT NULL AND [ClientRequestID] IS NOT NULL;');
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_PartnerCredentials_AccessKey'
          AND object_id = OBJECT_ID(N'dbo.PartnerCredentials')
    )
    BEGIN
        EXEC(N'
            CREATE UNIQUE INDEX [IX_PartnerCredentials_AccessKey]
            ON [dbo].[PartnerCredentials] ([AccessKey]);');
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_PartnerCredentials_PartnerID'
          AND object_id = OBJECT_ID(N'dbo.PartnerCredentials')
    )
    BEGIN
        EXEC(N'
            CREATE INDEX [IX_PartnerCredentials_PartnerID]
            ON [dbo].[PartnerCredentials] ([PartnerID]);');
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_Partners_Code'
          AND object_id = OBJECT_ID(N'dbo.Partners')
    )
    BEGIN
        EXEC(N'
            CREATE UNIQUE INDEX [IX_Partners_Code]
            ON [dbo].[Partners] ([Code]);');
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = N'FK_PartnerCredentials_Partners_PartnerID'
          AND parent_object_id = OBJECT_ID(N'dbo.PartnerCredentials')
    )
    BEGIN
        EXEC(N'
            ALTER TABLE [dbo].[PartnerCredentials]
            ADD CONSTRAINT [FK_PartnerCredentials_Partners_PartnerID]
            FOREIGN KEY ([PartnerID]) REFERENCES [dbo].[Partners] ([ID])
            ON DELETE NO ACTION;');
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = N'FK_CallbackRoutings_PartnerCredentials_PartnerCredentialID'
          AND parent_object_id = OBJECT_ID(N'dbo.CallbackRoutings')
    )
    BEGIN
        EXEC(N'
            ALTER TABLE [dbo].[CallbackRoutings]
            ADD CONSTRAINT [FK_CallbackRoutings_PartnerCredentials_PartnerCredentialID]
            FOREIGN KEY ([PartnerCredentialID]) REFERENCES [dbo].[PartnerCredentials] ([ID])
            ON DELETE NO ACTION;');
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = N'FK_CallbackRoutings_Partners_PartnerID'
          AND parent_object_id = OBJECT_ID(N'dbo.CallbackRoutings')
    )
    BEGIN
        EXEC(N'
            ALTER TABLE [dbo].[CallbackRoutings]
            ADD CONSTRAINT [FK_CallbackRoutings_Partners_PartnerID]
            FOREIGN KEY ([PartnerID]) REFERENCES [dbo].[Partners] ([ID])
            ON DELETE NO ACTION;');
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM [dbo].[__EFMigrationsHistory]
        WHERE [MigrationId] = N'20260909032417_AddPartnerAccessKeyAuthentication'
    )
    BEGIN
        INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20260909032417_AddPartnerAccessKeyAuthentication', N'10.0.6');
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
