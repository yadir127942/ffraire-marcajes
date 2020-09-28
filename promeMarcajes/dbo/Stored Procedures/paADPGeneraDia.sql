
CREATE PROCEDURE [dbo].[paADPGeneraDia] (
	@fechaLista DATE
	,@nombreDia NVARCHAR(10)
	)
AS
BEGIN
	SET @nombreDia = CASE DATENAME(dw, @fechaLista)
												WHEN 'Monday' THEN 'Lunes'
												WHEN 'Tuesday' THEN 'Martes'
												WHEN 'Wednesday' THEN 'Miércoles'
												WHEN 'Thursday' THEN 'Jueves'
												WHEN 'Friday' THEN 'Viernes'
												WHEN 'Saturday' THEN 'Sábado'
												WHEN 'Sunday' THEN 'Domingo'
									 END;

	WITH empleados AS (
		SELECT P.clave_emp, P.depto, P.fecha_ing, P.categoria, P.grado_niv, P.supervisor, P.cont_div, P.wturno, P.turno, P.cve_pago
					 , H.horas_lun, H.horas_mar, H.horas_mie, H.horas_jue, H.horas_vie, H.horas_sab, H.horas_dom
					 , F.a_fiscal, F.s_fiscal
			FROM PERSONAL AS P
 LEFT JOIN HORARIOS AS H ON P.turno = H.horario
INNER JOIN PERIODOS AS F ON f.fecha = @fechaLista
		 WHERE p.fecha_ing <= @fechaLista
					 AND p.fecha_baja >= @fechaLista
					 AND p.grado_niv < 10
	), previoADP AS (
		SELECT e.clave_emp, e.depto, @fechaLista AS fecha_mov, e.categoria, e.grado_niv, e.supervisor AS superv
					 , e.cont_div AS division, e.wturno AS turno, e.turno AS horario, e.cve_pago
					 , CASE 
									WHEN @nombreDia IN ('Lunes','Monday') THEN e.horas_lun
									WHEN @nombreDia IN ('Martes','Tuesday') THEN e.horas_mar
									WHEN @nombreDia IN ('Miércoles','Wednesday') THEN e.horas_mie
									WHEN @nombreDia IN ('Jueves','Thursday') THEN e.horas_jue
									WHEN @nombreDia IN ('Viernes','Friday') THEN e.horas_vie
									WHEN @nombreDia IN ('Sábado','Saturday') THEN e.horas_sab
									WHEN @nombreDia IN ('Domingo','Sunday') THEN e.horas_dom
						 END AS horas_nor
					, e.a_fiscal
					, e.s_fiscal
					, (
						 --Regresa el tipo de dia en los siguientes casos F - Festivo , N-Normal (L-S), D-Domingo
							CASE dbo.fnObtieneTipoDiaCalendarioLaboral(@fechaLista, e.cont_div)
									 WHEN 'D' THEN 'N'
									 ELSE dbo.fnObtieneTipoDiaCalendarioLaboral(@fechaLista, e.cont_div)
							END
						) AS tipo_mov
		 FROM empleados AS e
LEFT JOIN asis_dia_perm AS a ON e.clave_emp = a.clave_emp AND a.fecha_mov = @fechaLista
		WHERE a.clave_emp IS NULL
	)
	INSERT INTO ASIS_DIA_PERM (clave_emp, fecha_mov, horas_nor, depto, division, superv, categoria, horario
														 , turno, grado_niv, cve_pago, tipo_mov, observa, horas_lab, s_fiscal, a_fiscal, diasem)
	SELECT clave_emp, fecha_mov, horas_nor, depto, division, superv, categoria, horario
				 , turno, grado_niv, cve_pago
				 --SI EL DIA NO ES LABORABLE Y ES FESTIVO, SE TOMA COMO DIA NORMAL. CASO CONTRARIO SE TOMA EL DIA DE PERIODOS
				 , CASE 
								WHEN horas_nor = 0 AND tipo_mov = 'F' THEN 'N'
								ELSE tipo_mov
					 END AS tipo_mov
				 --SI EL DIA ES LABORABLE Y ES FESTIVO, SE OBSERVA COMO TAL. CASO CONTRARIO NO SE OBSERVA NADA
				 , CASE 
								WHEN horas_nor > 0 AND tipo_mov = 'F' THEN 'FESTIVO'
								ELSE ''
					 END AS observa
				 --SI EL DIA ES NORMAL Ó DOMINGO (FUNCION QUE REGRESA TIPO DIA)  , SE INICIAN LAS HORAS LABORADAS EN 0. CASO CONTRARIO SE INICIAN COMO NORMALES
				 , CASE 
								WHEN tipo_mov IN ('N') THEN 0
								ELSE horas_nor
					 END AS horas_lab
				 , s_fiscal
				 , a_fiscal
				 , @nombreDia AS diasem
		FROM previoADP
ORDER BY division, horario, clave_emp
END
