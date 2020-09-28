
CREATE PROCEDURE [dbo].[ActualizaDatosPermisoParcialPL] @DatosPL PLPermisoParcial READONLY
AS

--Proceso comentado  para poder analizar que dato actualizo en caso de ser necesario 
--IF OBJECT_ID ('verificaPL') is not null 
--drop table verificaPL ;

--DECLARE @Datos TABLE (
--	id_ADP BIGINT
--	,clave_emp NVARCHAR(5)
--    ,horas_lab decimal(4,2)
--	,horas_nor decimal(4,2)
--	,horas_ext decimal(4,2)
--	,observa nvarchar(30)
--	,mot_010 nvarchar(1)
--	,mot_101 nvarchar(1)
--	,fuera_turn nvarchar(1)
--	)

		UPDATE asis_dia_perm
			 SET 		
					 observa = D.observa
					 , mot_024 = D.mot_024
					 , mot_011 = D.mot_011	
--OUTPUT inserted.id_ADP
--	  ,inserted.clave_emp
--	  ,inserted.horas_lab
--	  ,inserted.horas_nor
--	  ,inserted.horas_ext
--	  ,inserted.observa
--	  ,inserted.mot_010 
--	  ,inserted.mot_101 
--	  ,inserted.fuera_turn
--INTO @datos
			FROM asis_dia_perm AS A
INNER JOIN @DatosPL AS D ON A.id_ADP = D.id_ADP

--select * into verificaPL from @datos
--motivo = TipoMotivos.SinIncidencia;
