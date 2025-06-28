CREATE PROCEDURE sp_GetAllRoles
AS
BEGIN
    SELECT Id AS RoleId, Name AS RoleName
    FROM AspNetRoles
END