﻿CREATE TABLE [dbo].[FALTAS] (
    [id_Faltas]  BIGINT         IDENTITY (1, 1) NOT NULL,
    [clave_emp]  NVARCHAR (5)   CONSTRAINT [DF__FALTAS__clave_em__3ABE46F4] DEFAULT ('') NOT NULL,
    [fecha_fal]  DATE           CONSTRAINT [DF__FALTAS__fecha_fa__3BB26B2D] DEFAULT ('2050-12-31') NOT NULL,
    [horas_fal]  NUMERIC (4, 2) CONSTRAINT [DF__FALTAS__horas_fa__3CA68F66] DEFAULT ((0)) NOT NULL,
    [semana_fal] INT            CONSTRAINT [DF__FALTAS__semana_f__3D9AB39F] DEFAULT ((0)) NOT NULL,
    [motivo_fal] NVARCHAR (50)  CONSTRAINT [DF__FALTAS__motivo_f__3E8ED7D8] DEFAULT ('') NOT NULL,
    [supervisor] NVARCHAR (50)  CONSTRAINT [DF__FALTAS__supervis__3F82FC11] DEFAULT ('') NOT NULL,
    [personal]   NVARCHAR (50)  CONSTRAINT [DF__FALTAS__personal__4077204A] DEFAULT ('') NOT NULL,
    [depto]      NVARCHAR (50)  CONSTRAINT [DF__FALTAS__depto__416B4483] DEFAULT ('') NOT NULL,
    [categ]      NVARCHAR (50)  CONSTRAINT [DF__FALTAS__categ__425F68BC] DEFAULT ('') NOT NULL,
    [turno]      NVARCHAR (50)  CONSTRAINT [DF__FALTAS__turno__43538CF5] DEFAULT ('') NOT NULL,
    [sdo_integ]  NUMERIC (8, 2) CONSTRAINT [DF__FALTAS__sdo_inte__4447B12E] DEFAULT ((0)) NOT NULL,
    [grado]      INT            CONSTRAINT [DF__FALTAS__grado__453BD567] DEFAULT ((0)) NOT NULL,
    [cve_pago]   CHAR (2)       CONSTRAINT [DF__FALTAS__cve_pago__462FF9A0] DEFAULT ('') NOT NULL,
    [bimestre]   INT            CONSTRAINT [DF__FALTAS__bimestre__47241DD9] DEFAULT ((0)) NOT NULL,
    [activo]     INT            CONSTRAINT [DF__FALTAS__activo__48184212] DEFAULT ((0)) NOT NULL,
    [captura]    DATETIME2 (0)  CONSTRAINT [DF__FALTAS__captura__490C664B] DEFAULT ('2050-12-31') NOT NULL,
    [modifica]   DATETIME2 (0)  CONSTRAINT [DF__FALTAS__modifica__4A008A84] DEFAULT ('2050-12-31') NOT NULL,
    [borra]      DATETIME2 (0)  CONSTRAINT [DF__FALTAS__borra__4AF4AEBD] DEFAULT ('2050-12-31') NOT NULL,
    [descrip]    VARCHAR (MAX)  CONSTRAINT [DF__FALTAS__descrip__4BE8D2F6] DEFAULT ('') NOT NULL,
    [tipo_capt]  CHAR (1)       CONSTRAINT [DF__FALTAS__tipo_cap__4CDCF72F] DEFAULT ('') NOT NULL,
    [referencia] CHAR (20)      CONSTRAINT [DF__FALTAS__referenc__4DD11B68] DEFAULT ('') NOT NULL,
    [laborable]  INT            CONSTRAINT [DF__FALTAS__laborabl__4EC53FA1] DEFAULT ((0)) NOT NULL,
    [verifica]   INT            CONSTRAINT [DF__FALTAS__verifica__4FB963DA] DEFAULT ((0)) NOT NULL,
    [impdia_gdo] NUMERIC (7, 2) CONSTRAINT [DF__FALTAS__impdia_g__50AD8813] DEFAULT ((0)) NOT NULL,
    [impdia_tur] NUMERIC (7, 2) CONSTRAINT [DF__FALTAS__impdia_t__51A1AC4C] DEFAULT ((0)) NOT NULL,
    [impdia_cer] NUMERIC (7, 2) CONSTRAINT [DF__FALTAS__impdia_c__5295D085] DEFAULT ((0)) NOT NULL,
    [a_fiscal]   INT            CONSTRAINT [DF__FALTAS__a_fiscal__5389F4BE] DEFAULT ((0)) NOT NULL,
    [orireg]     CHAR (1)       CONSTRAINT [DF__FALTAS__orireg__547E18F7] DEFAULT ('') NOT NULL,
    [cont_div]   CHAR (6)       CONSTRAINT [DF__FALTAS__cont_div__55723D30] DEFAULT ('') NOT NULL,
    [refleja]    BIT            CONSTRAINT [DF__FALTAS__refleja__56666169] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_FALTAS] PRIMARY KEY CLUSTERED ([id_Faltas] ASC)
);

