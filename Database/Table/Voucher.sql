CREATE TABLE [dbo].[Voucher] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [VoucherType] NVARCHAR (20) NOT NULL,
    [Date]        DATETIME      NULL,
    [ReferenceNo] NVARCHAR (50) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);