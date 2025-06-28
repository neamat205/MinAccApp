CREATE PROCEDURE sp_DeleteUser
    @UserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    -- First delete from AspNetUserRoles (if exists)
    DELETE FROM AspNetUserRoles WHERE UserId = @UserId;

    -- Then delete the user itself
    DELETE FROM AspNetUsers WHERE Id = @UserId;
END