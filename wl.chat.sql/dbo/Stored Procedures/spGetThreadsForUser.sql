CREATE PROCEDURE [dbo].[spGetThreadsForUser]
	@UserId NVARCHAR(100)
AS
BEGIN
	SELECT * FROM ChatThread 
	WHERE UserId = @UserId 
	ORDER BY CreatedAt DESC;
END
