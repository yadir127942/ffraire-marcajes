
CREATE VIEW [dbo].[vistaQuitaAgregaTiempo]
AS 
	SELECT HORARIO
				 , CASE 
								WHEN RTRIM(LTRIM(agre_tmpto)) <> '' THEN ROUND(DATEPART(HOUR, CONVERT(DATETIME,SUBSTRING(RTRIM(LTRIM(agre_tmpto)),0,6)) ) + (DATEPART(MINUTE, CONVERT(DATETIME,SUBSTRING(RTRIM(LTRIM(agre_tmpto)),0,6)) )* 1.66667) / 100.00 ,2)  
								ELSE '0' 
					 END AS AgregaTiempo
				 , CASE 
								WHEN RTRIM(LTRIM(quit_tmpo)) <> '' THEN ROUND(DATEPART(HOUR, CONVERT(DATETIME,SUBSTRING(RTRIM(LTRIM(quit_tmpo)),0,6)) ) + (DATEPART(MINUTE, CONVERT(DATETIME,SUBSTRING(RTRIM(LTRIM(quit_tmpo)),0,6)) )* 1.66667) / 100.00 ,2)  
								ELSE '0'
					 END AS QuitaTiempo
		FROM horarios 
