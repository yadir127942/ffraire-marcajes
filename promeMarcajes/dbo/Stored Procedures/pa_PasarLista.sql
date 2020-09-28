
CREATE PROCEDURE [dbo].[pa_PasarLista] @pniWSFiscal INT
	,@pniWAFiscal INT
	,@CEmp NVARCHAR(5) = ''
AS
BEGIN

 --Restaura Supervisor Correcto en semana actual , anexo 2019-09-19	
	exec paActualizaSupervisorActualSemanaEnCurso

	DECLARE @Bimestre INT

	SET @Bimestre = (
			SELECT TOP 1 bimestre
			FROM periodos
			WHERE s_fiscal = @pniWSFiscal
				AND a_fiscal = @pniWAFiscal
			)

	--LIMPIA : DE ADP HORAS ENTRADA SALIDA
	UPDATE asis_dia_perm
		 SET hora_ent = ''
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND RTRIM(LTRIM(hora_ent)) = ':'

	UPDATE asis_dia_perm
		 SET hora_sal = ''
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND RTRIM(LTRIM(hora_sal)) = ':'

	UPDATE asis_dia_perm
		 SET hora_ent2 = ''
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND RTRIM(LTRIM(hora_ent2)) = ':'

	UPDATE asis_dia_perm
		 SET hora_sal2 = ''
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND RTRIM(LTRIM(hora_sal2)) = ':'

	------------------------------------------
	-----------PASE DE LISTA 1-5--------------
	------------------------------------------
	 UPDATE ASIS_DIA_PERM
			SET horas_nor = 0.00
					, observa = ''
					, tipo_mov = 'N'
		 FROM ASIS_DIA_PERM AS A
