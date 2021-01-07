CREATE DATABASE MSSQLLocalBankDB;

GO 

USE MSSQLLocalBankDB;

CREATE TABLE [dbo].[Bank] (
    [Id]           INT            NULL,
    [today]        DATE           DEFAULT (CONVERT([date],getdate(),(112))) NOT NULL,
    [name]         NVARCHAR (100) DEFAULT (N'Банк') NOT NULL,
    [baseRate]     FLOAT (53)     DEFAULT ((10)) NOT NULL,
    [illegalChars] NVARCHAR (100) NULL,
    UNIQUE NONCLUSTERED ([Id] ASC),
    CHECK ([Id]=(1))
);

CREATE TABLE [dbo].[Clients] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [creditRating] INT DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[NaturalPersons] (
    [Id]       INT           NOT NULL,
    [name]     NVARCHAR (20) NOT NULL,
    [surname]  NVARCHAR (50) NULL,
    [birthday] DATE          DEFAULT (CONVERT([date],'01.01.1900')) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([Id]) REFERENCES [dbo].[Clients] ([Id])
);

CREATE TABLE [dbo].[VIPs] (
    [Id]        INT            NOT NULL,
    [name]      NVARCHAR (20)  NOT NULL,
    [surname]   NVARCHAR (50)  NULL,
    [birthday]  DATE           DEFAULT (CONVERT([date],'01.01.1900')) NOT NULL,
    [workPlace] NVARCHAR (100) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([Id]) REFERENCES [dbo].[Clients] ([Id])
);

CREATE TABLE [dbo].[Companies] (
    [Id]      INT            NOT NULL,
    [typeOrg] NVARCHAR (20)  NOT NULL,
    [Name]    NVARCHAR (100) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([Id]) REFERENCES [dbo].[Clients] ([Id])
);

CREATE TABLE [dbo].[Accounts] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [clientId]     INT           NOT NULL,
    [isOpen]       BIT           DEFAULT ((1)) NOT NULL,
    [isRefill]     BIT           DEFAULT ((1)) NOT NULL,
    [isWithdrawal] BIT           DEFAULT ((1)) NOT NULL,
    [openDate]     SMALLDATETIME DEFAULT (CONVERT([smalldatetime],'01.01.1900')) NOT NULL,
    [balance]      FLOAT (53)    DEFAULT ((0)) NOT NULL,
    [isCheking]    BIT           DEFAULT ((1)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([clientId]) REFERENCES [dbo].[Clients] ([Id])
);

CREATE TABLE [dbo].[Deposits] (
    [Id]                     INT           NOT NULL,
    [percent]                FLOAT (53)    NOT NULL,
    [capitalization]         BIT           NOT NULL,
    [endDate]                SMALLDATETIME NOT NULL,
    [previousCapitalization] SMALLDATETIME NOT NULL,
    [isActive]               BIT           NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([Id]) REFERENCES [dbo].[Accounts] ([Id])
);

CREATE TABLE [dbo].[Credits] (
    [Id]                     INT           NOT NULL,
    [percent]                FLOAT (53)    NOT NULL,
    [startDebt]              FLOAT (53)    NOT NULL,
    [curentDebt]             FLOAT (53)    NOT NULL,
    [monthlyPayment]         FLOAT (53)    NOT NULL,
    [endDate]                SMALLDATETIME NOT NULL,
    [previousCapitalization] SMALLDATETIME NOT NULL,
    [isActive]               BIT           DEFAULT ((1)) NOT NULL,
    [withoutNegativeBalance] BIT           DEFAULT ((1)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([Id]) REFERENCES [dbo].[Accounts] ([Id])
);

CREATE TABLE [dbo].[ActivityInfo] (
    [Id]      INT            IDENTITY (1, 1) NOT NULL,
    [date]    SMALLDATETIME  NOT NULL,
    [message] NVARCHAR (200) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[BalanceLog] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [AccountId]     INT           NOT NULL,
    [date]          SMALLDATETIME NOT NULL,
    [message]       NVARCHAR (50) NOT NULL,
    [balance]       FLOAT (53)    NOT NULL,
    [transactionId] INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Accounts] ([Id])
);


