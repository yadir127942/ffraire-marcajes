CREATE TABLE [dbo].[PERIODOS] (
    [fecha]      DATE         CONSTRAINT [DF_PERIODOS_fecha] DEFAULT ('2050-12-31') NOT NULL,
    [mes_cierre] INT          CONSTRAINT [DF_PERIODOS_mes_cierre] DEFAULT ((0)) NOT NULL,
    [s_fiscal]   INT          CONSTRAINT [DF_PERIODOS_s_fiscal] DEFAULT ((0)) NOT NULL,
    [decena]     INT          CONSTRAINT [DF_PERIODOS_decena] DEFAULT ((0)) NOT NULL,
    [quincena]   INT          CONSTRAINT [DF_PERIODOS_quincena] DEFAULT ((0)) NOT NULL,
    [mes]        INT          CONSTRAINT [DF_PERIODOS_mes] DEFAULT ((0)) NOT NULL,
    [bimestre]   INT          CONSTRAINT [DF_PERIODOS_bimestre] DEFAULT ((0)) NOT NULL,
    [turno]      INT          CONSTRAINT [DF_PERIODOS_turno] DEFAULT ((0)) NOT NULL,
    [dias]       INT          CONSTRAINT [DF_PERIODOS_dias] DEFAULT ((0)) NOT NULL,
    [tipo_dia]   NVARCHAR (1) CONSTRAINT [DF_PERIODOS_tipo_dia] DEFAULT ('') NOT NULL,
    [cierre]     NVARCHAR (1) CONSTRAINT [DF_PERIODOS_cierre] DEFAULT ('') NOT NULL,
    [a_fiscal]   INT          CONSTRAINT [DF_PERIODOS_a_fiscal] DEFAULT ((0)) NOT NULL,
    [shut_down]  INT          CONSTRAINT [DF_PERIODOS_shut_down] DEFAULT ((0)) NOT NULL,
    [tipo_dia_2] NVARCHAR (1) CONSTRAINT [DF_PERIODOS_tipo_dia_2] DEFAULT ('') NOT NULL,
    [a_sua]      INT          CONSTRAINT [DF_PERIODOS_a_sua] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_PERIODOS] PRIMARY KEY CLUSTERED ([fecha] ASC)
);