LEFT JOIN vistaobtieneHorario AS V ON A.horario = V.horario
		WHERE A.s_fiscal = @pniWSFiscal
					AND A.a_fiscal = @pniWAFiscal
					AND fecha_mov <= CONVERT(DATE, GETDATE())
					AND 1 = CASE @CEmp
											 WHEN '' THEN 1
											 ELSE CASE @CEmp
																 WHEN @CEmp THEN CASE 
																											WHEN A.clave_emp = @CEmp THEN 1
																											ELSE 0
																								 END
														END
									END
					AND horas_nor > 0
					AND ( CONVERT(INT, clave_emp) < 90000 OR CONVERT(INT, clave_emp) >= 95000 )
					AND (
								(RTRIM(LTRIM(hora_ent)) = '' AND RTRIM(LTRIM(hora_sal)) = '')
								OR 
								(RTRIM(LTRIM(hora_ent)) = ':' AND RTRIM(LTRIM(hora_sal)) = ':')
							)
					AND RTRIM(LTRIM(hora_ent2)) = '' AND RTRIM(LTRIM(hora_sal2)) = ''
					AND RTRIM(LTRIM(hora_ent3)) = '' AND RTRIM(LTRIM(hora_sal3)) = ''
					AND RTRIM(LTRIM(hora_ent4)) = '' AND RTRIM(LTRIM(hora_sal4)) = ''
					AND RTRIM(LTRIM(tipo_mov)) IN ('N','A')
					AND 1 = CASE 
											 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('MONDAY', 'LUNES') THEN 
														CASE
																 WHEN lunesEntrada = - 1 THEN 1
																 ELSE 0
														END
											 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('TUESDAY', 'MARTES') THEN 
														CASE 
																 WHEN martesEntrada = - 1 THEN 1
																 ELSE 0
														END
											 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('WEDNESDAY', 'MIÉRCOLES') THEN
														CASE 
																 WHEN miercolesEntrada = - 1 THEN 1
																 ELSE 0
														END
											 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('THURSDAY', 'JUEVES') THEN
														CASE 
																 WHEN juevesEntrada = - 1 THEN 1
																 ELSE 0
														END
											 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('FRIDAY', 'VIERNES') THEN 
														CASE 
																 WHEN viernesEntrada = - 1 THEN 1
																 ELSE 0
														END
											 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('SATURDAY', 'SÁBADO') THEN 
														CASE 
																 WHEN sabadoEntrada = - 1 THEN 1
																 ELSE 0
														END
											 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('SUNDAY', 'DOMINGO') THEN 
														CASE 
																 WHEN domingoEntrada = - 1 THEN 1
																 ELSE 0
														END
									END

	UPDATE asis_dia_perm
		 SET tipo_mov = 'N'
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND RTRIM(LTRIM(tipo_mov)) IN ('V', 'P')
				 AND 1 = CASE @CEmp
											WHEN '' THEN 1
											ELSE CASE @CEmp
																WHEN @CEmp THEN CASE 
																										 WHEN clave_emp = @CEmp THEN 1
																										 ELSE 0
																								END
													 END
								 END

	--FILTROS A MOVIMIENTOS ASIS_DIA_PERM QUE SEAN NORMALES
	--ACTUALIZA COMPO OBSERVA
	UPDATE asis_dia_perm
		 SET observa = ''
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND RTRIM(LTRIM(tipo_mov)) NOT IN ('D', 'T', 'X', 'E', 'Z', 'C', 'O', 'W')
				 AND 1 = CASE @CEmp
											WHEN '' THEN 1
											ELSE CASE @CEmp
																WHEN @CEmp THEN CASE 
																										 WHEN clave_emp = @CEmp THEN 1
																										 ELSE 0
																								END
													 END
								 END

	--ACTUALIZA TIPO DE MOVIMIENTO 
	UPDATE asis_dia_perm
		 SET tipo_mov = 'N'
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND RTRIM(LTRIM(tipo_mov)) = 'A'
				 AND CONVERT(INT, clave_emp) < 90000
				 AND LEN(RTRIM(LTRIM(hora_ent))) < 5
				 AND LEN(RTRIM(LTRIM(hora_sal))) = 5
				 AND RTRIM(LTRIM(tipo_mov)) NOT IN ('D', 'T', 'X', 'E', 'Z', 'C', 'O', 'W')
				 AND 1 = CASE @CEmp
											WHEN '' THEN 1
											ELSE CASE @CEmp
																WHEN @CEmp THEN CASE 
																										 WHEN clave_emp = @CEmp THEN 1
																										 ELSE 0
																								END
													 END
								END

	--TIPO DE MOVIMIENTO MOT_010
	UPDATE asis_dia_perm
		 SET mot_010 = '1'
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND CONVERT(INT, clave_emp) < 90000
				 AND LEN(RTRIM(LTRIM(hora_ent))) < 5
				 AND LEN(RTRIM(LTRIM(hora_sal))) = 5
				 AND horas_nor <> 0 --Anexo 29/Mayo/2017  solo puede tener retado en dias habiles D. Mendoza
				 AND RTRIM(LTRIM(tipo_mov)) NOT IN ('D', 'T', 'X', 'E', 'Z', 'C', 'O', 'W')
				 AND 1 = CASE @CEmp
											WHEN '' THEN 1
											ELSE CASE @CEmp
																WHEN @CEmp THEN CASE 
																										 WHEN clave_emp = @CEmp THEN 1
																										 ELSE 0
																								END
													 END
								 END

	--RETARDO MAS DE 5 MINUTOS
	UPDATE asis_dia_perm
		 SET observa = 'RETARDO +5 MINUTOS'
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND RTRIM(LTRIM(tipo_mov)) = 'N'
				 AND CONVERT(INT, clave_emp) < 90000
				 AND LEN(RTRIM(LTRIM(hora_ent))) < 5
				 AND LEN(RTRIM(LTRIM(hora_sal))) = 5
				 AND RTRIM(LTRIM(tipo_mov)) NOT IN ('D', 'T', 'X', 'E', 'Z', 'C', 'O', 'W')
				 AND horas_nor <> 0 --Anexo 29/Mayo/2017  solo puede tener retado en dias habiles D. Mendoza
				 AND 1 = CASE @CEmp
											WHEN '' THEN 1
											ELSE CASE @CEmp
																WHEN @CEmp THEN CASE 
																										 WHEN clave_emp = @CEmp THEN 1
																										 ELSE 0
																								END
													 END
								 END

	--HORAS LABORARES [HORAS_LAB]
	UPDATE asis_dia_perm
		 SET horas_lab = 0
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND RTRIM(LTRIM(tipo_mov)) = 'N'
				 AND CONVERT(INT, clave_emp) < 90000
				 AND (
							( LEN(RTRIM(LTRIM(hora_ent))) < 5 AND LEN(RTRIM(LTRIM(hora_sal))) = 5 )
							OR 
							( LEN(RTRIM(LTRIM(hora_ent))) = 5 AND LEN(RTRIM(LTRIM(hora_sal))) < 5 )
						 )
				 AND RTRIM(LTRIM(tipo_mov)) NOT IN ('D', 'T', 'X', 'E', 'Z', 'C', 'O', 'W')
				 AND 1 = CASE @CEmp
											WHEN '' THEN 1
											ELSE CASE @CEmp
																WHEN @CEmp THEN CASE 
																										 WHEN clave_emp = @CEmp THEN 1
																										 ELSE 0
																								END
													 END
								 END

	--BORRA MOTIVO 005 FALTAS INJUSTIFICADAS  PASE DE LISTA COMPLETO
	--HORAS EXTRAS [HORAS_EXT] SOLO PARA PASE LISTA COMPLETO
	IF RTRIM(LTRIM(@CEmp)) = ''
	BEGIN
		UPDATE asis_dia_perm
			 SET mot_05 = ''
		 WHERE s_fiscal = @pniWSFiscal
					 AND a_fiscal = @pniWAFiscal
					 AND RTRIM(LTRIM(tipo_mov)) NOT IN ('D', 'T', 'X', 'E', 'Z', 'C', 'O', 'W')

	 UPDATE asis_dia_perm
			SET horas_ext = 0
		WHERE s_fiscal = @pniWSFiscal
					AND a_fiscal = @pniWAFiscal
					AND RTRIM(LTRIM(tipo_mov)) = 'N'
					AND CONVERT(INT, clave_emp) < 90000
					AND ( LEN(RTRIM(LTRIM(hora_ent))) < 5 OR LEN(RTRIM(LTRIM(hora_sal))) < 5 )
					AND RTRIM(LTRIM(tipo_mov)) NOT IN ('D', 'T', 'X', 'E', 'Z', 'C', 'O', 'W')
	END

	------------------------------------------
	-----------PASE DE LISTA 2-5--------------
	------------------------------------------
	--LIMPIA MOTIVOS 
		UPDATE asis_dia_perm
			 SET mot_01 = '', mot_02 = '', mot_03 = '', mot_04 = '', mot_05 = '', mot_06 = '', mot_07 = '', mot_09 = '', mot_010 = ''
					 , mot_011 = '', mot_012 = '', mot_013 = '', mot_016 = '', mot_017 = '', mot_018 = '', mot_019 = '', mot_020 = ''
					 , mot_023 = '', mot_101 = '', fuera_turn = 0, observa = '', hrs_ext_au = 0.00, tesem = 0.00, tedesc = 0.00
					 --Modificacion DM Creacion : 2017/07/19 Produccion : N/A Anexo motivos combinados , permiso - 4 Hrs
					 , mot_024 = '', mot_025 = '', mot_026 = '', mot_027 = '', mot_028 = ''
			FROM asis_dia_perm AS A
