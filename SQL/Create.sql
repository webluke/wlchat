CREATE TABLE ChatHistory (
    Id INT IDENTITY PRIMARY KEY,
    UserId NVARCHAR(100) NOT NULL,
    Prompt NVARCHAR(MAX),
    Response NVARCHAR(MAX),
    Model NVARCHAR(100),
    CreatedAt DATETIME
);

CREATE TABLE ChatThread (
    Id INT IDENTITY PRIMARY KEY,
    UserId NVARCHAR(100) NOT NULL,
    Title NVARCHAR(200),
    CreatedAt DATETIME
);

ALTER TABLE ChatHistory ADD ThreadId INT NULL;
ALTER TABLE ChatHistory
    ADD CONSTRAINT FK_ChatHistory_ChatThread
    FOREIGN KEY (ThreadId) REFERENCES ChatThread(Id);
