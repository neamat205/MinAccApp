CREATE PROCEDURE sp_GetPermissionByRole
    @RoleId NVARCHAR(450),
    @ModuleName NVARCHAR(100) = NULL  -- Optional
AS
BEGIN
    SELECT ModuleName, CanView, CanEdit, CanDelete, CanCreate
    FROM RoleModulePermission
    WHERE RoleId = @RoleId
      AND (@ModuleName IS NULL OR ModuleName = @ModuleName)
END