CREATE TABLE [dbo].[VoucherEntry] (
    [Id]        INT             IDENTITY (1, 1) NOT NULL,
    [VoucherId] INT             NOT NULL,
    [AccountId] INT             NOT NULL,
    [Debit]     DECIMAL (18, 2) NULL,
    [Credit]    DECIMAL (18, 2) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_VoucherEntry_Voucher] FOREIGN KEY ([VoucherId]) REFERENCES [dbo].[Voucher] ([Id]),
    CONSTRAINT [FK_VoucherEntry_Account] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[ChartOfAccounts] ([Id])
);