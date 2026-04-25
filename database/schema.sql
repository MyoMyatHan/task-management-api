-- =============================================
-- Task Management Database Schema
-- Database: MSSQL
-- =============================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TaskManagementDB')
BEGIN
    CREATE DATABASE TaskManagementDB;
END
GO

USE TaskManagementDB;
GO

-- =============================================
-- Table: TaskHeaders
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskHeaders')
BEGIN
    CREATE TABLE TaskHeaders (
        TaskId          UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
        TaskCode        NVARCHAR(20)        NOT NULL,
        Title           NVARCHAR(200)       NOT NULL,
        Description     NVARCHAR(1000)      NULL,
        Priority        NVARCHAR(20)        NOT NULL DEFAULT 'Medium',
        Status          NVARCHAR(20)        NOT NULL DEFAULT 'Pending',
        DueDate         DATETIME2           NULL,
        AssignedTo      NVARCHAR(100)       NULL,
        CreatedBy       NVARCHAR(100)       NOT NULL,
        CreatedAt       DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt       DATETIME2           NULL,
        IsActive        BIT                 NOT NULL DEFAULT 1,

        CONSTRAINT PK_TaskHeaders PRIMARY KEY (TaskId),
        CONSTRAINT UQ_TaskHeaders_TaskCode UNIQUE (TaskCode),
        CONSTRAINT CK_TaskHeaders_Priority CHECK (Priority IN ('Low', 'Medium', 'High')),
        CONSTRAINT CK_TaskHeaders_Status   CHECK (Status   IN ('Pending', 'InProgress', 'Done'))
    );
END
GO

-- =============================================
-- Table: TaskDetails
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskDetails')
BEGIN
    CREATE TABLE TaskDetails (
        TaskDetailId    UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
        TaskId          UNIQUEIDENTIFIER    NOT NULL,
        [LineNo]        INT                 NOT NULL,
        ItemTitle       NVARCHAR(200)       NOT NULL,
        ItemDescription NVARCHAR(1000)      NULL,
        IsCompleted     BIT                 NOT NULL DEFAULT 0,
        Remark          NVARCHAR(500)       NULL,
        CreatedAt       DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
        IsActive        BIT                 NOT NULL DEFAULT 1,

        CONSTRAINT PK_TaskDetails PRIMARY KEY (TaskDetailId),
        CONSTRAINT FK_TaskDetails_TaskHeaders FOREIGN KEY (TaskId)
            REFERENCES TaskHeaders (TaskId)
    );

    CREATE INDEX IX_TaskDetails_TaskId_IsActive ON TaskDetails (TaskId, IsActive) INCLUDE ([LineNo]);
END
GO

-- =============================================
-- Table: FileAttachments
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FileAttachments')
BEGIN
    CREATE TABLE FileAttachments (
        FileId              UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
        TaskId              UNIQUEIDENTIFIER    NULL,
        OriginalFileName    NVARCHAR(260)       NOT NULL,
        StoredFileName      NVARCHAR(260)       NOT NULL,
        FilePath            NVARCHAR(500)       NOT NULL,
        ContentType         NVARCHAR(100)       NOT NULL,
        FileSize            BIGINT              NOT NULL,
        UploadedAt          DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
        IsActive            BIT                 NOT NULL DEFAULT 1,

        CONSTRAINT PK_FileAttachments PRIMARY KEY (FileId),
        CONSTRAINT FK_FileAttachments_TaskHeaders FOREIGN KEY (TaskId)
            REFERENCES TaskHeaders (TaskId)
    );

    CREATE INDEX IX_FileAttachments_TaskId_IsActive ON FileAttachments (TaskId, IsActive);
END
GO

-- =============================================
-- Table: SystemLogs
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SystemLogs')
BEGIN
    CREATE TABLE SystemLogs (
        LogId           UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
        Level           NVARCHAR(20)        NOT NULL,
        Action          NVARCHAR(200)       NOT NULL,
        Message         NVARCHAR(MAX)       NOT NULL,
        StackTrace      NVARCHAR(MAX)       NULL,
        CreatedAt       DATETIME2           NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT PK_SystemLogs PRIMARY KEY (LogId),
        CONSTRAINT CK_SystemLogs_Level CHECK (Level IN ('Info', 'Warning', 'Error'))
    );

    CREATE INDEX IX_SystemLogs_Level ON SystemLogs (Level);
    CREATE INDEX IX_SystemLogs_CreatedAt ON SystemLogs (CreatedAt);
END
GO

-- =============================================
-- Indexes: IsActive filter optimisation
-- =============================================
CREATE INDEX IX_TaskHeaders_IsActive
    ON TaskHeaders (IsActive)
    INCLUDE (TaskCode, CreatedAt);
GO