INNER JOIN faltas AS F ON A.clave_emp = F.clave_emp AND A.fecha_mov = F.fecha_fal
		 WHERE A.s_fiscal = @pniWSFiscal
					 AND A.a_fiscal = @pniWAFiscal
					 AND RTRIM(LTRIM(A.tipo_mov)) NOT IN ('D', 'T', 'X', 'E', 'Z', 'C', 'O', 'W', 'F')
					 AND 1 = CASE @CEmp
												WHEN '' THEN 1
												ELSE CASE @CEmp
																	WHEN @CEmp THEN CASE 
																											 WHEN A.clave_emp = @CEmp THEN 1
																											 ELSE 0
																									END
														 END
									 END

	--ACTUALIZA  OBSERVA -- MOTIVOS FATLAS
		UPDATE asis_dia_perm
			 SET observa = ISNULL(M.des_falta, '')
					 , mot_01 = ( SELECT CASE F.motivo_fal WHEN '001' THEN '1' ELSE A.mot_01 END )
					 , mot_02 = ( SELECT CASE F.motivo_fal WHEN '002' THEN '1' ELSE A.mot_02 END )
					 , mot_03 = ( SELECT CASE F.motivo_fal WHEN '003' THEN '1' ELSE A.mot_03 END )
					 , mot_04 = ( SELECT CASE F.motivo_fal WHEN '004' THEN '1' ELSE A.mot_04 END )
					 , mot_05 = ( SELECT CASE F.motivo_fal WHEN '005' THEN '1' ELSE A.mot_05 END )
					 , mot_06 = ( SELECT CASE F.motivo_fal WHEN '006' THEN '1' ELSE A.mot_06 END )
					 , mot_07 = ( SELECT CASE F.motivo_fal WHEN '007' THEN '1' ELSE A.mot_07 END )
					 , mot_09 = ( SELECT CASE F.motivo_fal WHEN '009' THEN '1' ELSE A.mot_09 END )
					 , mot_010 = ( SELECT CASE F.motivo_fal WHEN '010' THEN '1' ELSE A.mot_010 END )
					 , mot_011 = ( SELECT CASE F.motivo_fal WHEN '011' THEN '1' ELSE A.mot_011 END )
					 , mot_012 = ( SELECT CASE F.motivo_fal WHEN '012' THEN '1' ELSE A.mot_012 END )
					 , mot_013 = ( SELECT CASE F.motivo_fal WHEN '013' THEN '1' ELSE A.mot_013 END )
					 , mot_016 = ( SELECT CASE F.motivo_fal WHEN '016' THEN '1' ELSE A.mot_016 END )
					 , mot_017 = ( SELECT CASE F.motivo_fal WHEN '017' THEN '1' ELSE A.mot_017 END )
					 , mot_018 = ( SELECT CASE F.motivo_fal WHEN '018' THEN '1' ELSE A.mot_018 END )
					 , mot_019 = ( SELECT CASE F.motivo_fal WHEN '019' THEN '1' ELSE A.mot_019 END )
					 , mot_020 = ( SELECT CASE F.motivo_fal WHEN '020' THEN '1' ELSE A.mot_020 END )
					 , mot_023 = ( SELECT CASE F.motivo_fal WHEN '023' THEN '1' ELSE A.mot_023 END )
					 --Modificacion DM Creacion : 2017/07/19 Produccion : N/A Anexo motivos combinados , permiso - 4 Hrs
					 , mot_024 = ( SELECT CASE F.motivo_fal WHEN '024' THEN '1' ELSE A.mot_024 END )
					 , mot_025 = ( SELECT CASE F.motivo_fal WHEN '025' THEN '1' ELSE A.mot_025 END )
					 , mot_026 = ( SELECT CASE F.motivo_fal WHEN '026' THEN '1' ELSE A.mot_026 END )
					 , mot_027 = ( SELECT CASE F.motivo_fal WHEN '027' THEN '1' ELSE A.mot_027 END )
					 , mot_028 = ( SELECT CASE F.motivo_fal WHEN '028' THEN '1' ELSE A.mot_028 END )
			FROM asis_dia_perm AS A
