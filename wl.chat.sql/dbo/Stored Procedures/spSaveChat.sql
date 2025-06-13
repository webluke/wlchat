CREATE PROCEDURE [dbo].[spSaveChat]
	@ThreadId INT, 
	@UserId NVARCHAR(100), 
    @Prompt NVARCHAR(MAX),
    @Response NVARCHAR(MAX), 
	@Model NVARCHAR(100), 
	@CreatedAt DATETIME
AS
BEGIN
	INSERT INTO ChatHistory (ThreadId, UserId, Prompt, Response, Model, CreatedAt)
    VALUES (@ThreadId, @UserId, @Prompt, @Response, @Model, @CreatedAt)
END
