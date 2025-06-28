CREATE PROCEDURE sp_GetAllUsersWithRoles
AS
BEGIN
    SELECT 
        u.Id AS UserId,
        u.UserName,
        u.Email,
        r.Name AS RoleName
    FROM AspNetUsers u
    LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
END