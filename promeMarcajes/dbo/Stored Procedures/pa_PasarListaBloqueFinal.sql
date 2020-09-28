
CREATE PROCEDURE [dbo].[pa_PasarListaBloqueFinal] @pniWSFiscal INT
	,@pniWAFiscal INT
	,@Bimestre INT
	,@CEmp NVARCHAR(5) = ''
AS
BEGIN
	--INSERTA AUSENTISMO(FALTAS) PARA :
	--RETARDO + 5MINS
	--PERMISO PARCIAL +4 HORAS
	--RETARDO + 4HRS 
	--PERMISO - 4HRS
	--PERMISO - 4HRS CON RETARDO + 5MINS
	--PERMISO - 4HRS CON RETARDO + 4HRS
	--PERMISO + 4HRS CON RETARDO + 5MINS
	--PERMISO + 4HRS CON RETARDO + 4HRS
	INSERT INTO FALTAS (clave_emp, fecha_fal, horas_fal, semana_fal, motivo_fal, supervisor, personal, depto
											, categ, turno, sdo_integ, grado, cve_pago, bimestre, activo, captura, tipo_capt
											, referencia, laborable, a_fiscal, cont_div)
	 SELECT A.clave_emp, A.fecha_mov, A.horas_nor, @pniWSFiscal
					, CASE 
								 WHEN mot_010 = 1 THEN '010'
								 WHEN mot_011 = 1 THEN '011'
								 WHEN mot_101 = 1 THEN '101'
								 WHEN mot_024 = 1 THEN '024'
								 WHEN mot_025 = 1 THEN '025'
								 WHEN mot_026 = 1 THEN '026'
								 WHEN mot_027 = 1 THEN '027'
								 WHEN mot_028 = 1 THEN '028'
								 ELSE ''
					 END
					, A.superv, '05634', A.depto
					, A.categoria, A.turno, A.sdo_integ, A.grado_niv, A.cve_pago, @Bimestre, 1, GETDATE(), 'A'
					, 'SISTEMA', 1, A.a_fiscal, A.division
		 FROM ASIS_DIA_PERM AS A
LEFT JOIN FALTAS AS F ON A.clave_emp = F.clave_emp AND A.fecha_mov = F.fecha_fal
		WHERE A.s_fiscal = @pniWSFiscal 
					AND A.a_fiscal = @pniWAFiscal
					AND A.horas_nor > 0
					AND CONVERT(INT, A.clave_emp) < 90000
					AND (
								RTRIM(LTRIM(mot_010)) = '1'
								OR RTRIM(LTRIM(mot_011)) = '1'
								OR RTRIM(LTRIM(mot_101)) = '1'
								OR RTRIM(LTRIM(mot_024)) = '1'
								OR RTRIM(LTRIM(mot_025)) = '1'	--AGREGADO
								OR RTRIM(LTRIM(mot_026)) = '1'	--AGREGADO
								OR RTRIM(LTRIM(mot_027)) = '1'	--AGREGADO
								OR RTRIM(LTRIM(mot_028)) = '1'	--AGREGADO
							)
					AND fecha_mov BETWEEN (
							SELECT TOP 1 MIN(fecha_mov)
								FROM ASIS_DIA_PERM
							 WHERE s_fiscal = @pniWSFiscal AND a_fiscal = @pniWAFiscal
							)
							AND (
							SELECT TOP 1 MAX(fecha_mov)
								FROM ASIS_DIA_PERM
							 WHERE s_fiscal = @pniWSFiscal AND a_fiscal = @pniWAFiscal
							)
					AND F.clave_emp IS NULL
					AND 1 = CASE @CEmp
											 WHEN '' THEN 1
											 ELSE CASE @CEmp
																 WHEN @CEmp THEN CASE 
																										  WHEN A.clave_emp = @CEmp THEN 1
																										  ELSE 0
																								 END
														END
									END
END