CREATE TABLE [dbo].[Transactions] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [name]        NVARCHAR (50) NOT NULL,
    [date]        SMALLDATETIME NOT NULL,
    [fromAccount] INT           NULL,
    [toAccount]   INT           NULL,
    [sum]         FLOAT (53)    NOT NULL,
    [status]      BIT           NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO
CREATE TRIGGER Only_One_Row
ON Bank
AFTER INSERT, UPDATE
AS
UPDATE Bank
SET Id = 1

GO
CREATE TRIGGER Account_isCheking_Update2
ON Credits
AFTER DELETE
AS
UPDATE Accounts
SET isCheking = 1
WHERE Id = (SELECT Id FROM deleted)
GO
CREATE TRIGGER Account_isCheking_Update
ON Credits
AFTER INSERT
AS
UPDATE Accounts
SET isCheking = 0
WHERE Id = (SELECT Id FROM inserted)

GO
CREATE TRIGGER Account_isCheking_Update4
ON Deposits
AFTER INSERT
AS
UPDATE Accounts
SET isCheking = 0
WHERE Id = (SELECT Id FROM inserted)
GO
CREATE TRIGGER Account_isCheking_Update3
ON Deposits
AFTER DELETE
AS
UPDATE Accounts
SET isCheking = 1
WHERE Id = (SELECT Id FROM deleted)

GO

USE MSSQLLocalBankDB;

SET IDENTITY_INSERT [dbo].[Clients] ON
INSERT INTO [dbo].[Clients] ([Id], [creditRating]) VALUES (1, 0)
INSERT INTO [dbo].[Clients] ([Id], [creditRating]) VALUES (2, 0)
INSERT INTO [dbo].[Clients] ([Id], [creditRating]) VALUES (3, 0)
INSERT INTO [dbo].[Clients] ([Id], [creditRating]) VALUES (4, 0)
INSERT INTO [dbo].[Clients] ([Id], [creditRating]) VALUES (5, 0)
INSERT INTO [dbo].[Clients] ([Id], [creditRating]) VALUES (6, 0)
SET IDENTITY_INSERT [dbo].[Clients] OFF

INSERT INTO [dbo].[NaturalPersons] ([Id], [name], [surname], [birthday]) VALUES (1, N'Игорь', N'Катамаранов', N'1979-01-17')
INSERT INTO [dbo].[NaturalPersons] ([Id], [name], [surname], [birthday]) VALUES (4, N'Юрий', N'Грачевич', N'1975-03-20')

INSERT INTO [dbo].[VIPs] ([Id], [name], [surname], [birthday], [workPlace]) VALUES (2, N'Сигурни', N'Уивер', N'1949-10-08', N'USCSS Nostromo')
INSERT INTO [dbo].[VIPs] ([Id], [name], [surname], [birthday], [workPlace]) VALUES (5, N'Элен', N'Рипли', N'2066-10-20', N'USCSS Nostromo')

INSERT INTO [dbo].[Companies] ([Id], [typeOrg], [Name]) VALUES (3, N'ООО', N'Weyland-Yutani Corporation')
INSERT INTO [dbo].[Companies] ([Id], [typeOrg], [Name]) VALUES (6, N'USCSS', N'Prometeus')

