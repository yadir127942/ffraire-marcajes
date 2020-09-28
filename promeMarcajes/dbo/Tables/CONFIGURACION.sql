CREATE TABLE [dbo].[CONFIGURACION] (
    [mintolent]  INT            CONSTRAINT [DF_CONFIGURACION_mintolent] DEFAULT ((0)) NOT NULL,
    [mintolsal]  INT            CONSTRAINT [DF_CONFIGURACION_mintolsal] DEFAULT ((0)) NOT NULL,
    [ajuste_emp] NVARCHAR (5)   CONSTRAINT [DF_CONFIGURACION_ajuste_emp] DEFAULT ('') NOT NULL,
    [respaldo]   BIT            CONSTRAINT [DF_CONFIGURACION_respaldo] DEFAULT ((0)) NOT NULL,
    [niv_resp]   INT            CONSTRAINT [DF_CONFIGURACION_niv_resp] DEFAULT ((0)) NOT NULL,
    [sem_resp]   INT            CONSTRAINT [DF_CONFIGURACION_sem_resp] DEFAULT ((0)) NOT NULL,
    [vac_pago]   INT            CONSTRAINT [DF_CONFIGURACION_vac_pago] DEFAULT ((0)) NOT NULL,
    [vac_decto]  INT            CONSTRAINT [DF_CONFIGURACION_vac_decto] DEFAULT ((0)) NOT NULL,
    [txt_aut]    NUMERIC (6, 2) CONSTRAINT [DF_CONFIGURACION_txt_aut] DEFAULT ((0)) NOT NULL
);

