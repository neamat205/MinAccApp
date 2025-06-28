CREATE PROCEDURE sp_AssignRoleToUser
    @UserId NVARCHAR(450),
    @RoleId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    -- Delete existing roles for the user (if any)
    DELETE FROM AspNetUserRoles WHERE UserId = @UserId;

    -- Insert new role assignment
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@UserId, @RoleId);
END