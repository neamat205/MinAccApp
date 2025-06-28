

CREATE TYPE [dbo].[VoucherEntryType] AS TABLE(
	[AccountId] [int] NULL,
	[Debit] [decimal](18, 2) NULL,
	[Credit] [decimal](18, 2) NULL
)
GO

