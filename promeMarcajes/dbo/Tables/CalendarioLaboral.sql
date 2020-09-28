CREATE TABLE [dbo].[CalendarioLaboral] (
    [id]       BIGINT       IDENTITY (1, 1) NOT NULL,
    [fecha]    DATE         DEFAULT ('2050-12-31') NOT NULL,
    [division] NVARCHAR (6) DEFAULT ('') NOT NULL,
    [idTipo]   INT          DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);

