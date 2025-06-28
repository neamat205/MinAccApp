CREATE PROCEDURE sp_SetPermissionForRole
    @RoleId NVARCHAR(450),
    @ModuleName NVARCHAR(100),
    @CanView BIT,
    @CanEdit BIT,
    @CanDelete BIT,
    @CanCreate BIT
AS
BEGIN
    IF EXISTS (
        SELECT 1 FROM RoleModulePermission
        WHERE RoleID = @RoleId AND ModuleName = @ModuleName
    )
    BEGIN
        UPDATE RoleModulePermission
        SET CanView = @CanView,
            CanEdit = @CanEdit,
            CanDelete = @CanDelete,
            CanCreate = @CanCreate
        WHERE RoleID = @RoleId AND ModuleName = @ModuleName
    END
    ELSE
    BEGIN
        INSERT INTO RoleModulePermission (
            RoleID, ModuleName, CanView, CanEdit, CanDelete, CanCreate
        )
        VALUES (
            @RoleId, @ModuleName, @CanView, @CanEdit, @CanDelete, @CanCreate
        )
    END
END