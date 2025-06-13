CREATE TABLE [dbo].[ChatHistory]
(
	Id INT IDENTITY PRIMARY KEY,
    UserId NVARCHAR(100) NOT NULL,
    ThreadId INT NULL,
    Prompt NVARCHAR(MAX),
    Response NVARCHAR(MAX),
    Model NVARCHAR(100),
    CreatedAt DATETIME,
    CONSTRAINT FK_ChatHistory_ChatThread FOREIGN KEY (ThreadId) REFERENCES ChatThread(Id)
);