INNER JOIN faltas AS F ON A.clave_emp = F.clave_emp AND A.fecha_mov = F.fecha_fal
 LEFT JOIN mot_fals AS M ON CONVERT(INT, F.motivo_fal) = CONVERT(INT, M.motivo)
		 WHERE A.s_fiscal = @pniWSFiscal
					 AND A.a_fiscal = @pniWAFiscal
					 AND RTRIM(LTRIM(A.tipo_mov)) NOT IN ('D', 'T', 'X', 'E', 'Z', 'C', 'O', 'W', 'F')
					 AND 1 = CASE @CEmp
												WHEN '' THEN 1
												ELSE CASE @CEmp
																	WHEN @CEmp THEN CASE 
																											 WHEN A.clave_emp = @CEmp THEN 1
																											 ELSE 0
																									END
														 END
									 END

	--PRIMER CASE ('001' ,'002', '003', '012', '023') [ UPDATE TIPO_MOV -- HORAS_LAB -- HORA_ENT -- HORA_SAL -- HORA_ENT2 -- HORA_SAL2 ]
	--INCAPACIDADES 
		UPDATE asis_dia_perm
			 SET tipo_mov = 'I'
					 , hora_ent = '' , hora_sal = ''
					 , hora_ent2 = '' , hora_sal2 = ''
					 , hora_ent3 = '', hora_sal3 = ''
					 , hora_ent4 = '', hora_sal4 = ''
					 , horas_lab = ( SELECT CASE F.motivo_fal
																			 WHEN '023' THEN A.horas_nor
																			 ELSE 0.00
																	END
												 )
			FROM asis_dia_perm AS A
INNER JOIN faltas AS F ON A.clave_emp = F.clave_emp AND A.fecha_mov = F.fecha_fal
 LEFT JOIN mot_fals AS M ON CONVERT(INT, F.motivo_fal) = CONVERT(INT, M.motivo)
		 WHERE A.s_fiscal = @pniWSFiscal
					 AND A.a_fiscal = @pniWAFiscal
					 AND RTRIM(LTRIM(A.tipo_mov)) NOT IN ('D', 'T', 'X', 'E', 'C', 'O', 'W', 'F')
					 AND motivo_fal IN ('001', '002', '003', '012', '023')
					 AND 1 = CASE @CEmp
												WHEN '' THEN 1
												ELSE CASE @CEmp
																	WHEN @CEmp THEN CASE 
																											 WHEN A.clave_emp = @CEmp THEN 1
																											 ELSE 0
																									END
														 END
									 END

	--SEGUNDO CASE ('004' ,'006','011','013', '016', '017', '018','019','020')  [ UPDATE TIPO_MOV -- HORAS_LAB -- HORA_ENT -- HORA_SAL -- HORA_ENT2 -- HORA_SAL2 ]]
		UPDATE asis_dia_perm
			 SET tipo_mov = 'P'
					 , hora_ent = ( SELECT CASE WHEN F.motivo_fal NOT IN ('011') THEN A.hora_ent ELSE '' END )
					 , hora_sal = ( SELECT CASE WHEN F.motivo_fal IN ('011') THEN A.hora_sal ELSE '' END )
					 , hora_ent2 = ( SELECT CASE WHEN F.motivo_fal IN ('011') THEN A.hora_ent2 ELSE '' END )
					 , hora_sal2 = ( SELECT CASE WHEN F.motivo_fal IN ('011') THEN A.hora_sal2 ELSE '' END )
					 , horas_lab = ( SELECT CASE WHEN F.motivo_fal IN ('011') THEN A.horas_lab ELSE A.horas_nor END )
			FROM asis_dia_perm AS A
