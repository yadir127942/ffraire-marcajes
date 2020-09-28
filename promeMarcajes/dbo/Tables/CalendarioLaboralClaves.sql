CREATE TABLE [dbo].[CalendarioLaboralClaves] (
    [id]          INT           IDENTITY (1, 1) NOT NULL,
    [tipo]        NVARCHAR (2)  DEFAULT ('') NOT NULL,
    [descripcion] NVARCHAR (80) DEFAULT ('') NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);

