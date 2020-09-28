﻿CREATE TABLE [dbo].[MARCAJES] (
    [idMarcaje] BIGINT        IDENTITY (1, 1) NOT NULL,
    [clave_emp] NVARCHAR (5)  CONSTRAINT [DF__MARCAJES__clave___7E7452C3] DEFAULT ('') NOT NULL,
    [marcaje]   DATETIME2 (0) CONSTRAINT [DF__MARCAJES__marcaj__7F6876FC] DEFAULT ('2050-12-31') NOT NULL,
    [marcado]   BIT           CONSTRAINT [DF__MARCAJES__marcad__0150BF6E] DEFAULT ((0)) NOT NULL,
    [s_fiscal]  INT           CONSTRAINT [DF__MARCAJES__s_fisc__0244E3A7] DEFAULT ((0)) NOT NULL,
    [a_fiscal]  INT           CONSTRAINT [DF__MARCAJES__a_fisc__033907E0] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_MARCAJES] PRIMARY KEY CLUSTERED ([idMarcaje] ASC)
);