SET IDENTITY_INSERT [dbo].[Accounts] ON
INSERT INTO [dbo].[Accounts] ([Id], [clientId], [isOpen], [isRefill], [isWithdrawal], [openDate], [balance], [isCheking]) VALUES (1, 1, 1, 1, 1, N'2021-01-07 13:42:00', 4445, 1)
INSERT INTO [dbo].[Accounts] ([Id], [clientId], [isOpen], [isRefill], [isWithdrawal], [openDate], [balance], [isCheking]) VALUES (2, 4, 1, 1, 1, N'2021-01-07 13:42:00', 5000, 1)
INSERT INTO [dbo].[Accounts] ([Id], [clientId], [isOpen], [isRefill], [isWithdrawal], [openDate], [balance], [isCheking]) VALUES (3, 2, 1, 1, 1, N'2021-01-07 13:42:00', 5000, 1)
INSERT INTO [dbo].[Accounts] ([Id], [clientId], [isOpen], [isRefill], [isWithdrawal], [openDate], [balance], [isCheking]) VALUES (4, 5, 1, 1, 1, N'2021-01-07 13:42:00', 0, 1)
INSERT INTO [dbo].[Accounts] ([Id], [clientId], [isOpen], [isRefill], [isWithdrawal], [openDate], [balance], [isCheking]) VALUES (5, 3, 1, 1, 1, N'2021-01-07 13:42:00', 0, 1)
INSERT INTO [dbo].[Accounts] ([Id], [clientId], [isOpen], [isRefill], [isWithdrawal], [openDate], [balance], [isCheking]) VALUES (6, 6, 1, 1, 1, N'2021-01-07 13:42:00', 0, 1)
INSERT INTO [dbo].[Accounts] ([Id], [clientId], [isOpen], [isRefill], [isWithdrawal], [openDate], [balance], [isCheking]) VALUES (7, 1, 1, 0, 0, N'2021-01-07 13:42:00', 10000, 0)
INSERT INTO [dbo].[Accounts] ([Id], [clientId], [isOpen], [isRefill], [isWithdrawal], [openDate], [balance], [isCheking]) VALUES (8, 1, 1, 1, 1, N'2021-01-07 13:42:00', 10000, 0)
INSERT INTO [dbo].[Accounts] ([Id], [clientId], [isOpen], [isRefill], [isWithdrawal], [openDate], [balance], [isCheking]) VALUES (9, 4, 0, 0, 0, N'2021-01-07 13:42:00', 0, 1)
SET IDENTITY_INSERT [dbo].[Accounts] OFF

INSERT INTO [dbo].[Deposits] ([Id], [percent], [capitalization], [endDate], [previousCapitalization], [isActive]) VALUES (7, 10, 1, N'2021-07-07 13:42:00', N'2021-01-07 13:42:00', 1)

INSERT INTO [dbo].[Credits] ([Id], [percent], [startDebt], [curentDebt], [monthlyPayment], [endDate], [previousCapitalization], [isActive], [withoutNegativeBalance]) VALUES (8, 10, 10000, 10000, 1666.6666666666667, N'2021-07-07 13:42:00', N'2021-01-07 13:42:00', 1, 1)

INSERT INTO [dbo].[Bank] ([Id], [today], [name], [baseRate], [illegalChars]) VALUES (1, N'2021-01-07', N'Мой банк', 10, N'+')

SET IDENTITY_INSERT [dbo].[Transactions] ON
INSERT INTO [dbo].[Transactions] ([Id], [name], [date], [fromAccount], [toAccount], [sum], [status]) VALUES (1, N'Пополнение наличными', N'2021-01-07 13:42:00', 0, 1, 10000, 1)
INSERT INTO [dbo].[Transactions] ([Id], [name], [date], [fromAccount], [toAccount], [sum], [status]) VALUES (2, N'Перевод средств', N'2021-01-07 13:42:00', 1, 3, 5555, 1)
INSERT INTO [dbo].[Transactions] ([Id], [name], [date], [fromAccount], [toAccount], [sum], [status]) VALUES (3, N'Снятие наличных', N'2021-01-07 13:42:00', 3, 0, 555, 1)
INSERT INTO [dbo].[Transactions] ([Id], [name], [date], [fromAccount], [toAccount], [sum], [status]) VALUES (4, N'Пополнение наличными', N'2021-01-07 13:42:00', 0, 2, 5000, 1)
INSERT INTO [dbo].[Transactions] ([Id], [name], [date], [fromAccount], [toAccount], [sum], [status]) VALUES (5, N'Перевод средств', N'2021-01-07 13:42:00', 2, 9, 999, 1)
INSERT INTO [dbo].[Transactions] ([Id], [name], [date], [fromAccount], [toAccount], [sum], [status]) VALUES (6, N'Перевод средств', N'2021-01-07 13:42:00', 9, 2, 999, 1)
SET IDENTITY_INSERT [dbo].[Transactions] OFF

