CREATE PROCEDURE [dbo].[paActualizaSupervisorActualSemanaEnCurso]
AS
BEGIN
	 DECLARE @FechaRestauraSupervisor DATE = (SELECT CONVERT(DATE, GETDATE()))
	 DECLARE @SFiscalRestauraSupervisor INT = (SELECT s_fiscal FROM periodos WHERE fecha = @FechaRestauraSupervisor)
	 DECLARE @AFiscalRestauraSupervisor INT = (SELECT a_fiscal FROM periodos WHERE fecha = @FechaRestauraSupervisor);

		UPDATE ASIS_DIA_PERM
			 SET superv = P.supervisor
			FROM ASIS_DIA_PERM AS A
INNER JOIN PERSONAL AS P ON A.clave_emp = P.clave_emp
		 WHERE A.s_fiscal = @SFiscalRestauraSupervisor
					 AND A.a_fiscal = @AFiscalRestauraSupervisor
					 AND A.superv <> P.supervisor;
END
