CREATE PROCEDURE [dbo].[spGetMessagesForThread]
	@ThreadId INT
AS
BEGIN
	SELECT * FROM ChatHistory 
	WHERE ThreadId = @ThreadId 
	ORDER BY CreatedAt
END