SET IDENTITY_INSERT [dbo].[ActivityInfo] ON
INSERT INTO [dbo].[ActivityInfo] ([Id], [date], [message]) VALUES (1, N'2021-01-07 13:42:00', N'Счет Id:1 сообщил: Баланс изменился: 0 --> 10000 (Транзакция Id 1)')
INSERT INTO [dbo].[ActivityInfo] ([Id], [date], [message]) VALUES (2, N'2021-01-07 13:42:00', N'Счет Id:1 сообщил: Баланс изменился: 10000 --> 4445 (Транзакция Id 2)')
INSERT INTO [dbo].[ActivityInfo] ([Id], [date], [message]) VALUES (3, N'2021-01-07 13:42:00', N'Счет Id:3 сообщил: Баланс изменился: 0 --> 5555 (Транзакция Id 2)')
INSERT INTO [dbo].[ActivityInfo] ([Id], [date], [message]) VALUES (4, N'2021-01-07 13:42:00', N'Счет Id:3 сообщил: Баланс изменился: 5555 --> 5000 (Транзакция Id 3)')
INSERT INTO [dbo].[ActivityInfo] ([Id], [date], [message]) VALUES (5, N'2021-01-07 13:42:00', N'Счет Id:2 сообщил: Баланс изменился: 0 --> 5000 (Транзакция Id 4)')
INSERT INTO [dbo].[ActivityInfo] ([Id], [date], [message]) VALUES (6, N'2021-01-07 13:42:00', N'Счет Id:2 сообщил: Баланс изменился: 5000 --> 4001 (Транзакция Id 5)')
INSERT INTO [dbo].[ActivityInfo] ([Id], [date], [message]) VALUES (7, N'2021-01-07 13:42:00', N'Счет Id:9 сообщил: Баланс изменился: 0 --> 999 (Транзакция Id 5)')
INSERT INTO [dbo].[ActivityInfo] ([Id], [date], [message]) VALUES (8, N'2021-01-07 13:42:00', N'Счет Id:9 сообщил: Баланс изменился: 999 --> 0 (Транзакция Id 6)')
INSERT INTO [dbo].[ActivityInfo] ([Id], [date], [message]) VALUES (9, N'2021-01-07 13:42:00', N'Счет Id:2 сообщил: Баланс изменился: 4001 --> 5000 (Транзакция Id 6)')
SET IDENTITY_INSERT [dbo].[ActivityInfo] OFF

SET IDENTITY_INSERT [dbo].[BalanceLog] ON
INSERT INTO [dbo].[BalanceLog] ([Id], [AccountId], [date], [message], [balance], [transactionId]) VALUES (1, 1, N'2021-01-07 13:42:00', N'10000', 10000, 1)
INSERT INTO [dbo].[BalanceLog] ([Id], [AccountId], [date], [message], [balance], [transactionId]) VALUES (2, 1, N'2021-01-07 13:42:00', N'4445', 4445, 2)
INSERT INTO [dbo].[BalanceLog] ([Id], [AccountId], [date], [message], [balance], [transactionId]) VALUES (3, 3, N'2021-01-07 13:42:00', N'5555', 5555, 2)
INSERT INTO [dbo].[BalanceLog] ([Id], [AccountId], [date], [message], [balance], [transactionId]) VALUES (4, 3, N'2021-01-07 13:42:00', N'5000', 5000, 3)
INSERT INTO [dbo].[BalanceLog] ([Id], [AccountId], [date], [message], [balance], [transactionId]) VALUES (5, 2, N'2021-01-07 13:42:00', N'5000', 5000, 4)
INSERT INTO [dbo].[BalanceLog] ([Id], [AccountId], [date], [message], [balance], [transactionId]) VALUES (6, 2, N'2021-01-07 13:42:00', N'4001', 4001, 5)
INSERT INTO [dbo].[BalanceLog] ([Id], [AccountId], [date], [message], [balance], [transactionId]) VALUES (7, 9, N'2021-01-07 13:42:00', N'999', 999, 5)
INSERT INTO [dbo].[BalanceLog] ([Id], [AccountId], [date], [message], [balance], [transactionId]) VALUES (8, 9, N'2021-01-07 13:42:00', N'0', 0, 6)
INSERT INTO [dbo].[BalanceLog] ([Id], [AccountId], [date], [message], [balance], [transactionId]) VALUES (9, 2, N'2021-01-07 13:42:00', N'5000', 5000, 6)
SET IDENTITY_INSERT [dbo].[BalanceLog] OFF