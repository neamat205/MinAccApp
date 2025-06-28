CREATE PROCEDURE sp_ManageChartOfAccounts
    @Action NVARCHAR(10),  -- 'CREATE', 'UPDATE', 'DELETE', 'READ'
    @Id INT = NULL,
    @Name NVARCHAR(100) = NULL,
    @ParentId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'CREATE'
    BEGIN
        INSERT INTO ChartOfAccounts (Name, ParentId)
        VALUES (@Name, @ParentId)
    END
    ELSE IF @Action = 'UPDATE'
    BEGIN
        UPDATE ChartOfAccounts
        SET Name = @Name, ParentId = @ParentId
        WHERE Id = @Id
    END
    ELSE IF @Action = 'DELETE'
    BEGIN
        DELETE FROM ChartOfAccounts WHERE Id = @Id
    END
    ELSE IF @Action = 'READ'
    BEGIN
        SELECT * FROM ChartOfAccounts
    END
END