INNER JOIN faltas AS F ON A.clave_emp = F.clave_emp AND A.fecha_mov = F.fecha_fal
 LEFT JOIN mot_fals AS M ON CONVERT(INT, F.motivo_fal) = CONVERT(INT, M.motivo)
		 WHERE A.s_fiscal = @pniWSFiscal
					 AND A.a_fiscal = @pniWAFiscal
					 AND RTRIM(LTRIM(A.tipo_mov)) NOT IN ('D', 'T', 'X', 'E', 'C', 'O', 'W', 'F')
					 AND F.motivo_fal IN ('004', '006', '011', '013', '016', '017', '018', '019', '020')
					 AND 1 = CASE @CEmp
												WHEN '' THEN 1
												ELSE CASE @CEmp
																	WHEN @CEmp THEN CASE 
																											 WHEN A.clave_emp = @CEmp THEN 1
																											 ELSE 0
																									END
														 END
									 END

	--TERCER CASE ('007' ,'009')  [ UPDATE TIPO_MOV -- HORAS_LAB -- HORA_ENT -- HORA_SAL -- HORA_ENT2 -- HORA_SAL2 ]]
		UPDATE asis_dia_perm
			 SET hora_ent = '' , hora_sal = ''
					 , hora_ent2 = '' , hora_sal2 = ''
					 , hora_ent3 = '', hora_sal3 = ''
					 , hora_ent4 = '', hora_sal4 = ''
					 --,horas_ext= 0 --en vacaciones o suspension no puede tener horas extras
					 , tipo_mov = ( SELECT CASE F.motivo_fal
																			WHEN '007' THEN 'A'
																			ELSE 'V'
																 END
												)
					 , horas_lab = ( SELECT CASE F.motivo_fal
																			 WHEN '007' THEN 0--A.horas_lab -- Por suspension se le quitan horas laboradas
																			 ELSE A.horas_nor
																	END
												 )
			FROM asis_dia_perm AS A
INNER JOIN faltas AS F ON A.clave_emp = F.clave_emp AND A.fecha_mov = F.fecha_fal
 LEFT JOIN mot_fals AS M ON CONVERT(INT, F.motivo_fal) = CONVERT(INT, M.motivo)
		 WHERE A.s_fiscal = @pniWSFiscal
					 AND A.a_fiscal = @pniWAFiscal
					 AND RTRIM(LTRIM(A.tipo_mov)) NOT IN ('D', 'T', 'X', 'E', 'C', 'O', 'W', 'F')
					 AND F.motivo_fal IN ('007', '009')
					 AND 1 = CASE @CEmp
												WHEN '' THEN 1
												ELSE CASE @CEmp
														WHEN @CEmp THEN CASE 
																								 WHEN A.clave_emp = @CEmp THEN 1
																								 ELSE 0
																						END
														 END
									 END

	--INSERTA FALTA INJUSTIFICADA EN MARCAJES SEMANALES DE EMPLEADO
	 UPDATE ASIS_DIA_PERM
			SET tipo_mov = 'A'
					, observa = 'FALTA INJUSTIFICADA'
					, mot_05 = '1'		
					, horas_lab = 0.00
					, horas_ext = 0.00
					--LIMPIA LOS DEMAS MOTIVOS PARA EVITAR SE QUEDEN DUPLICADOS AL INSERTAR  EL MOTIVO DE FALTA INJUSTIFICADA  : DM 2018/AGO/10
					, mot_01 = '', mot_02 = '', mot_03 = '', mot_04 = '', mot_06 = '', mot_07 = '', mot_09 = '', mot_010 = ''
					, mot_011 = '', mot_012 = '', mot_013 = '', mot_016 = '', mot_017 = '', mot_018 = '', mot_019 = '', mot_020 = ''
					, mot_023 = '', mot_101 = '', mot_024 = '', mot_025 = '', mot_026 = '', mot_027 = '', mot_028 = ''
		 FROM ASIS_DIA_PERM AS A
LEFT JOIN vistaobtieneHorario AS V ON A.horario = V.horario
		WHERE A.s_fiscal = @pniWSFiscal
					AND A.a_fiscal = @pniWAFiscal
					AND fecha_mov <= CONVERT(DATE, GETDATE())
					AND horas_nor > 0
					AND ( CONVERT(INT, clave_emp) < 90000 OR CONVERT(INT, clave_emp) >= 95000 )
					AND RTRIM(LTRIM(hora_ent)) = '' AND RTRIM(LTRIM(hora_sal)) = ''
					AND RTRIM(LTRIM(hora_ent2)) = '' AND RTRIM(LTRIM(hora_sal2)) = ''
					AND RTRIM(LTRIM(hora_ent3)) = '' AND RTRIM(LTRIM(hora_sal3)) = ''
					AND RTRIM(LTRIM(hora_ent4)) = '' AND RTRIM(LTRIM(hora_sal4)) = ''
					AND RTRIM(LTRIM(tipo_mov)) IN ('N', 'A')
					AND RTRIM(LTRIM(mot_07)) <> '1'
					AND 1 = CASE 
											 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('MONDAY', 'LUNES') THEN
														CASE 
																 WHEN lunesEntrada <> - 1 THEN 1
																 ELSE 0
														END
											 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('TUESDAY', 'MARTES') THEN
														CASE 
																 WHEN martesEntrada <> - 1 THEN 1
																 ELSE 0
														END
											 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('WEDNESDAY', 'MIÉRCOLES') THEN
														CASE 
																 WHEN miercolesEntrada <> - 1 THEN 1
																 ELSE 0
														END
											 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('THURSDAY', 'JUEVES') THEN
														CASE 
																 WHEN juevesEntrada <> - 1 THEN 1
																 ELSE 0
														END
											 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('FRIDAY', 'VIERNES') THEN
														CASE 
																 WHEN viernesEntrada <> - 1 THEN 1
																 ELSE 0
														END
											 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('SATURDAY', 'SÁBADO') THEN
														CASE 
																 WHEN sabadoEntrada <> - 1 THEN 1
																 ELSE 0
														END
											 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('SUNDAY', 'DOMINGO') THEN
														CASE 
																 WHEN domingoEntrada <> - 1 THEN 1
																 ELSE 0
														END
									END
					AND 1 = CASE @CEmp
											 WHEN '' THEN 1
											 ELSE CASE @CEmp
																 WHEN @CEmp THEN CASE 
																											WHEN A.clave_emp = @CEmp THEN 1
																											ELSE 0
																								 END
														END
									END

	DECLARE @MinutosEntr DECIMAL(15, 12)
	DECLARE @HoraFinal DECIMAL(15, 12)
	DECLARE @MinutosTolerancia DECIMAL(15, 12)
	DECLARE @Tiempo DATETIME = GETDATE()

	SET @MinutosEntr = DATEPART(MINUTE, @Tiempo) * 1.66667
	SET @MinutosEntr = @MinutosEntr * 1.66667
	SET @MinutosEntr = @MinutosEntr / 100.00
	SET @HoraFinal = DATEPART(HOUR, @Tiempo) + @MinutosEntr
	SET @MinutosTolerancia = (5 * 1.66667) / 100.00

	--INSERTA MOVIMIENTO AUN NO ENTRA PARA ESE MISMO DIA            
	 UPDATE ASIS_DIA_PERM
			SET tipo_mov = 'N'
					, observa = 'AUN NO ENTRA'
					, mot_05 = ''
		 FROM ASIS_DIA_PERM AS A
