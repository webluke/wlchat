CREATE PROCEDURE [dbo].[spDeleteThread]
	@ThreadId INT
AS
BEGIN
	DELETE FROM ChatHistory WHERE ThreadId = @ThreadId;
	DELETE FROM ChatThread WHERE Id = @ThreadId;
END

