CREATE PROCEDURE sp_GetPermissionsForUserAndModule
    @UserId NVARCHAR(450),
    @ModuleName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        ur.RoleId,
        MAX(CAST(CanView AS INT)) OVER() AS CanView,
        MAX(CAST(CanCreate AS INT)) OVER() AS CanCreate,
        MAX(CAST(CanEdit AS INT)) OVER() AS CanEdit,
        MAX(CAST(CanDelete AS INT)) OVER() AS CanDelete
    FROM RoleModulePermission rmp
    INNER JOIN AspNetUserRoles ur ON ur.RoleId = rmp.RoleId
    WHERE ur.UserId = @UserId
      AND rmp.ModuleName = @ModuleName;
END