LEFT JOIN vistaobtieneHorario AS V ON A.horario = V.horario
		WHERE A.s_fiscal = @pniWSFiscal
			AND A.a_fiscal = @pniWAFiscal
			AND fecha_mov = CONVERT(DATE, GETDATE())
			AND horas_nor > 0
			AND ( CONVERT(INT, clave_emp) < 90000 OR CONVERT(INT, clave_emp) >= 95000 )
			AND RTRIM(LTRIM(hora_ent)) = '' AND RTRIM(LTRIM(hora_sal)) = ''
			AND RTRIM(LTRIM(hora_ent2)) = '' AND RTRIM(LTRIM(hora_sal2)) = ''
			AND RTRIM(LTRIM(hora_ent3)) = '' AND RTRIM(LTRIM(hora_sal3)) = ''
			AND RTRIM(LTRIM(hora_ent4)) = '' AND RTRIM(LTRIM(hora_sal4)) = ''
			AND RTRIM(LTRIM(tipo_mov)) IN ('N', 'A')
			AND RTRIM(LTRIM(mot_07)) <> '1'
			AND 1 = CASE 
									 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('MONDAY', 'LUNES') THEN 
												CASE 
														 WHEN (@HoraFinal - lunesEntrada) <= @MinutosTolerancia THEN 1
														 ELSE 0
												END
									 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('TUESDAY', 'MARTES') THEN
												CASE 
														 WHEN (@HoraFinal - martesEntrada) <= @MinutosTolerancia THEN 1
														 ELSE 0
												END
									 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('WEDNESDAY', 'MIÉRCOLES') THEN
												CASE 
														 WHEN (@HoraFinal - miercolesEntrada) <= @MinutosTolerancia THEN 1
														 ELSE 0
												END
									 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('THURSDAY', 'JUEVES') THEN
												CASE 
														 WHEN (@HoraFinal - juevesEntrada) <= @MinutosTolerancia THEN 1
														 ELSE 0
												END
									 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('FRIDAY', 'VIERNES') THEN
												CASE 
														 WHEN (@HoraFinal - viernesEntrada) <= @MinutosTolerancia THEN 1
														 ELSE 0
												END
									 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('SATURDAY', 'SÁBADO') THEN
												CASE 
														 WHEN (@HoraFinal - sabadoEntrada) <= @MinutosTolerancia THEN 1
														 ELSE 0
												END
									 WHEN UPPER(DATENAME(WEEKDAY, A.fecha_mov)) IN ('SUNDAY', 'DOMINGO') THEN
												CASE 
														 WHEN (@HoraFinal - domingoEntrada) <= @MinutosTolerancia THEN 1
														 ELSE 0
												END
							END
			AND 1 = CASE @CEmp
									 WHEN '' THEN 1
									 ELSE CASE @CEmp
														 WHEN @CEmp THEN CASE 
																									WHEN A.clave_emp = @CEmp THEN 1
																									ELSE 0
																						 END
												END
							END

	--INSERTA AUSENTISMO EN FALTAS SI YA EXISTE ALGUN AUSENTIMO ESE DIA NO LO INSERTA
	INSERT INTO FALTAS (clave_emp, fecha_fal, horas_fal, semana_fal, motivo_fal, supervisor, personal, depto, categ
											, turno, sdo_integ, grado, cve_pago, bimestre, activo, captura, tipo_capt, referencia, laborable
											, a_fiscal, cont_div)
	 SELECT A.clave_emp, A.fecha_mov, A.horas_nor, @pniWSFiscal, '005', A.superv, '00000', A.depto, A.categoria
					, A.turno, A.sdo_integ, A.grado_niv, A.cve_pago, @Bimestre, 1, GETDATE(), 'A', 'SISTEMA', 1
					, A.a_fiscal, A.division
		 FROM ASIS_DIA_PERM AS A
