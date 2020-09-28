
CREATE FUNCTION [dbo].[fnObtieneTipoDiaCalendarioLaboral] (
	@fecha DATE
	, @division NVARCHAR(6)
	)
RETURNS NVARCHAR(2)
AS
BEGIN
	DECLARE @tipoDia NVARCHAR(2)

	SET @tipoDia = (
									SELECT D.tipo
										FROM periodos AS P
							 LEFT JOIN CalendarioLaboral AS C ON P.fecha = C.fecha
							 LEFT JOIN CalendarioLaboralClaves AS D ON C.idTipo = D.id
									 WHERE C.fecha = @Fecha
												 AND C.division = @Division
								 )

	IF @tipoDia IS NULL
	BEGIN
		SET @tipoDia = CASE 
												WHEN DATENAME(DW, @fecha) NOT IN ('Sunday','Domingo') THEN 'N'
												ELSE 'D'
									 END
	END

	RETURN @tipoDia
END
