CREATE PROCEDURE [dbo].[spCreateThread]
    @UserId NVARCHAR(100),
    @Title NVARCHAR(255),
    @CreatedAt DATETIME
AS
BEGIN
    SET NOCOUNT ON;
	INSERT INTO ChatThread (UserId, Title, CreatedAt)
    VALUES (@UserId, @Title, @CreatedAt);
    SELECT CAST(SCOPE_IDENTITY() as int);
END
