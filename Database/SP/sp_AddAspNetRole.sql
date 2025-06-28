
CREATE PROCEDURE sp_AddAspNetRole
    @RoleName NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM AspNetRoles WHERE UPPER(Name) = UPPER(@RoleName)
    )
    BEGIN
        THROW 50000, 'Role already exists in AspNetRoles.', 1;
    END

    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), @RoleName, UPPER(@RoleName), NEWID());
END