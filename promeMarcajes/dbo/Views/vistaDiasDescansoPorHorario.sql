
CREATE VIEW [dbo].[vistaDiasDescansoPorHorario]
AS
	SELECT horario, lunes, martes, miercoles, jueves, viernes, sabado, domingo
				 , CASE 
								WHEN RTRIM(LTRIM(domingo)) = '' THEN 'domingo'
								WHEN RTRIM(LTRIM(sabado))  = '' THEN 'sabado'
								WHEN RTRIM(LTRIM(viernes))  = '' THEN 'viernes'
								WHEN RTRIM(LTRIM(jueves))  = '' THEN 'jueves'
								WHEN RTRIM(LTRIM(miercoles))  = '' THEN 'miercoles'
								WHEN RTRIM(LTRIM(martes))  = '' THEN 'martes'    
								WHEN RTRIM(LTRIM(lunes))  = '' THEN 'lunes'    
					 END AS descansoLaborado
		FROM horarios
