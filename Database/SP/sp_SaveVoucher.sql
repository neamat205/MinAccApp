CREATE PROCEDURE sp_SaveVoucher
    @VoucherType NVARCHAR(20),
    @Date DATETIME,
    @ReferenceNo NVARCHAR(50),
    @Entries VoucherEntryType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

     
        INSERT INTO Voucher (VoucherType, Date, ReferenceNo)
        VALUES (@VoucherType, @Date, @ReferenceNo);

        DECLARE @VoucherId INT = SCOPE_IDENTITY();

     
        INSERT INTO VoucherEntry (VoucherId, AccountId, Debit, Credit)
        SELECT @VoucherId, AccountId, Debit, Credit FROM @Entries;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END