LEFT JOIN FALTAS AS F ON A.clave_emp = F.clave_emp AND A.fecha_mov = F.fecha_fal
		WHERE A.s_fiscal = @pniWSFiscal
					AND A.a_fiscal = @pniWAFiscal
					AND fecha_mov BETWEEN (
							SELECT TOP 1 MIN(fecha_mov)
								FROM ASIS_DIA_PERM
							 WHERE s_fiscal = @pniWSFiscal
										 AND a_fiscal = @pniWAFiscal
							)
							AND (
							SELECT TOP 1 MAX(fecha_mov)
								FROM ASIS_DIA_PERM
							 WHERE s_fiscal = @pniWSFiscal
										 AND a_fiscal = @pniWAFiscal
							)
					AND tipo_mov = 'A'
					AND A.horas_nor > 0
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

	--LIMPIA : DE ADP HORAS ENTRADA SALIDA
	UPDATE asis_dia_perm
		 SET hora_ent = ''
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND RTRIM(LTRIM(hora_ent)) = ':'

	UPDATE asis_dia_perm
		 SET hora_sal = ''
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND RTRIM(LTRIM(hora_sal)) = ':'

	UPDATE asis_dia_perm
		 SET hora_ent2 = ''
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND RTRIM(LTRIM(hora_ent2)) = ':'

	UPDATE asis_dia_perm
		 SET hora_sal2 = ''
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND RTRIM(LTRIM(hora_sal2)) = ':'

	--LIMPIA MOTIVOS  PASE LISTA X/5
	UPDATE asis_dia_perm
		 SET mot_01 = '', mot_02 = '', mot_03 = '', mot_04 = '', mot_05 = '', mot_06 = '', mot_07 = '', mot_09 = '', mot_010 = ''
				 , mot_011 = '', mot_012 = '', mot_013 = '', mot_016 = '', mot_017 = '', mot_018 = '', mot_019 = '', mot_020 = ''
				 , mot_023 = '', mot_101 = '', fuera_turn = 0, observa = '', hrs_ext_au = 0.00, tesem = 0.00, tedesc = 0.00
				 --Modificacion DM Creacion : 2017/07/19 Produccion : N/A Anexo motivos combinados , permiso - 4 Hrs
				 , mot_024 = '', MOT_025 = '', mot_026 = '', mot_027 = '', mot_028 = ''
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND horas_nor = 0
				 AND LEN(RTRIM(LTRIM(hora_ent))) < 5 AND LEN(RTRIM(LTRIM(hora_sal))) < 5
				 AND LEN(RTRIM(LTRIM(hora_ent2))) < 5 AND LEN(RTRIM(LTRIM(hora_sal2))) < 5
				 AND LEN(RTRIM(LTRIM(hora_ent3))) < 5 AND LEN(RTRIM(LTRIM(hora_sal3))) < 5
				 AND LEN(RTRIM(LTRIM(hora_ent4))) < 5 AND LEN(RTRIM(LTRIM(hora_sal4))) < 5
				 AND RTRIM(LTRIM(tipo_mov)) in ('N', 'E')
				 AND CONVERT(INT, clave_emp) < 90000
				 AND 1 = CASE @CEmp
											WHEN '' THEN 1
											ELSE CASE @CEmp
																WHEN @CEmp THEN CASE 
																										 WHEN clave_emp = @CEmp THEN 1
																										 ELSE 0
																								END
													 END
								 END

	--LIMPIA MOTIVOS  PASE LISTA 4/5
	UPDATE asis_dia_perm
		 SET mot_01 = '', mot_02 = '', mot_03 = '', mot_04 = '', mot_05 = '', mot_06 = '', mot_07 = '', mot_09 = '', mot_010 = ''
				 , mot_011 = '', mot_012 = '', mot_013 = '', mot_016 = '', mot_017 = '', mot_018 = '', mot_019 = '', mot_020 = ''
				 , mot_023 = '', mot_101 = '', fuera_turn = 0, observa = '', hrs_ext_au = 0.00, tesem = 0.00, tedesc = 0.00
				 --Modificacion DM Creacion : 2017/07/19 Produccion : N/A Anexo motivos combinados , permiso - 4 Hrs
				 , mot_024 = '', MOT_025 = '', mot_026 = '', mot_027 = '', mot_028 = ''
	 WHERE s_fiscal = @pniWSFiscal
				 AND a_fiscal = @pniWAFiscal
				 AND RTRIM(LTRIM(tipo_mov)) NOT IN ('D', 'T', 'X', 'E', 'Z', 'C', 'O', 'W')
				 AND (
							 (
								 LEN(RTRIM(LTRIM(HORA_ENT))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL))) = 5
								 AND LEN(LTRIM(RTRIM(HORA_ENT2))) < 2 AND LEN(RTRIM(LTRIM(HORA_SAL2))) < 2
								 AND LEN(LTRIM(RTRIM(HORA_ENT3))) < 2 AND LEN(RTRIM(LTRIM(HORA_SAL3))) < 2
								 AND LEN(RTRIM(LTRIM(HORA_ENT4))) < 2 AND LEN(RTRIM(LTRIM(HORA_SAL4))) < 2
							 )
							 OR
							 (
								 LEN(RTRIM(LTRIM(HORA_ENT))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL))) = 5
								 AND LEN(RTRIM(LTRIM(HORA_ENT2))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL2))) = 5
								 AND LEN(RTRIM(LTRIM(HORA_ENT3))) < 2 AND LEN(RTRIM(LTRIM(HORA_SAL3))) < 2
								 AND LEN(RTRIM(LTRIM(HORA_ENT4))) < 2 AND LEN(RTRIM(LTRIM(HORA_SAL4))) < 2
							 )
							 OR
							 (
								 LEN(RTRIM(LTRIM(HORA_ENT))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL))) = 5
								 AND LEN(RTRIM(LTRIM(HORA_ENT2))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL2))) = 5
								 AND LEN(RTRIM(LTRIM(HORA_ENT3))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL3))) = 5
								 AND LEN(RTRIM(LTRIM(HORA_ENT4))) < 2 AND LEN(RTRIM(LTRIM(HORA_SAL4))) < 2
							 )
							 OR
							 (
								 LEN(RTRIM(LTRIM(HORA_ENT))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL))) = 5
								 AND LEN(RTRIM(LTRIM(HORA_ENT2))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL2))) = 5
								 AND LEN(RTRIM(LTRIM(HORA_ENT3))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL3))) = 5
								 AND LEN(RTRIM(LTRIM(HORA_ENT4))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL4))) = 5
							 )
						 )
						 AND 1 = CASE @CEmp
													WHEN '' THEN 1
													ELSE CASE @CEmp
																		WHEN @CEmp THEN CASE 
																												 WHEN clave_emp = @CEmp THEN 1
																												 ELSE 0
																										END
															 END
										 END

	--ACTUALIZA TIPO MOVIMIENTO PASE LISTA 4/5
 UPDATE asis_dia_perm
		SET tipo_mov = 'N'
	WHERE s_fiscal = @pniWSFiscal
				AND a_fiscal = @pniWAFiscal
				AND 1 = CASE @CEmp
										 WHEN '' THEN 1
										 ELSE CASE @CEmp
															 WHEN @CEmp THEN CASE 
																										WHEN clave_emp = @CEmp THEN 1
																										ELSE 0
																							 END
													END
								END
				AND RTRIM(LTRIM(tipo_mov)) NOT IN ('D', 'T','X', 'E', 'Z', 'C', 'O', 'W')
				AND (
							(
								LEN(RTRIM(LTRIM(HORA_ENT))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL))) = 5
								AND LEN(LTRIM(RTRIM(HORA_ENT2))) < 2 AND LEN(RTRIM(LTRIM(HORA_SAL2))) < 2
								AND LEN(LTRIM(RTRIM(HORA_ENT3))) < 2 AND LEN(RTRIM(LTRIM(HORA_SAL3))) < 2
								AND LEN(RTRIM(LTRIM(HORA_ENT4))) < 2 AND LEN(RTRIM(LTRIM(HORA_SAL4))) < 2
							)
							OR 
							(
								LEN(RTRIM(LTRIM(HORA_ENT))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL))) = 5
								AND LEN(RTRIM(LTRIM(HORA_ENT2))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL2))) = 5
								AND LEN(RTRIM(LTRIM(HORA_ENT3))) < 2 AND LEN(RTRIM(LTRIM(HORA_SAL3))) < 2
								AND LEN(RTRIM(LTRIM(HORA_ENT4))) < 2 AND LEN(RTRIM(LTRIM(HORA_SAL4))) < 2
							)
							OR
							(
								LEN(RTRIM(LTRIM(HORA_ENT))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL))) = 5
								AND LEN(RTRIM(LTRIM(HORA_ENT2))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL2))) = 5
								AND LEN(RTRIM(LTRIM(HORA_ENT3))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL3))) = 5
								AND LEN(RTRIM(LTRIM(HORA_ENT4))) < 2 AND LEN(RTRIM(LTRIM(HORA_SAL4))) < 2
							)
							OR
							(
								LEN(RTRIM(LTRIM(HORA_ENT))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL))) = 5
								AND LEN(RTRIM(LTRIM(HORA_ENT2))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL2))) = 5
								AND LEN(RTRIM(LTRIM(HORA_ENT3))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL3))) = 5
								AND LEN(RTRIM(LTRIM(HORA_ENT4))) = 5 AND LEN(RTRIM(LTRIM(HORA_SAL4))) = 5
							)
						)
END
