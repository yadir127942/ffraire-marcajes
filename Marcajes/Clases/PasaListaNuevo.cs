using System;
using System.Data.SqlClient;
using System.Data;
using static Marcajes.Clases.EnumsComunes;

namespace Marcajes.Clases
{
	class PasaListaNuevo
	{

		#region Variables

		int pniWSFiscal, pniWAFiscal, pniWBimestre;
		SqlConnection conexion;
		string psClaveEmp;

		#endregion

		#region [NUEVOS METODOS PARA COMBINACION MOTIVOS]

		private static void paselistaPermisosretardos(int pniWSFiscal, int pniWAFiscal, string psClaveEmp, ref SqlConnection conexion)
		{

			SqlCommand cmdTabla = new SqlCommand();
			cmdTabla.Connection = conexion;
			cmdTabla.CommandText = " SELECT id_adp ";
			cmdTabla.CommandText += " , A.clave_emp ";
			cmdTabla.CommandText += " ,fecha_mov , UPPER(DATENAME(DW, fecha_mov)) AS DiaSemana  ";
			cmdTabla.CommandText += " ,h.lunes	,h.martes	,h.miercoles	,h.jueves	,h.viernes	,h.sabado	,h.domingo ";
			cmdTabla.CommandText += " ,RTRIM(LTRIM(hora_ent))AS hora_ent	,RTRIM(LTRIM(hora_sal)) AS hora_sal ";
			cmdTabla.CommandText += " ,RTRIM(LTRIM(hora_ent2))AS hora_ent2	,RTRIM(LTRIM(hora_sal2)) AS hora_sal2 ";
			cmdTabla.CommandText += " ,RTRIM(LTRIM(hora_ent3))AS hora_ent3	,RTRIM(LTRIM(hora_sal3)) AS hora_sal3 ";
			cmdTabla.CommandText += " ,RTRIM(LTRIM(hora_ent4))AS hora_ent4	,RTRIM(LTRIM(hora_sal4)) AS hora_sal4 ";
			cmdTabla.CommandText += " ,mot_010	,mot_101	,observa ";
			cmdTabla.CommandText += " ,F.id_Faltas	,F.fecha_fal	,F.clave_emp	,F.motivo_fal ";
			cmdTabla.CommandText += " FROM ASIS_DIA_PERM AS A ";
			cmdTabla.CommandText += " LEFT JOIN HORARIOS AS H ON A.horario = H.horario ";
			cmdTabla.CommandText += " LEFT JOIN vistaobtieneHorario AS V ON A.HORARIO = V.horario ";
			cmdTabla.CommandText += " LEFT JOIN FALTAS AS F ON A.clave_emp = F.clave_emp ";
			cmdTabla.CommandText += "     AND A.fecha_mov = F.fecha_fal ";
			cmdTabla.CommandText += " WHERE A.s_fiscal = @SFiscal ";
			cmdTabla.CommandText += "     AND A.a_fiscal = @AFiscal ";
			cmdTabla.CommandText += "     AND( ";
			cmdTabla.CommandText += "         mot_010 = '1' ";
			cmdTabla.CommandText += "         OR mot_101 = '1' ";
			cmdTabla.CommandText += "         ) ";
			if (!string.IsNullOrEmpty(psClaveEmp.Trim()))
			{
				cmdTabla.CommandText += " AND A.clave_emp = @psClaveEmp";
			}
			cmdTabla.CommandText += " ORDER BY  A.clave_emp ,A.fecha_mov";
			cmdTabla.Parameters.Add("@psClaveEmp", SqlDbType.NVarChar).Value = psClaveEmp;
			cmdTabla.Parameters.Add("@SFiscal", SqlDbType.Int).Value = pniWSFiscal;
			cmdTabla.Parameters.Add("@AFiscal", SqlDbType.Int).Value = pniWAFiscal;


			DataTable tabCombinacionMotivos = new DataTable();
			SqlDataAdapter daCombinacionMotivos = new SqlDataAdapter();
			daCombinacionMotivos.SelectCommand = cmdTabla;
			daCombinacionMotivos.Fill(tabCombinacionMotivos);

			TimeSpan MarcajeEntradaHorario;
			TimeSpan MarcajeSalidaHorario;
			TimeSpan MarcajeEntrada;
			TimeSpan MarcajeSalida;
			DateTime marcajeEntradaCompleto;
			DateTime marcajeSalidaCompleto;
			DateTime marcajeEntradaCompleto2;
			DateTime marcajeSalidaCompleto2;
			DateTime marcajeEntradaCompleto3;
			DateTime marcajeSalidaCompleto3;
			//DateTime marcajeEntradaCompleto4;
			//DateTime marcajeSalidaCompleto4;
			DateTime marcajeEntradaHorarioCompleto;
			DateTime marcajeSalidaHorarioCompleto;

			TimeSpan valorMaximo = TimeSpan.MaxValue;
			string entradaHorario, salidaHorario, horaEntrada, horaSalida;
			decimal tiempoPermiso = 0.0M;
			decimal tiempoAuxiliar = 0.0M;

			#region Solo un par de marcajes
			foreach (DataRow registro in tabCombinacionMotivos.Select("LEN(hora_ent) = 5 AND LEN(HORA_SAL) = 5 AND hora_ent2 = '' AND hora_sal2= '' AND hora_ent3 = '' and hora_sal3 = '' and hora_ent4 ='' and hora_sal4 = '' ", "clave_emp , fecha_mov"))
			{
				TipoMotivos motivoAnterior = TipoMotivos.SinIncidencia;

				if (registro["mot_010"].ToString().Trim() == "1")
				{
					motivoAnterior = TipoMotivos.Retardo5Min;
				}
				else { motivoAnterior = TipoMotivos.RetardoMas4Horas; }


				#region [OBTIENE ENTRADA SALIDA DE HORARIO]
				entradaHorario = "";
				salidaHorario = "";
				switch (registro["diaSemana"].ToString())
				{
					case "LUNES":
					case "MONDAY":
						entradaHorario = registro["lunes"].ToString().Trim();
						salidaHorario = registro["lunes"].ToString().Trim();

						break;
					case "MARTES":
					case "TUESDAY":
						entradaHorario = registro["martes"].ToString().Trim();
						salidaHorario = registro["martes"].ToString().Trim();

						break;
					case "MIERCOLES":
					case "MIÉRCOLES":
					case "WEDNESDAY":
						entradaHorario = registro["miercoles"].ToString().Trim();
						salidaHorario = registro["miercoles"].ToString().Trim();

						break;
					case "JUEVES":
					case "THURSDAY":
						entradaHorario = registro["jueves"].ToString().Trim();
						salidaHorario = registro["jueves"].ToString().Trim();

						break;
					case "VIERNES":
					case "FRIDAY":
						entradaHorario = registro["viernes"].ToString().Trim();
						salidaHorario = registro["viernes"].ToString().Trim();

						break;
					case "SABADO":
					case "SÁBADO":
					case "SATURDAY":
						entradaHorario = registro["sabado"].ToString().Trim();
						salidaHorario = registro["sabado"].ToString().Trim();

						break;
					case "DOMINGO":
					case "SUNDAY":
						entradaHorario = registro["domingo"].ToString().Trim();
						salidaHorario = registro["domingo"].ToString().Trim();

						break;
				}
				#endregion

				horaEntrada = registro["hora_ent"].ToString();
				horaSalida = registro["hora_sal"].ToString();

				MarcajeEntradaHorario = entradaHorario != "" && entradaHorario != "?" && entradaHorario != ":" ? TimeSpan.Parse(entradaHorario.Substring(0, 5)) : valorMaximo;
				MarcajeSalidaHorario = salidaHorario != "" && salidaHorario != "?" && salidaHorario != ":" ? TimeSpan.Parse(salidaHorario.Substring(5, 5)) : valorMaximo;

				MarcajeEntrada = horaEntrada != "" && horaEntrada != "?" && horaEntrada != ":" ? TimeSpan.Parse(horaEntrada) : valorMaximo;
				MarcajeSalida = horaEntrada != "" && horaEntrada != "?" && horaSalida != ":" ? TimeSpan.Parse(horaSalida) : valorMaximo;

				marcajeEntradaCompleto = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeEntrada);
				marcajeSalidaCompleto = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeSalida);

				marcajeEntradaHorarioCompleto = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeEntradaHorario);
				marcajeSalidaHorarioCompleto = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeSalidaHorario);

				//agrego dia para horarios que cambian de dia
				switch (marcajeSalidaHorarioCompleto.CompareTo(marcajeEntradaHorarioCompleto))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalidaHorarioCompleto = marcajeSalidaHorarioCompleto.AddDays(1);
						break;
				}

				switch (marcajeEntradaCompleto.CompareTo(marcajeEntradaHorarioCompleto))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeEntradaCompleto = marcajeEntradaCompleto.AddDays(1);
						marcajeSalidaCompleto = marcajeSalidaCompleto.AddDays(1);
						break;
				}

				//agrego dia a salida cuando la salida es menor a entrada 
				switch (marcajeSalidaCompleto.CompareTo(marcajeEntradaCompleto))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalidaCompleto = marcajeSalidaCompleto.AddDays(1);
						break;
				}

				TipoMotivos tipoIncidecia = TipoMotivos.SinIncidencia;
				tiempoPermiso = 0.0M;
				if (marcajeSalidaCompleto < marcajeSalidaHorarioCompleto)
				{
					tiempoPermiso = CalculoHorasParesMarcajes(marcajeSalidaCompleto, marcajeSalidaHorarioCompleto);

					if (tiempoPermiso >= 4)
					{
						tipoIncidecia = TipoMotivos.PermisoParcialMayor4Horas;
					}
					else if (tiempoPermiso > 0.50M && tiempoPermiso < 4)
					//else if (tiempoPermiso >= 0.10M && tiempoPermiso < 4)  //Cambio Peticion Glinka , permisos menores de 4 Horas Aplican para un permiso de 6 minutos en delante
					{
						tipoIncidecia = TipoMotivos.PermisoParcialMenor4Horas;
					}
				}

				ActualizaMotivoCombinacion(conexion, registro, motivoAnterior, tipoIncidecia);
			}
			#endregion

			#region DOS PARES DE MARCAJES
			foreach (DataRow registro in tabCombinacionMotivos.Select("LEN(hora_ent) = 5 AND LEN(HORA_SAL) = 5 AND LEN(hora_ent2) = 5 AND LEN(hora_sal2) = 5 AND hora_ent3 = '' and hora_sal3 = '' and hora_ent4 ='' and hora_sal4 = '' "))
			{

				TipoMotivos motivoAnterior = TipoMotivos.SinIncidencia;

				if (registro["mot_010"].ToString().Trim() == "1")
				{
					motivoAnterior = TipoMotivos.Retardo5Min;
				}
				else { motivoAnterior = TipoMotivos.RetardoMas4Horas; }


				#region [OBTIENE ENTRADA SALIDA DE HORARIO]
				entradaHorario = "";
				salidaHorario = "";
				switch (registro["diaSemana"].ToString())
				{
					case "LUNES":
					case "MONDAY":
						entradaHorario = registro["lunes"].ToString().Trim();
						salidaHorario = registro["lunes"].ToString().Trim();

						break;
					case "MARTES":
					case "TUESDAY":
						entradaHorario = registro["martes"].ToString().Trim();
						salidaHorario = registro["martes"].ToString().Trim();

						break;
					case "MIERCOLES":
					case "MIÉRCOLES":
					case "WEDNESDAY":
						entradaHorario = registro["miercoles"].ToString().Trim();
						salidaHorario = registro["miercoles"].ToString().Trim();

						break;
					case "JUEVES":
					case "THURSDAY":
						entradaHorario = registro["jueves"].ToString().Trim();
						salidaHorario = registro["jueves"].ToString().Trim();

						break;
					case "VIERNES":
					case "FRIDAY":
						entradaHorario = registro["viernes"].ToString().Trim();
						salidaHorario = registro["viernes"].ToString().Trim();

						break;
					case "SABADO":
					case "SÁBADO":
					case "SATURDAY":
						entradaHorario = registro["sabado"].ToString().Trim();
						salidaHorario = registro["sabado"].ToString().Trim();

						break;
					case "DOMINGO":
					case "SUNDAY":
						entradaHorario = registro["domingo"].ToString().Trim();
						salidaHorario = registro["domingo"].ToString().Trim();

						break;
				}
				#endregion

				MarcajeEntradaHorario = entradaHorario != "" && entradaHorario != "?" && entradaHorario != ":" ? TimeSpan.Parse(entradaHorario.Substring(0, 5)) : valorMaximo;
				MarcajeSalidaHorario = salidaHorario != "" && salidaHorario != "?" && salidaHorario != ":" ? TimeSpan.Parse(salidaHorario.Substring(5, 5)) : valorMaximo;

				marcajeEntradaHorarioCompleto = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeEntradaHorario);
				marcajeSalidaHorarioCompleto = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeSalidaHorario);

				horaEntrada = registro["hora_ent"].ToString();
				horaSalida = registro["hora_sal"].ToString();

				MarcajeEntrada = horaEntrada != "" && horaEntrada != "?" && horaEntrada != ":" ? TimeSpan.Parse(horaEntrada) : valorMaximo;
				MarcajeSalida = horaEntrada != "" && horaEntrada != "?" && horaSalida != ":" ? TimeSpan.Parse(horaSalida) : valorMaximo;

				marcajeEntradaCompleto = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeEntrada);
				marcajeSalidaCompleto = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeSalida);

				horaEntrada = registro["hora_ent2"].ToString();
				horaSalida = registro["hora_sal2"].ToString();

				MarcajeEntrada = horaEntrada != "" && horaEntrada != "?" && horaEntrada != ":" ? TimeSpan.Parse(horaEntrada) : valorMaximo;
				MarcajeSalida = horaEntrada != "" && horaEntrada != "?" && horaSalida != ":" ? TimeSpan.Parse(horaSalida) : valorMaximo;

				marcajeEntradaCompleto2 = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeEntrada);
				marcajeSalidaCompleto2 = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeSalida);

				//agrego dia para horarios que cambian de dia
				switch (marcajeSalidaHorarioCompleto.CompareTo(marcajeEntradaHorarioCompleto))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalidaHorarioCompleto = marcajeSalidaHorarioCompleto.AddDays(1);
						break;
				}

				switch (marcajeEntradaCompleto.CompareTo(marcajeEntradaHorarioCompleto))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeEntradaCompleto = marcajeEntradaCompleto.AddDays(1);
						marcajeSalidaCompleto = marcajeSalidaCompleto.AddDays(1);
						break;
				}

				//agrego dia a salida cuando la salida es menor a entrada 
				switch (marcajeSalidaCompleto.CompareTo(marcajeEntradaCompleto))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalidaCompleto = marcajeSalidaCompleto.AddDays(1);
						break;
				}

				switch (marcajeEntradaCompleto2.CompareTo(marcajeEntradaHorarioCompleto))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeEntradaCompleto2 = marcajeEntradaCompleto2.AddDays(1);
						marcajeSalidaCompleto2 = marcajeSalidaCompleto2.AddDays(1);
						break;
				}

				//agrego dia a salida cuando la salida es menor a entrada 
				switch (marcajeSalidaCompleto2.CompareTo(marcajeEntradaCompleto2))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalidaCompleto2 = marcajeSalidaCompleto2.AddDays(1);
						break;
				}

				TipoMotivos tipoIncidecia = TipoMotivos.SinIncidencia;
				tiempoPermiso = 0.0M;

				if (marcajeSalidaCompleto < marcajeSalidaHorarioCompleto)
				{
					if (marcajeEntradaCompleto2 < marcajeSalidaHorarioCompleto)
					{
						//acumula tiempo entre salida y entrada de primer y segundo par de marcajes
						tiempoPermiso = CalculoHorasParesMarcajes(marcajeEntradaCompleto2, marcajeSalidaCompleto);

						if (tiempoPermiso < 0.5M)
						//if (tiempoPermiso < 0.10M)//Cambio Peticion Glinka , permisos menores de 4 Horas Aplican para un permiso de 6 minutos en delante
						{
							tiempoPermiso = 0.0M;
						}

						//aqui se acumula el tiempo de la salida  
						if (marcajeSalidaCompleto2 < marcajeSalidaHorarioCompleto)
						{
							tiempoAuxiliar = 0.0M;
							tiempoAuxiliar = CalculoHorasParesMarcajes(marcajeSalidaCompleto2, marcajeSalidaHorarioCompleto);

							if (tiempoAuxiliar < 0.5M)
							//if (tiempoAuxiliar < 0.1M)//Cambio Peticion Glinka , permisos menores de 4 Horas Aplican para un permiso de 6 minutos en delante
							{
								tiempoAuxiliar = 0.00M;
							}
							tiempoPermiso += tiempoAuxiliar;
						}

					}
					else
					{
						tiempoPermiso = CalculoHorasParesMarcajes(marcajeSalidaCompleto, marcajeSalidaHorarioCompleto);
					}

					//Aqui se compararia el laborado con el de permisos si el de permisos excede en demasia el de laborado digamos que solo fue una hora laborada se elimina permiso para dar permiso total.


					if (tiempoPermiso >= 4)
					{
						tipoIncidecia = TipoMotivos.PermisoParcialMayor4Horas;
					}
					else if (tiempoPermiso > 0.50M && tiempoPermiso < 4)
					//else if (tiempoPermiso >= 0.10M && tiempoPermiso < 4)//Cambio Peticion Glinka , permisos menores de 4 Horas Aplican para un permiso de 6 minutos en delante
					{
						tipoIncidecia = TipoMotivos.PermisoParcialMenor4Horas;
					}

					ActualizaMotivoCombinacion(conexion, registro, motivoAnterior, tipoIncidecia);

				}
			}
			#endregion

			#region TRES PARES DE  MARCAJES
			foreach (DataRow registro in tabCombinacionMotivos.Select("LEN(hora_ent) = 5 AND LEN(HORA_SAL) = 5 AND LEN(hora_ent2) = 5 AND LEN(hora_sal2) = 5 AND LEN(hora_ent3) = 5 and LEN(hora_sal3) = 5 and hora_ent4 ='' and hora_sal4 = '' "))
			{

				TipoMotivos motivoAnterior = TipoMotivos.SinIncidencia;

				if (registro["mot_010"].ToString().Trim() == "1")
				{
					motivoAnterior = TipoMotivos.Retardo5Min;
				}
				else { motivoAnterior = TipoMotivos.RetardoMas4Horas; }


				#region [OBTIENE ENTRADA SALIDA DE HORARIO]
				entradaHorario = "";
				salidaHorario = "";
				switch (registro["diaSemana"].ToString())
				{
					case "LUNES":
					case "MONDAY":
						entradaHorario = registro["lunes"].ToString().Trim();
						salidaHorario = registro["lunes"].ToString().Trim();

						break;
					case "MARTES":
					case "TUESDAY":
						entradaHorario = registro["martes"].ToString().Trim();
						salidaHorario = registro["martes"].ToString().Trim();

						break;
					case "MIERCOLES":
					case "MIÉRCOLES":
					case "WEDNESDAY":
						entradaHorario = registro["miercoles"].ToString().Trim();
						salidaHorario = registro["miercoles"].ToString().Trim();

						break;
					case "JUEVES":
					case "THURSDAY":
						entradaHorario = registro["jueves"].ToString().Trim();
						salidaHorario = registro["jueves"].ToString().Trim();

						break;
					case "VIERNES":
					case "FRIDAY":
						entradaHorario = registro["viernes"].ToString().Trim();
						salidaHorario = registro["viernes"].ToString().Trim();

						break;
					case "SABADO":
					case "SÁBADO":
					case "SATURDAY":
						entradaHorario = registro["sabado"].ToString().Trim();
						salidaHorario = registro["sabado"].ToString().Trim();

						break;
					case "DOMINGO":
					case "SUNDAY":
						entradaHorario = registro["domingo"].ToString().Trim();
						salidaHorario = registro["domingo"].ToString().Trim();

						break;
				}
				#endregion

				MarcajeEntradaHorario = entradaHorario != "" && entradaHorario != "?" && entradaHorario != ":" ? TimeSpan.Parse(entradaHorario.Substring(0, 5)) : valorMaximo;
				MarcajeSalidaHorario = salidaHorario != "" && salidaHorario != "?" && salidaHorario != ":" ? TimeSpan.Parse(salidaHorario.Substring(5, 5)) : valorMaximo;

				marcajeEntradaHorarioCompleto = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeEntradaHorario);
				marcajeSalidaHorarioCompleto = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeSalidaHorario);

				horaEntrada = registro["hora_ent"].ToString();
				horaSalida = registro["hora_sal"].ToString();

				MarcajeEntrada = horaEntrada != "" && horaEntrada != "?" && horaEntrada != ":" ? TimeSpan.Parse(horaEntrada) : valorMaximo;
				MarcajeSalida = horaEntrada != "" && horaEntrada != "?" && horaSalida != ":" ? TimeSpan.Parse(horaSalida) : valorMaximo;

				marcajeEntradaCompleto = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeEntrada);
				marcajeSalidaCompleto = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeSalida);

				horaEntrada = registro["hora_ent2"].ToString();
				horaSalida = registro["hora_sal2"].ToString();

				MarcajeEntrada = horaEntrada != "" && horaEntrada != "?" && horaEntrada != ":" ? TimeSpan.Parse(horaEntrada) : valorMaximo;
				MarcajeSalida = horaEntrada != "" && horaEntrada != "?" && horaSalida != ":" ? TimeSpan.Parse(horaSalida) : valorMaximo;

				marcajeEntradaCompleto2 = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeEntrada);
				marcajeSalidaCompleto2 = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeSalida);

				horaEntrada = registro["hora_ent3"].ToString();
				horaSalida = registro["hora_sal3"].ToString();

				MarcajeEntrada = horaEntrada != "" && horaEntrada != "?" && horaEntrada != ":" ? TimeSpan.Parse(horaEntrada) : valorMaximo;
				MarcajeSalida = horaEntrada != "" && horaEntrada != "?" && horaSalida != ":" ? TimeSpan.Parse(horaSalida) : valorMaximo;

				marcajeEntradaCompleto3 = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeEntrada);
				marcajeSalidaCompleto3 = Convert.ToDateTime(registro["fecha_mov"]).Add(MarcajeSalida);

				//agrego dia para horarios que cambian de dia
				switch (marcajeSalidaHorarioCompleto.CompareTo(marcajeEntradaHorarioCompleto))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalidaHorarioCompleto = marcajeSalidaHorarioCompleto.AddDays(1);
						break;
				}

				switch (marcajeEntradaCompleto.CompareTo(marcajeEntradaHorarioCompleto))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeEntradaCompleto = marcajeEntradaCompleto.AddDays(1);
						marcajeSalidaCompleto = marcajeSalidaCompleto.AddDays(1);
						break;
				}

				//agrego dia a salida cuando la salida es menor a entrada 
				switch (marcajeSalidaCompleto.CompareTo(marcajeEntradaCompleto))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalidaCompleto = marcajeSalidaCompleto.AddDays(1);
						break;
				}

				switch (marcajeEntradaCompleto2.CompareTo(marcajeEntradaHorarioCompleto))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeEntradaCompleto2 = marcajeEntradaCompleto2.AddDays(1);
						marcajeSalidaCompleto2 = marcajeSalidaCompleto2.AddDays(1);
						break;
				}

				//agrego dia a salida cuando la salida es menor a entrada 
				switch (marcajeSalidaCompleto2.CompareTo(marcajeEntradaCompleto2))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalidaCompleto2 = marcajeSalidaCompleto2.AddDays(1);
						break;
				}

				switch (marcajeEntradaCompleto3.CompareTo(marcajeEntradaHorarioCompleto))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeEntradaCompleto3 = marcajeEntradaCompleto3.AddDays(1);
						marcajeSalidaCompleto3 = marcajeSalidaCompleto3.AddDays(1);
						break;
				}

				//agrego dia a salida cuando la salida es menor a entrada 
				switch (marcajeSalidaCompleto3.CompareTo(marcajeEntradaCompleto3))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalidaCompleto3 = marcajeSalidaCompleto3.AddDays(1);
						break;
				}

				TipoMotivos tipoIncidecia = TipoMotivos.SinIncidencia;
				tiempoPermiso = 0.0M;

				if (marcajeSalidaCompleto < marcajeSalidaHorarioCompleto)
				{
					if (marcajeEntradaCompleto2 < marcajeSalidaHorarioCompleto)
					{
						//acumula tiempo entre salida 1er par marcajes y entrada 2do par de marcajes
						tiempoPermiso = CalculoHorasParesMarcajes(marcajeEntradaCompleto2, marcajeSalidaCompleto);

						if (tiempoPermiso < 0.5M) //aqui esta el condicional para sumar este tiempo solo si es mayor a cierta cantidad , ejemplo si abandono solo .5 media hora no se contaria 
						{
							tiempoPermiso = 0.0M;
						}

						if (marcajeSalidaCompleto2 < marcajeSalidaHorarioCompleto)
						{
							if (marcajeEntradaCompleto3 < marcajeSalidaHorarioCompleto)
							{
								tiempoAuxiliar = 0.0M;
								tiempoAuxiliar = CalculoHorasParesMarcajes(marcajeSalidaCompleto2, marcajeEntradaCompleto3);

								if (tiempoAuxiliar < 0.5M)
								//if (tiempoAuxiliar < 0.1M)//Cambio Peticion Glinka , permisos menores de 4 Horas Aplican para un permiso de 6 minutos en delante
								{
									tiempoAuxiliar = 0.00M;
								}
								tiempoPermiso += tiempoAuxiliar;

								if (marcajeSalidaCompleto3 < marcajeSalidaHorarioCompleto)
								{
									//aqui se acumula el tiempo de la salida  
									tiempoAuxiliar = 0.0M;
									tiempoAuxiliar = CalculoHorasParesMarcajes(marcajeSalidaCompleto3, marcajeSalidaHorarioCompleto);

									if (tiempoAuxiliar < 0.5M)
									//if (tiempoAuxiliar < 0.1M)//Cambio Peticion Glinka , permisos menores de 4 Horas Aplican para un permiso de 6 minutos en delante
									{
										tiempoAuxiliar = 0.00M;
									}
									tiempoPermiso += tiempoAuxiliar;
								}
							}
							else
							{
								tiempoAuxiliar = CalculoHorasParesMarcajes(marcajeSalidaCompleto2, marcajeSalidaHorarioCompleto);

								if (tiempoAuxiliar < .5M)
								//if (tiempoAuxiliar < .1M)//Cambio Peticion Glinka , permisos menores de 4 Horas Aplican para un permiso de 6 minutos en delante
								{
									tiempoAuxiliar = 0.0M;
								}

								tiempoPermiso += tiempoAuxiliar;
							}
						}
					}
					else
					{
						tiempoPermiso = CalculoHorasParesMarcajes(marcajeSalidaCompleto, marcajeSalidaHorarioCompleto);
					}

					//Aqui se compararia el laborado con el de permisos si el de permisos excede en demasia el de laborado digamos que solo fue una hora laborada se elimina permiso para dar permiso total.


					if (tiempoPermiso >= 4)
					{
						tipoIncidecia = TipoMotivos.PermisoParcialMayor4Horas;
					}
					else if (tiempoPermiso > 0.50M && tiempoPermiso < 4)
					//else if (tiempoPermiso >= 0.10M && tiempoPermiso < 4)//Cambio Peticion Glinka , permisos menores de 4 Horas Aplican para un permiso de 6 minutos en delante
					{
						tipoIncidecia = TipoMotivos.PermisoParcialMenor4Horas;
					}

					ActualizaMotivoCombinacion(conexion, registro, motivoAnterior, tipoIncidecia);

				}
			}
			#endregion

		}

		private static void ActualizaMotivoCombinacion(SqlConnection conexion, DataRow registro, TipoMotivos motivoAnterior, TipoMotivos tipoIncidecia)
		{
			SqlCommand cmdActualizaMotivos = new SqlCommand();
			switch (tipoIncidecia)
			{
				//case TipoMotivos.SinIncidencia:
				//    break;
				//case TipoMotivos.Retardo5Min:
				//    break;                    
				//case TipoMotivos.RetardoMas4Horas:
				//    break;
				//default:
				//    break;
				case TipoMotivos.PermisoParcialMayor4Horas:

					cmdActualizaMotivos.Connection = conexion;
					cmdActualizaMotivos.Parameters.Clear();
					cmdActualizaMotivos.CommandText = " UPDATE asis_dia_perm SET  mot_101 = '' , mot_010 = ''  ";//limpia motivo antetior
					cmdActualizaMotivos.CommandText += string.Format(", {0} ", motivoAnterior == TipoMotivos.Retardo5Min ? "mot_025 = '1' , observa = 'RET +5MINS , PP +4HRS' " : "mot_026 = '1' , observa = 'RET +4HRS , PP +4HRS' ");
					cmdActualizaMotivos.CommandText += " WHERE id_ADP = @IdAsistencia ";
					cmdActualizaMotivos.Parameters.Add("@idAsistencia", SqlDbType.BigInt).Value = registro["id_ADP"];
					cmdActualizaMotivos.ExecuteNonQuery();

					cmdActualizaMotivos.Connection = conexion;
					cmdActualizaMotivos.Parameters.Clear();
					cmdActualizaMotivos.CommandText = " UPDATE FALTAS SET   "; //limpia motivo antetior
					cmdActualizaMotivos.CommandText += string.Format(" motivo_fal = '{0}' ", motivoAnterior == TipoMotivos.Retardo5Min ? "025" : "026");
					cmdActualizaMotivos.CommandText += " WHERE id_faltas = @IdFalta ";
					cmdActualizaMotivos.Parameters.Add("@idFalta", SqlDbType.BigInt).Value = registro["id_faltas"];
					cmdActualizaMotivos.ExecuteNonQuery();

					break;
				case TipoMotivos.PermisoParcialMenor4Horas:

					cmdActualizaMotivos.Connection = conexion;
					cmdActualizaMotivos.Parameters.Clear();
					cmdActualizaMotivos.CommandText = " UPDATE asis_dia_perm SET  mot_101 = '' , mot_010 = ''  "; //limpia motivo antetior
					cmdActualizaMotivos.CommandText += string.Format(", {0} ", motivoAnterior == TipoMotivos.Retardo5Min ? "mot_027 = '1' , observa = 'RET +5MINS , PP -4HRS' " : "mot_028 = '1' , observa = 'RET +4HRS , PP -4HRS' ");
					cmdActualizaMotivos.CommandText += " WHERE id_ADP = @IdAsistencia ";
					cmdActualizaMotivos.Parameters.Add("@idAsistencia", SqlDbType.BigInt).Value = registro["id_ADP"];
					cmdActualizaMotivos.ExecuteNonQuery();


					cmdActualizaMotivos.Connection = conexion;
					cmdActualizaMotivos.Parameters.Clear();
					cmdActualizaMotivos.CommandText = " UPDATE FALTAS SET    "; //limpia motivo antetior
					cmdActualizaMotivos.CommandText += string.Format(" motivo_fal = '{0}' ", motivoAnterior == TipoMotivos.Retardo5Min ? "027" : "028");
					cmdActualizaMotivos.CommandText += " WHERE id_faltas = @IdFalta ";
					cmdActualizaMotivos.Parameters.Add("@idFalta", SqlDbType.BigInt).Value = registro["id_faltas"];
					cmdActualizaMotivos.ExecuteNonQuery();
					break;

			}
		}

		private static decimal CalculoHorasParesMarcajes(DateTime MarcajeEntrada, DateTime MarcajeSalida)
		{
			decimal tiempo = 0.00M;
			TimeSpan duracion;

			duracion = MarcajeSalida.Subtract(MarcajeEntrada).Duration();
			tiempo = TiempoDetalle(duracion);

			return tiempo;
		}

		public static decimal TiempoDetalle(TimeSpan Hora)
		{
			decimal Minutos, HoraDecimal;
			Minutos = Math.Round(((decimal)Hora.Minutes * 1.66667M / 100.00M), 2);
			HoraDecimal = Hora.Hours + Minutos;
			return HoraDecimal;
		}

		#endregion

		#region [ASIGNACION PERMISOS PARCIALES]

		private static DataTable creaTVPPLPermisoParcial()
		{
			DataTable TVPPLDetalleTiempos = new DataTable();
			TVPPLDetalleTiempos.Columns.Add("id_ADP", typeof(System.Int64));
			TVPPLDetalleTiempos.Columns.Add("observa", typeof(System.String));
			TVPPLDetalleTiempos.Columns.Add("mot_024", typeof(System.String));
			TVPPLDetalleTiempos.Columns.Add("mot_011", typeof(System.String));
			return TVPPLDetalleTiempos;
		}

		public static void pasarListaPermisosParciales(string psClaveEmp, int pniWSFiscal, int pniWAFiscal, ref SqlConnection conexion)
		{

			if (conexion.State == ConnectionState.Closed) { conexion.Open(); }

			SqlDataAdapter daTablas = new SqlDataAdapter();
			SqlCommand cmdTabla = new SqlCommand();
			SqlParameterCollection paramCollection = new SqlCommand().Parameters;

			DataTable tab_AsisDiaPerm = new DataTable();
			//DataTable tab_Faltas = new DataTable();
			//DataTable tab_MotFals = new DataTable();
			//DataTable tab_Configuracion = new DataTable();

			//datos nuevos de aqui en delante para pase de lista 

			//MODIFICACION ANEXO MOTIVOS COMBINADOS            


			#region [CARGA ASIS_DIA_PERM]

			#region [SELECT QUERY]

			cmdTabla = new SqlCommand();
			cmdTabla.Connection = conexion;

			cmdTabla.CommandText = "  SELECT id_ADP " +
													 "					 , A.clave_emp " +
													 "					 , CONVERT(DATETIME, A.fecha_mov) AS fecha_mov " +
													 "					 , CASE " +
													 "									WHEN UPPER(DATENAME(DW, fecha_mov)) IN('DOMINGO', 'SUNDAY') THEN domingo " +
													 "									WHEN UPPER(DATENAME(DW, fecha_mov)) IN('SABADO', 'SÁBADO', 'SATURDAY') THEN sabado " +
													 "									WHEN UPPER(DATENAME(DW, fecha_mov)) IN('VIERNES', 'FRIDAY') THEN viernes " +
													 "									WHEN UPPER(DATENAME(DW, fecha_mov)) IN('JUEVES', 'THURSDAY') THEN jueves " +
													 "									WHEN UPPER(DATENAME(DW, fecha_mov)) IN('MIERCOLES', 'MIÉRCOLES', 'WEDNESDAY') THEN miercoles " +
													 "									WHEN UPPER(DATENAME(DW, fecha_mov)) IN('MARTES', 'TUESDAY') THEN martes " +
													 "									WHEN UPPER(DATENAME(DW, fecha_mov)) IN('LUNES', 'MONDAY') THEN lunes " +
													 "						 END AS EntradaSalidaHorario " +
													 "					 , A.diasem " +
													 "					 , CASE " +
													 "									WHEN UPPER(DATENAME(DW, fecha_mov)) IN('DOMINGO', 'SUNDAY') THEN horas_dom " +
													 "									WHEN UPPER(DATENAME(DW, fecha_mov)) IN('SABADO', 'SÁBADO', 'SATURDAY') THEN horas_sab " +
													 "									WHEN UPPER(DATENAME(DW, fecha_mov)) IN('VIERNES', 'FRIDAY') THEN horas_vie " +
													 "									WHEN UPPER(DATENAME(DW, fecha_mov)) IN('JUEVES', 'THURSDAY') THEN horas_jue " +
													 "									WHEN UPPER(DATENAME(DW, fecha_mov)) IN('MIERCOLES', 'MIÉRCOLES', 'WEDNESDAY') THEN horas_mie " +
													 "									WHEN UPPER(DATENAME(DW, fecha_mov)) IN('MARTES', 'TUESDAY') THEN horas_mar " +
													 "									WHEN UPPER(DATENAME(DW, fecha_mov)) IN('LUNES', 'MONDAY') THEN horas_lun " +
													 "						 END AS HorasDia " +
													 "					 , CASE WHEN hora_ent  = '24:00' THEN '00:00' ELSE RTRIM(LTRIM(A.hora_ent))  END AS hora_ent " +
													 "					 , CASE WHEN hora_sal  = '24:00' THEN '00:00' ELSE RTRIM(LTRIM(A.hora_sal))  END AS hora_sal " +
													 "					 , CASE WHEN hora_ent2 = '24:00' THEN '00:00' ELSE RTRIM(LTRIM(A.hora_ent2)) END AS hora_ent2 " +
													 "					 , CASE WHEN hora_sal2 = '24:00' THEN '00:00' ELSE RTRIM(LTRIM(A.hora_sal2)) END AS hora_sal2 " +
													 "					 , CASE WHEN hora_ent3 = '24:00' THEN '00:00' ELSE RTRIM(LTRIM(A.hora_ent3)) END AS hora_ent3 " +
													 "					 , CASE WHEN hora_sal3 = '24:00' THEN '00:00' ELSE RTRIM(LTRIM(A.hora_sal3)) END AS hora_sal3 " +
													 "					 , CASE WHEN hora_ent4 = '24:00' THEN '00:00' ELSE RTRIM(LTRIM(A.hora_ent4)) END AS hora_ent4 " +
													 "					 , CASE WHEN hora_sal4 = '24:00' THEN '00:00' ELSE RTRIM(LTRIM(A.hora_sal4)) END AS hora_sal4 " +
													 "					 , A.horas_lab, A.horas_nor, A.horas_ext, A.hs_ext_aut, A.hrs_ext_au, A.hrs_ext2_au " +
													 "					 , A.hrs_ext3_au, A.hs_ext3_au, A.hs_ext2_au, A.depto, A.turno, A.superv, A.categoria " +
													 "					 , A.grado_niv, A.division, A.cve_pago, A.horario " +
													 "					 , A.mot_01, A.mot_02, A.mot_03, A.mot_04, A.mot_05, A.mot_06, A.mot_07, A.mot_09 " +
													 "					 , A.mot_010, A.mot_011, A.mot_012, A.mot_013, A.mot_016, A.mot_017, A.mot_018, A.mot_019 " +
													 "					 , A.mot_020, A.mot_023, A.mot_101 " +
													 "					 , A.fecha_alt, A.tipo_mov, A.tipo_proc, A.fuera_turn, A.nvohorario, A.diasem, A.imp_ext2 " +
													 "					 , A.imp_ext3, A.tesem, A.tedesc, A.textra_tot, A.textra_dob, A.textra_tri, A.sdo_integ " +
													 "					 , A.s_fiscal, A.a_fiscal, A.subido, A.observa " +
													 "					 , ISNULL(H.dias_asist, '') AS dias_asist " +
													 "					 , V2.AgregaTiempo " +
													 "					 , V2.QuitaTiempo " +
													 "					 , H.tolera_ent AS ToleranciaEntrada " +
													 "					 , H.tolera_sal AS ToleranciaSalida" +
													 "			FROM ASIS_DIA_PERM AS A " +
													 " LEFT JOIN HORARIOS AS H ON A.horario = H.horario " +
													 " LEFT JOIN vistaQuitaAgregaTiempo AS V2 ON A.HORARIO = V2.horario " +
													 "		 WHERE A.s_fiscal = @SFiscal " +
													 "					 AND A.a_fiscal = @AFiscal " +
													 "					 AND RTRIM(LTRIM(A.tipo_mov)) = 'N' " +
													 "					 AND RTRIM(LTRIM(mot_010)) = '' " + //--R + 5MIN
													 "					 AND RTRIM(LTRIM(mot_101)) = ''" + //--R + 4HRS
													 "					 AND ( " +
													 "									( LEN(RTRIM(LTRIM(A.HORA_ENT))) = 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL))) = 5 ) " +
													 "									OR " +
													 "									( " +
													 "										LEN(RTRIM(LTRIM(A.HORA_ENT))) = 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL))) = 5 " +
													 "										AND LEN(RTRIM(LTRIM(A.HORA_ENT2))) = 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL2))) = 5 " +
													 "									) " +
													 "									OR " +
													 "									( " +
													 "										LEN(RTRIM(LTRIM(A.HORA_ENT))) = 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL))) = 5 " +
													 "										AND LEN(RTRIM(LTRIM(A.HORA_ENT2))) = 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL2))) = 5 " +
													 "										AND LEN(RTRIM(LTRIM(A.HORA_ENT3))) = 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL3))) = 5 " +
													 "									) " +
													 "									OR " +
													 "									( " +
													 "										LEN(RTRIM(LTRIM(A.HORA_ENT))) = 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL))) = 5 " +
													 "										AND LEN(RTRIM(LTRIM(A.HORA_ENT2))) = 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL2))) = 5 " +
													 "										AND LEN(RTRIM(LTRIM(A.HORA_ENT3))) = 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL3))) = 5 " +
													 "										AND LEN(RTRIM(LTRIM(A.HORA_ENT4))) = 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL4))) = 5 " +
													 "									) " +
													 "					     ) ";
			#endregion

			if (!string.IsNullOrEmpty(psClaveEmp.Trim()))
			{
				cmdTabla.CommandText += " AND clave_emp = @psClaveEmp";
				cmdTabla.Parameters.Add("@psClaveEmp", SqlDbType.NVarChar).Value = psClaveEmp;
				cmdTabla.CommandText += " ";
			}
			cmdTabla.CommandText += " ORDER BY  A.clave_emp ,A.fecha_mov";

			cmdTabla.Parameters.Add("@SFiscal", SqlDbType.Int).Value = pniWSFiscal;
			cmdTabla.Parameters.Add("@AFiscal", SqlDbType.Int).Value = pniWAFiscal;
			daTablas.SelectCommand = cmdTabla;

			tab_AsisDiaPerm.Clear();
			daTablas.Fill(tab_AsisDiaPerm);

			#endregion

			DataView vUnParMarcajes = new DataView(tab_AsisDiaPerm, "LEN(hora_ent) = 5 AND LEN(hora_sal) = 5 AND LEN(hora_ent2) = 0 AND LEN(hora_sal2) = 0 AND LEN(hora_ent3) = 0 AND LEN(hora_sal3) = 0 AND LEN(hora_ent4) = 0 AND LEN(hora_sal4) = 0  ", "", DataViewRowState.CurrentRows);
			DataView vDosParesMarcajes = new DataView(tab_AsisDiaPerm, "LEN(hora_ent) = 5 AND LEN(hora_sal) = 5 AND LEN(hora_ent2) = 5 AND LEN(hora_sal2) = 5 AND LEN(hora_ent3) = 0 AND LEN(hora_sal3) = 0 AND LEN(hora_ent4) = 0 AND LEN(hora_sal4) = 0  ", "", DataViewRowState.CurrentRows);
			DataView vTresParesMarcajes = new DataView(tab_AsisDiaPerm, "LEN(hora_ent) = 5 AND LEN(hora_sal) = 5 AND LEN(hora_ent2) = 5 AND LEN(hora_sal2) = 5 AND LEN(hora_ent3) = 5 AND LEN(hora_sal3) = 5 AND LEN(hora_ent4) = 0 AND LEN(hora_sal4) = 0  ", "", DataViewRowState.CurrentRows);

			int claveEmpleado = 0;
			Int64 idADP = 0;
			decimal horasNormales = 0.0M;
			decimal horasNormalesADP = 0.0M;
			decimal horasLaboradas = 0.0M;
			decimal horasExtras = 0.0M;
			decimal agregaTiempo = 0.0M;
			decimal quitaTiempo = 0.0M;
			decimal retardo = 0.00M;
			string horario = "";

			string tipoMovimiento = "";
			string horaEntrada, horaSalida, entradaHorario, salidaHorario;

			bool diaHabil = false;
			bool cambioDia = false;

			DateTime fechaMovimiento;

			DateTime marcajeEntrada;
			DateTime marcajeSalida;
			DateTime marcajeEntrada2;
			DateTime marcajeSalida2;
			DateTime marcajeEntrada3;
			DateTime marcajeSalida3;

			DateTime EntradaLaboral;
			DateTime SalidaLaboral;
			DateTime EntradaLaboral2;
			DateTime SalidaLaboral2;
			DateTime EntradaLaboral3;
			DateTime SalidaLaboral3;

			DateTime EntradaExtra;
			DateTime SalidaExtra;
			DateTime EntradaExtra2;
			DateTime SalidaExtra2;
			DateTime EntradaExtra3;
			DateTime SalidaExtra3;

			DateTime EntradaExtraFinal;
			DateTime SalidaExtraFinal;
			DateTime EntradaExtraFinal2;
			DateTime SalidaExtraFinal2;
			DateTime EntradaExtraFinal3;
			DateTime SalidaExtraFinal3;

			DateTime marcajeEntradaHorario;
			DateTime marcajeSalidaHorario;
			DateTime marcajeEntradaHorarioTolerancia;
			DateTime marcajeSalidaHorarioTolerancia;

			DateTime fechaDefault = new DateTime(2050, 12, 31);

			TimeSpan EntradaHorario;
			TimeSpan SalidaHorario;
			TimeSpan Entrada;
			TimeSpan Salida;
			TimeSpan valorMaximo = TimeSpan.MaxValue;

			TipoMotivos motivo = TipoMotivos.SinIncidencia;

			DataTable TVPPLPermisoParcial = creaTVPPLPermisoParcial();

			decimal permisosAcumulado = 0.0M;
			decimal permisosAcumuladoAuxiliar = 0.0M;
			TimeSpan horaInicial = new TimeSpan(0, 0, 0);

			#region [UN PAR DE MARCAJES COMPLETOS]
			foreach (DataRow asistencia in vUnParMarcajes.ToTable().Rows)
			{
				//switch (asistencia["clave_emp"].ToString().Trim())
				//{
				//	case "12092":
				//		string sbreak = "";
				//		break;
				//}
				idADP = Convert.ToInt64(asistencia["id_ADP"]);
				claveEmpleado = Convert.ToInt32(asistencia["clave_emp"]);
				horario = asistencia["horario"].ToString().Trim();

				horasNormales = Convert.ToDecimal(asistencia["horasDia"]);
				horasNormalesADP = Convert.ToDecimal(asistencia["horas_nor"]);
				horasExtras = 0.00M;
				horasLaboradas = 0.00M;

				retardo = 0.0M;
				motivo = TipoMotivos.SinIncidencia;

				agregaTiempo = 0.0M;
				quitaTiempo = 0.0M;

				tipoMovimiento = "N";

				fechaMovimiento = Convert.ToDateTime(asistencia["fecha_mov"]);
				entradaHorario = asistencia["entradaSalidaHorario"].ToString().Trim();
				salidaHorario = asistencia["entradaSalidaHorario"].ToString().Trim();
				horaEntrada = asistencia["hora_ent"].ToString();
				horaSalida = asistencia["hora_sal"].ToString();

				diaHabil = false;
				if (asistencia["entradaSalidaHorario"].ToString().Trim() != "")
				{ diaHabil = true; }

				//horario 
				EntradaHorario = entradaHorario != "" && entradaHorario != "?" && entradaHorario != ":" ? TimeSpan.Parse(entradaHorario.Substring(0, 5)) : valorMaximo;
				SalidaHorario = salidaHorario != "" && salidaHorario != "?" && salidaHorario != ":" ? TimeSpan.Parse(salidaHorario.Substring(5, 5)) : valorMaximo;

				marcajeEntradaHorario = EntradaHorario != valorMaximo ? fechaMovimiento.Add(EntradaHorario) : fechaMovimiento;
				marcajeSalidaHorario = SalidaHorario != valorMaximo ? fechaMovimiento.Add(SalidaHorario) : fechaMovimiento;

				//Tolerancia entrada salida horarios
				marcajeEntradaHorarioTolerancia = EntradaHorario != valorMaximo ? marcajeEntradaHorario.AddMinutes(-Convert.ToDouble(asistencia["toleranciaEntrada"])) : marcajeEntradaHorario;
				marcajeSalidaHorarioTolerancia = SalidaHorario != valorMaximo ? marcajeSalidaHorario.AddMinutes(Convert.ToDouble(asistencia["toleranciaSalida"])) : marcajeSalidaHorario;

				//marcaje par 1
				Entrada = horaEntrada != "" && horaEntrada != "?" && horaEntrada != ":" ? TimeSpan.Parse(horaEntrada) : valorMaximo;
				Salida = horaEntrada != "" && horaEntrada != "?" && horaSalida != ":" ? TimeSpan.Parse(horaSalida) : valorMaximo;

				marcajeEntrada = fechaMovimiento.Add(Entrada);
				marcajeSalida = fechaMovimiento.Add(Salida);

				cambioDia = false;
				//agrego dia para horarios que cambian de dia
				switch (marcajeSalidaHorario.CompareTo(marcajeEntradaHorario))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalidaHorario = marcajeSalidaHorario.AddDays(1);
						marcajeSalidaHorarioTolerancia = marcajeSalidaHorarioTolerancia.AddDays(1);
						cambioDia = true;
						break;
				}

				if (cambioDia)
				{
					switch (marcajeEntrada.CompareTo(marcajeSalidaHorario.AddDays(-1)))
					{
						case -1://Fecha de entrada es mayor a fecha de salida 
							marcajeEntrada = marcajeEntrada.AddDays(1);
							break;
						default:
							TimeSpan duracion = marcajeEntrada.Subtract(marcajeEntradaHorario).Duration();
							if (TiempoDetalle(duracion) > horasNormales)
							{
								switch (marcajeEntrada.CompareTo(marcajeEntradaHorario))
								{
									case 1:
										marcajeEntrada = marcajeEntrada.AddDays(1);
										break;
									default:
										//Es menor entrada marcaje que la de horario por tanto no hace nada 
										break;
								}
							}
							break;
					}
				}
				else if (marcajeEntradaHorario.TimeOfDay == horaInicial && diaHabil)
				{
					switch (marcajeEntrada.CompareTo(marcajeEntradaHorario))
					{
						case 1://Fecha de entrada es mayor a fecha de salida 
							marcajeEntrada = marcajeEntrada.AddDays(-1);
							break;
					}
				}

				//agrego dia a salida cuando la salida es menor a entrada 
				switch (marcajeSalida.CompareTo(marcajeEntrada))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalida = marcajeSalida.AddDays(1);
						break;
				}

				//Acomodo de Marcajes Laborales,Extras   Inicio y Final              
				EntradaLaboral = fechaDefault;
				SalidaLaboral = fechaDefault;
				EntradaExtra = fechaDefault;
				SalidaExtra = fechaDefault;
				EntradaExtraFinal = fechaDefault;
				SalidaExtraFinal = fechaDefault;


				//MARCAJES PARA HORA LABORADAS                
				if (marcajeEntrada >= marcajeEntradaHorario && marcajeEntrada <= marcajeSalidaHorario)
				{
					EntradaLaboral = marcajeEntrada;
				}
				else if (marcajeEntrada < marcajeEntradaHorario && marcajeSalida > marcajeEntradaHorario)
				{
					EntradaLaboral = marcajeEntradaHorario;
				}

				if (EntradaLaboral != fechaDefault)
				{
					if (marcajeSalida >= marcajeEntradaHorario && marcajeSalida <= marcajeSalidaHorario)
					{
						SalidaLaboral = marcajeSalida;
					}
					else if (marcajeSalida > marcajeSalidaHorario)
					{
						SalidaLaboral = marcajeSalidaHorario;
					}
				}


				permisosAcumulado = 0.0M;

				switch (FuncionesComunes.ObtenerTipoTrabajador(claveEmpleado))
				{
					case TipoTrabajador.Personal:
					case TipoTrabajador.TOMPersonal:

						#region DIA HABIL
						if (diaHabil)
						{
							//permisos 
							if (SalidaLaboral == fechaDefault && EntradaLaboral == fechaDefault)
							{
								motivo = TipoMotivos.PermisoParcialMayor4Horas;
							}
							else
							{
								if (SalidaLaboral != fechaDefault)
								{
									if (SalidaLaboral < marcajeSalidaHorario)
									{
										permisosAcumulado = (decimal)marcajeSalidaHorario.Subtract(SalidaLaboral).TotalMinutes;

										//if (permisosAcumulado < 6M) //Menor a 6 Minutos 
										if (permisosAcumulado <= 30M) //Menor Igual a 30 Minutos 
										{
											motivo = TipoMotivos.SinIncidencia;
											permisosAcumulado = 0.0M;
										}
										else if (permisosAcumulado >= 240M) //Mayor  igual a 4 Horas
											motivo = TipoMotivos.PermisoParcialMayor4Horas;
										else
											motivo = TipoMotivos.PermisoParcialMenor4Horas;

									}
								}
							}
						}
						#endregion
						break;
				}

				GrabaDatosPermisoParcialPL(idADP, permisosAcumulado, motivo, ref TVPPLPermisoParcial);

			}

			//Actualiza en SQL 
			SqlCommand cmdActualizaPL = new SqlCommand("ActualizaDatosPermisoParcialPL", conexion);
			cmdActualizaPL.CommandType = CommandType.StoredProcedure;
			cmdActualizaPL.Parameters.Add("@DatosPL", SqlDbType.Structured).Value = TVPPLPermisoParcial;
			cmdActualizaPL.ExecuteNonQuery();

			TVPPLPermisoParcial.Dispose();
			TVPPLPermisoParcial.Clear();
			TVPPLPermisoParcial = creaTVPPLPermisoParcial();
			#endregion

			#region [DOS PARES MARCAJES COMPLETOS]

			foreach (DataRow asistencia in vDosParesMarcajes.ToTable().Rows)
			{
				idADP = Convert.ToInt64(asistencia["id_ADP"]);
				claveEmpleado = Convert.ToInt32(asistencia["clave_emp"]);
				horario = asistencia["horario"].ToString().Trim();

				horasNormales = Convert.ToDecimal(asistencia["horasDia"]);
				horasNormalesADP = Convert.ToDecimal(asistencia["horas_nor"]);
				horasExtras = 0.00M;
				horasLaboradas = 0.00M;

				retardo = 0.0M;
				motivo = TipoMotivos.SinIncidencia;

				agregaTiempo = 0.0M;
				quitaTiempo = 0.0M;

				tipoMovimiento = "N";

				fechaMovimiento = Convert.ToDateTime(asistencia["fecha_mov"]);
				entradaHorario = asistencia["entradaSalidaHorario"].ToString().Trim();
				salidaHorario = asistencia["entradaSalidaHorario"].ToString().Trim();


				diaHabil = false;
				if (asistencia["entradaSalidaHorario"].ToString().Trim() != "")
				{ diaHabil = true; }

				//horario 
				EntradaHorario = entradaHorario != "" && entradaHorario != "?" && entradaHorario != ":" ? TimeSpan.Parse(entradaHorario.Substring(0, 5)) : valorMaximo;
				SalidaHorario = salidaHorario != "" && salidaHorario != "?" && salidaHorario != ":" ? TimeSpan.Parse(salidaHorario.Substring(5, 5)) : valorMaximo;

				marcajeEntradaHorario = EntradaHorario != valorMaximo ? fechaMovimiento.Add(EntradaHorario) : fechaMovimiento;
				marcajeSalidaHorario = SalidaHorario != valorMaximo ? fechaMovimiento.Add(SalidaHorario) : fechaMovimiento;

				//Tolerancia entrada salida horarios
				marcajeEntradaHorarioTolerancia = EntradaHorario != valorMaximo ? marcajeEntradaHorario.AddMinutes(-Convert.ToDouble(asistencia["toleranciaEntrada"])) : marcajeEntradaHorario;
				marcajeSalidaHorarioTolerancia = SalidaHorario != valorMaximo ? marcajeSalidaHorario.AddMinutes(Convert.ToDouble(asistencia["toleranciaSalida"])) : marcajeSalidaHorario;

				//marcaje par 1
				horaEntrada = asistencia["hora_ent"].ToString();
				horaSalida = asistencia["hora_sal"].ToString();

				Entrada = horaEntrada != "" && horaEntrada != "?" && horaEntrada != ":" ? TimeSpan.Parse(horaEntrada) : valorMaximo;
				Salida = horaEntrada != "" && horaEntrada != "?" && horaSalida != ":" ? TimeSpan.Parse(horaSalida) : valorMaximo;

				marcajeEntrada = fechaMovimiento.Add(Entrada);
				marcajeSalida = fechaMovimiento.Add(Salida);


				//marcaje par 2
				horaEntrada = asistencia["hora_ent2"].ToString();
				horaSalida = asistencia["hora_sal2"].ToString();
				Entrada = horaEntrada != "" && horaEntrada != "?" && horaEntrada != ":" ? TimeSpan.Parse(horaEntrada) : valorMaximo;
				Salida = horaEntrada != "" && horaEntrada != "?" && horaSalida != ":" ? TimeSpan.Parse(horaSalida) : valorMaximo;

				marcajeEntrada2 = fechaMovimiento.Add(Entrada);
				marcajeSalida2 = fechaMovimiento.Add(Salida);

				cambioDia = false;
				//agrego dia para horarios que cambian de dia
				switch (marcajeSalidaHorario.CompareTo(marcajeEntradaHorario))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalidaHorario = marcajeSalidaHorario.AddDays(1);
						marcajeSalidaHorarioTolerancia = marcajeSalidaHorarioTolerancia.AddDays(1);
						cambioDia = true;
						break;
				}

				if (cambioDia)
				{
					switch (marcajeEntrada.CompareTo(marcajeSalidaHorario.AddDays(-1)))
					{
						case -1://Fecha de entrada es mayor a fecha de salida 
							marcajeEntrada = marcajeEntrada.AddDays(1);
							break;
						default:
							TimeSpan duracion = marcajeEntrada.Subtract(marcajeEntradaHorario).Duration();
							if (TiempoDetalle(duracion) > horasNormales)
							{
								switch (marcajeEntrada.CompareTo(marcajeEntradaHorario))
								{
									case 1:
										marcajeEntrada = marcajeEntrada.AddDays(1);
										break;
									default:
										//Es menor entrada marcaje que la de horario por tanto no hace nada 
										break;
								}
							}
							break;
					}
				}
				else if (marcajeEntradaHorario.TimeOfDay == horaInicial && diaHabil)
				{
					switch (marcajeEntrada.CompareTo(marcajeEntradaHorario))
					{
						case 1://Fecha de entrada es mayor a fecha de salida 
							marcajeEntrada = marcajeEntrada.AddDays(-1);
							break;
					}
				}

				//agrego dia a salida cuando la salida es menor a entrada 
				switch (marcajeSalida.CompareTo(marcajeEntrada))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalida = marcajeSalida.AddDays(1);
						break;
				}

				switch (marcajeEntrada2.CompareTo(marcajeSalidaHorario))
				{
					case 1://Entrada 2 es mayor a salida horario

						switch (marcajeEntrada2.CompareTo(marcajeEntradaHorario))
						{
							case -1://Entrada 2 es menor a salida de horario, por tanto hubo cambio de dia
								marcajeEntrada2 = marcajeEntrada2.AddDays(1);
								marcajeSalida2 = marcajeSalida2.AddDays(1);
								break;
						}

						break;
				}

				//agrego dia a salida cuando la salida es menor a entrada 
				switch (marcajeSalida2.CompareTo(marcajeEntrada2))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalida2 = marcajeSalida2.AddDays(1);
						break;
				}

				//Valida que entrada2 siga en mismo dia que salida1 
				switch (marcajeEntrada2.CompareTo(marcajeSalida))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeEntrada2 = marcajeEntrada2.AddDays(1);
						marcajeSalida2 = marcajeSalida2.AddDays(1);
						break;
				}

				//Acomodo de Marcajes Laborales,Extras   Inicio y Final              
				EntradaLaboral = fechaDefault;
				SalidaLaboral = fechaDefault;
				EntradaLaboral2 = fechaDefault;
				SalidaLaboral2 = fechaDefault;

				EntradaExtra = fechaDefault;
				SalidaExtra = fechaDefault;
				EntradaExtra2 = fechaDefault;
				SalidaExtra2 = fechaDefault;

				EntradaExtraFinal = fechaDefault;
				SalidaExtraFinal = fechaDefault;
				EntradaExtraFinal2 = fechaDefault;
				SalidaExtraFinal2 = fechaDefault;

				#region MARCAJES PARA HORAS LABORADAS
				
				//MARCAJES PARA HORA LABORADAS  PRIMER PAR MARCAJES
				if (marcajeEntrada >= marcajeEntradaHorario && marcajeEntrada <= marcajeSalidaHorario)
				{
					EntradaLaboral = marcajeEntrada;
				}
				else if (marcajeEntrada < marcajeEntradaHorario && marcajeSalida > marcajeEntradaHorario)
				{
					EntradaLaboral = marcajeEntradaHorario;
				}

				if (EntradaLaboral != fechaDefault)
				{
					if (marcajeSalida >= marcajeEntradaHorario && marcajeSalida <= marcajeSalidaHorario)
					{
						SalidaLaboral = marcajeSalida;
					}
					else if (marcajeSalida > marcajeSalidaHorario)
					{
						SalidaLaboral = marcajeSalidaHorario;
					}
				}

				//MARCAJES PARA HORA LABORADAS  SEGUNDO PAR MARCAJES              
				if (marcajeEntrada2 >= marcajeEntradaHorario && marcajeEntrada2 <= marcajeSalidaHorario)
				{
					EntradaLaboral2 = marcajeEntrada2;
				}
				else if (marcajeEntrada2 < marcajeEntradaHorario && marcajeSalida2 > marcajeEntradaHorario)
				{
					EntradaLaboral2 = marcajeEntradaHorario;
				}

				if (EntradaLaboral2 != fechaDefault)
				{
					if (marcajeSalida2 >= marcajeEntradaHorario && marcajeSalida2 <= marcajeSalidaHorario)
					{
						SalidaLaboral2 = marcajeSalida2;
					}
					else if (marcajeSalida2 > marcajeSalidaHorario)
					{
						SalidaLaboral2 = marcajeSalidaHorario;
					}
				}

				#endregion

				permisosAcumulado = 0.0M;
				permisosAcumuladoAuxiliar = 0.0M;

				switch (FuncionesComunes.ObtenerTipoTrabajador(claveEmpleado))
				{
					case TipoTrabajador.Personal:
					case TipoTrabajador.TOMPersonal:

						#region DIA HABIL
						if (diaHabil)
						{
							//permisos 
							if (SalidaLaboral == fechaDefault && EntradaLaboral == fechaDefault && SalidaLaboral2 == fechaDefault && EntradaLaboral2 == fechaDefault)
							{
								motivo = TipoMotivos.PermisoParcialMayor4Horas;
							}
							else
							{

								//permisos 
								permisosAcumuladoAuxiliar = 0.0M;
								if (SalidaLaboral != fechaDefault)
								{
									if (EntradaLaboral2 != fechaDefault)
										permisosAcumuladoAuxiliar = (decimal)EntradaLaboral2.Subtract(SalidaLaboral).TotalMinutes;
									else
									{
										if (SalidaLaboral < marcajeSalidaHorario)
											permisosAcumuladoAuxiliar = (decimal)marcajeSalidaHorario.Subtract(SalidaLaboral).TotalMinutes;
									}

									permisosAcumulado = permisosAcumuladoAuxiliar;
									permisosAcumuladoAuxiliar = 0.0M;
									if (EntradaLaboral2 != fechaDefault)
									{

										if (SalidaLaboral2 < marcajeSalidaHorario)
										{
											permisosAcumuladoAuxiliar = (decimal)marcajeSalidaHorario.Subtract(SalidaLaboral2).TotalMinutes;
										}

									}
									permisosAcumulado += permisosAcumuladoAuxiliar;


									//if (permisosAcumulado < 6M) //Menor a 6 Minutos 
									if (permisosAcumulado <= 30M) //Menor Igual a 30 Minutos 
									{
										motivo = TipoMotivos.SinIncidencia;
										permisosAcumulado = 0.0M;
									}
									else if (permisosAcumulado >= 240M) //Mayor  igual a 4 Horas
										motivo = TipoMotivos.PermisoParcialMayor4Horas;
									else
										motivo = TipoMotivos.PermisoParcialMenor4Horas;

								}
								else if (EntradaLaboral2 != fechaDefault)
								{
									permisosAcumulado = 0.0M;
									if (SalidaLaboral2 < marcajeSalidaHorario)
									{
										permisosAcumulado = (decimal)marcajeSalidaHorario.Subtract(SalidaLaboral2).TotalMinutes;
									}

									//if (permisosAcumulado < 6M) //Menor a 6 Minutos 
									if (permisosAcumulado <= 30M) //Menor Igual a 30 Minutos 
									{
										motivo = TipoMotivos.SinIncidencia;
										permisosAcumulado = 0.0M;
									}
									else if (permisosAcumulado >= 240M) //Mayor  igual a 4 Horas
										motivo = TipoMotivos.PermisoParcialMayor4Horas;
									else
										motivo = TipoMotivos.PermisoParcialMenor4Horas;

								}
							}
						}
						#endregion

						break;

				}

				GrabaDatosPermisoParcialPL(idADP, 0, motivo, ref TVPPLPermisoParcial);
			}

			//Actualiza en SQL 
			cmdActualizaPL = new SqlCommand("ActualizaDatosPermisoParcialPL", conexion);
			cmdActualizaPL.CommandType = CommandType.StoredProcedure;
			cmdActualizaPL.Parameters.Add("@DatosPL", SqlDbType.Structured).Value = TVPPLPermisoParcial;
			cmdActualizaPL.ExecuteNonQuery();

			TVPPLPermisoParcial.Dispose();
			TVPPLPermisoParcial.Clear();
			TVPPLPermisoParcial = creaTVPPLPermisoParcial();
			#endregion

			#region [TRES PARES DE MARCAJES COMPLETOS]
			
			foreach (DataRow asistencia in vTresParesMarcajes.ToTable().Rows)
			{
				idADP = Convert.ToInt64(asistencia["id_ADP"]);
				claveEmpleado = Convert.ToInt32(asistencia["clave_emp"]);
				horario = asistencia["horario"].ToString().Trim();

				horasNormales = Convert.ToDecimal(asistencia["horasDia"]);
				horasNormalesADP = Convert.ToDecimal(asistencia["horas_nor"]);
				horasExtras = 0.00M;
				horasLaboradas = 0.00M;

				retardo = 0.0M;
				motivo = TipoMotivos.SinIncidencia;

				agregaTiempo = 0.0M;
				quitaTiempo = 0.0M;

				tipoMovimiento = "N";

				fechaMovimiento = Convert.ToDateTime(asistencia["fecha_mov"]);
				entradaHorario = asistencia["entradaSalidaHorario"].ToString().Trim();
				salidaHorario = asistencia["entradaSalidaHorario"].ToString().Trim();


				diaHabil = false;
				if (asistencia["entradaSalidaHorario"].ToString().Trim() != "")
				{ diaHabil = true; }

				//horario 
				EntradaHorario = entradaHorario != "" && entradaHorario != "?" && entradaHorario != ":" ? TimeSpan.Parse(entradaHorario.Substring(0, 5)) : valorMaximo;
				SalidaHorario = salidaHorario != "" && salidaHorario != "?" && salidaHorario != ":" ? TimeSpan.Parse(salidaHorario.Substring(5, 5)) : valorMaximo;

				marcajeEntradaHorario = EntradaHorario != valorMaximo ? fechaMovimiento.Add(EntradaHorario) : fechaMovimiento;
				marcajeSalidaHorario = SalidaHorario != valorMaximo ? fechaMovimiento.Add(SalidaHorario) : fechaMovimiento;

				//Tolerancia entrada salida horarios
				marcajeEntradaHorarioTolerancia = EntradaHorario != valorMaximo ? marcajeEntradaHorario.AddMinutes(-Convert.ToDouble(asistencia["toleranciaEntrada"])) : marcajeEntradaHorario;
				marcajeSalidaHorarioTolerancia = SalidaHorario != valorMaximo ? marcajeSalidaHorario.AddMinutes(Convert.ToDouble(asistencia["toleranciaSalida"])) : marcajeSalidaHorario;

				//marcaje par 1
				horaEntrada = asistencia["hora_ent"].ToString();
				horaSalida = asistencia["hora_sal"].ToString();
				Entrada = horaEntrada != "" && horaEntrada != "?" && horaEntrada != ":" ? TimeSpan.Parse(horaEntrada) : valorMaximo;
				Salida = horaEntrada != "" && horaEntrada != "?" && horaSalida != ":" ? TimeSpan.Parse(horaSalida) : valorMaximo;
				marcajeEntrada = fechaMovimiento.Add(Entrada);
				marcajeSalida = fechaMovimiento.Add(Salida);


				//marcaje par 2
				horaEntrada = asistencia["hora_ent2"].ToString();
				horaSalida = asistencia["hora_sal2"].ToString();
				Entrada = horaEntrada != "" && horaEntrada != "?" && horaEntrada != ":" ? TimeSpan.Parse(horaEntrada) : valorMaximo;
				Salida = horaEntrada != "" && horaEntrada != "?" && horaSalida != ":" ? TimeSpan.Parse(horaSalida) : valorMaximo;
				marcajeEntrada2 = fechaMovimiento.Add(Entrada);
				marcajeSalida2 = fechaMovimiento.Add(Salida);

				//marcaje par 3
				horaEntrada = asistencia["hora_ent3"].ToString();
				horaSalida = asistencia["hora_sal3"].ToString();
				Entrada = horaEntrada != "" && horaEntrada != "?" && horaEntrada != ":" ? TimeSpan.Parse(horaEntrada) : valorMaximo;
				Salida = horaEntrada != "" && horaEntrada != "?" && horaSalida != ":" ? TimeSpan.Parse(horaSalida) : valorMaximo;
				marcajeEntrada3 = fechaMovimiento.Add(Entrada);
				marcajeSalida3 = fechaMovimiento.Add(Salida);


				cambioDia = false;
				//agrego dia para horarios que cambian de dia
				switch (marcajeSalidaHorario.CompareTo(marcajeEntradaHorario))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalidaHorario = marcajeSalidaHorario.AddDays(1);
						marcajeSalidaHorarioTolerancia = marcajeSalidaHorarioTolerancia.AddDays(1);
						cambioDia = true;
						break;
				}

				if (cambioDia)
				{
					switch (marcajeEntrada.CompareTo(marcajeSalidaHorario.AddDays(-1)))
					{
						case -1://Fecha de entrada es mayor a fecha de salida 
							marcajeEntrada = marcajeEntrada.AddDays(1);
							break;
						default:
							TimeSpan duracion = marcajeEntrada.Subtract(marcajeEntradaHorario).Duration();
							if (TiempoDetalle(duracion) > horasNormales)
							{
								switch (marcajeEntrada.CompareTo(marcajeEntradaHorario))
								{
									case 1:
										marcajeEntrada = marcajeEntrada.AddDays(1);
										break;
									default:
										//Es menor entrada marcaje que la de horario por tanto no hace nada 
										break;
								}
							}
							break;
					}
				}

				//agrego dia a salida cuando la salida es menor a entrada 
				switch (marcajeSalida.CompareTo(marcajeEntrada))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalida = marcajeSalida.AddDays(1);
						break;
				}


				switch (marcajeEntrada2.CompareTo(marcajeEntradaHorario))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeEntrada2 = marcajeEntrada2.AddDays(1);
						marcajeSalida2 = marcajeSalida2.AddDays(1);
						break;
				}

				//agrego dia a salida cuando la salida es menor a entrada 
				switch (marcajeSalida2.CompareTo(marcajeEntrada2))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalida2 = marcajeSalida2.AddDays(1);
						break;
				}


				//Valida que entrada2 siga en mismo dia que salida1 
				switch (marcajeEntrada2.CompareTo(marcajeSalida))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeEntrada2 = marcajeEntrada2.AddDays(1);
						marcajeSalida2 = marcajeSalida2.AddDays(1);
						break;
				}

				//--
				switch (marcajeEntrada3.CompareTo(marcajeEntradaHorario))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeEntrada3 = marcajeEntrada3.AddDays(1);
						marcajeSalida3 = marcajeSalida3.AddDays(1);
						break;
				}

				//agrego dia a salida cuando la salida es menor a entrada 
				switch (marcajeSalida3.CompareTo(marcajeEntrada3))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeSalida3 = marcajeSalida3.AddDays(1);
						break;
				}


				//Valida que entrada2 siga en mismo dia que salida1 
				switch (marcajeEntrada3.CompareTo(marcajeSalida2))
				{
					case -1://Fecha de entrada es mayor a fecha de salida 
						marcajeEntrada3 = marcajeEntrada3.AddDays(1);
						marcajeSalida3 = marcajeSalida3.AddDays(1);
						break;
				}


				//Acomodo de Marcajes Laborales,Extras   Inicio y Final              
				EntradaLaboral = fechaDefault;
				SalidaLaboral = fechaDefault;
				EntradaLaboral2 = fechaDefault;
				SalidaLaboral2 = fechaDefault;
				EntradaLaboral3 = fechaDefault;
				SalidaLaboral3 = fechaDefault;

				EntradaExtra = fechaDefault;
				SalidaExtra = fechaDefault;
				EntradaExtra2 = fechaDefault;
				SalidaExtra2 = fechaDefault;
				EntradaExtra3 = fechaDefault;
				SalidaExtra3 = fechaDefault;

				EntradaExtraFinal = fechaDefault;
				SalidaExtraFinal = fechaDefault;
				EntradaExtraFinal2 = fechaDefault;
				SalidaExtraFinal2 = fechaDefault;
				EntradaExtraFinal3 = fechaDefault;
				SalidaExtraFinal3 = fechaDefault;


				#region MARCAJES PARA HORAS LABORADAS
				//MARCAJES PARA HORA LABORADAS  PRIMER PAR MARCAJES              
				if (marcajeEntrada >= marcajeEntradaHorario && marcajeEntrada <= marcajeSalidaHorario)
				{
					EntradaLaboral = marcajeEntrada;
				}
				else if (marcajeEntrada < marcajeEntradaHorario && marcajeSalida > marcajeEntradaHorario)
				{
					EntradaLaboral = marcajeEntradaHorario;
				}

				if (EntradaLaboral != fechaDefault)
				{
					if (marcajeSalida >= marcajeEntradaHorario && marcajeSalida <= marcajeSalidaHorario)
					{
						SalidaLaboral = marcajeSalida;
					}
					else if (marcajeSalida > marcajeSalidaHorario)
					{
						SalidaLaboral = marcajeSalidaHorario;
					}
				}

				//MARCAJES PARA HORA LABORADAS  SEGUNDO PAR MARCAJES                          
				if (marcajeEntrada2 >= marcajeEntradaHorario && marcajeEntrada2 <= marcajeSalidaHorario)
				{
					EntradaLaboral2 = marcajeEntrada2;
				}
				else if (marcajeEntrada2 < marcajeEntradaHorario && marcajeSalida2 > marcajeEntradaHorario)
				{
					EntradaLaboral2 = marcajeEntradaHorario;
				}

				if (EntradaLaboral2 != fechaDefault)
				{
					if (marcajeSalida2 >= marcajeEntradaHorario && marcajeSalida2 <= marcajeSalidaHorario)
					{
						SalidaLaboral2 = marcajeSalida2;
					}
					else if (marcajeSalida2 > marcajeSalidaHorario)
					{
						SalidaLaboral2 = marcajeSalidaHorario;
					}
				}

				//MARCAJES PARA HORA LABORADAS  TERCER PAR MARCAJES                          
				if (marcajeEntrada3 >= marcajeEntradaHorario && marcajeEntrada3 <= marcajeSalidaHorario)
				{
					EntradaLaboral3 = marcajeEntrada3;
				}
				else if (marcajeEntrada3 < marcajeEntradaHorario && marcajeSalida3 > marcajeEntradaHorario)
				{
					EntradaLaboral3 = marcajeEntradaHorario;
				}

				if (EntradaLaboral3 != fechaDefault)
				{
					if (marcajeSalida3 >= marcajeEntradaHorario && marcajeSalida3 <= marcajeSalidaHorario)
					{
						SalidaLaboral3 = marcajeSalida3;
					}
					else if (marcajeSalida3 > marcajeSalidaHorario)
					{
						SalidaLaboral3 = marcajeSalidaHorario;
					}
				}

				#endregion

				switch (FuncionesComunes.ObtenerTipoTrabajador(claveEmpleado))
				{
					case TipoTrabajador.Personal:
					case TipoTrabajador.TOMPersonal:

						#region DIA HABIL
						if (diaHabil)
						{
							//permisos 
							if (SalidaLaboral == fechaDefault && EntradaLaboral == fechaDefault && SalidaLaboral2 == fechaDefault && EntradaLaboral2 == fechaDefault && SalidaLaboral3 == fechaDefault && EntradaLaboral3 == fechaDefault)
							{
								motivo = TipoMotivos.PermisoParcialMayor4Horas;
							}
							else
							{
								//permisos 
								permisosAcumulado = 0.0M;
								if (SalidaLaboral != fechaDefault)
								{
									if (EntradaLaboral2 != fechaDefault)
									{
										permisosAcumuladoAuxiliar = (decimal)EntradaLaboral2.Subtract(SalidaLaboral).TotalMinutes;
										permisosAcumulado += permisosAcumuladoAuxiliar;
										permisosAcumuladoAuxiliar = 0.0M;

										if (EntradaLaboral3 != fechaDefault)
										{
											permisosAcumuladoAuxiliar = (decimal)EntradaLaboral3.Subtract(SalidaLaboral2).TotalMinutes;
											permisosAcumulado += permisosAcumuladoAuxiliar;

											permisosAcumuladoAuxiliar = 0.0M;
											if (SalidaLaboral3 < marcajeSalidaHorario)
											{
												permisosAcumuladoAuxiliar = (decimal)marcajeSalidaHorario.Subtract(SalidaLaboral3).TotalMinutes;
											}

											permisosAcumulado += permisosAcumuladoAuxiliar;
											permisosAcumuladoAuxiliar = 0.0M;

										}
										else
										{
											permisosAcumuladoAuxiliar = 0.0M;
											if (SalidaLaboral2 < marcajeSalidaHorario)
											{
												permisosAcumuladoAuxiliar = (decimal)marcajeSalidaHorario.Subtract(SalidaLaboral2).TotalMinutes;
											}
											permisosAcumulado += permisosAcumuladoAuxiliar;
											permisosAcumuladoAuxiliar = 0.0M;
										}
									}
									else
									{
										permisosAcumuladoAuxiliar = 0.0M;
										if (SalidaLaboral < marcajeSalidaHorario)
											permisosAcumuladoAuxiliar = (decimal)marcajeSalidaHorario.Subtract(SalidaLaboral).TotalMinutes;
									}

									permisosAcumulado += permisosAcumuladoAuxiliar;
									permisosAcumuladoAuxiliar = 0.0M;

									//if (permisosAcumulado < 6M) //Menor a 6 Minutos 
									if (permisosAcumulado <= 30M) //Menor Igual a 30 Minutos 
									{
										motivo = TipoMotivos.SinIncidencia;
										permisosAcumulado = 0.0M;
									}
									else if (permisosAcumulado >= 240M) //Mayor  igual a 4 Horas
										motivo = TipoMotivos.PermisoParcialMayor4Horas;
									else
										motivo = TipoMotivos.PermisoParcialMenor4Horas;
								}
								else if (EntradaLaboral2 != fechaDefault)
								{
									if (EntradaLaboral3 != fechaDefault)
									{
										permisosAcumuladoAuxiliar = (decimal)EntradaLaboral3.Subtract(SalidaLaboral2).TotalMinutes;
										permisosAcumulado += permisosAcumuladoAuxiliar;

										permisosAcumuladoAuxiliar = 0.0M;
										if (SalidaLaboral3 < marcajeSalidaHorario)
										{
											permisosAcumuladoAuxiliar = (decimal)marcajeSalidaHorario.Subtract(SalidaLaboral3).TotalMinutes;
										}

										permisosAcumulado += permisosAcumuladoAuxiliar;
										permisosAcumuladoAuxiliar = 0.0M;

									}
									else
									{
										permisosAcumuladoAuxiliar = 0.0M;
										if (SalidaLaboral2 < marcajeSalidaHorario)
										{
											permisosAcumuladoAuxiliar = (decimal)marcajeSalidaHorario.Subtract(SalidaLaboral2).TotalMinutes;
										}
										permisosAcumulado += permisosAcumuladoAuxiliar;
										permisosAcumuladoAuxiliar = 0.0M;
									}

									//if (permisosAcumulado < 6M) //Menor a 6 Minutos 
									if (permisosAcumulado <= 30M) //Menor Igual a 30 Minutos 
									{
										motivo = TipoMotivos.SinIncidencia;
										permisosAcumulado = 0.0M;
									}
									else if (permisosAcumulado >= 240M) //Mayor  igual a 4 Horas
										motivo = TipoMotivos.PermisoParcialMayor4Horas;
									else
										motivo = TipoMotivos.PermisoParcialMenor4Horas;
								}
								else if (EntradaLaboral3 != fechaDefault)
								{
									permisosAcumuladoAuxiliar = 0.0M;
									if (SalidaLaboral3 < marcajeSalidaHorario)
									{
										permisosAcumuladoAuxiliar = (decimal)marcajeSalidaHorario.Subtract(SalidaLaboral3).TotalMinutes;
									}

									permisosAcumulado += permisosAcumuladoAuxiliar;
									permisosAcumuladoAuxiliar = 0.0M;

									//if (permisosAcumulado < 6M) //Menor a 6 Minutos 
									if (permisosAcumulado <= 30M) //Menor Igual a 30 Minutos 
									{
										motivo = TipoMotivos.SinIncidencia;
										permisosAcumulado = 0.0M;
									}
									else if (permisosAcumulado >= 240M) //Mayor  igual a 4 Horas
										motivo = TipoMotivos.PermisoParcialMayor4Horas;
									else
										motivo = TipoMotivos.PermisoParcialMenor4Horas;

								}
							}
						}
						#endregion

						break;
				}
				GrabaDatosPermisoParcialPL(idADP, 0, motivo, ref TVPPLPermisoParcial);
			}

			//Actualiza en SQL 
			cmdActualizaPL = new SqlCommand("ActualizaDatosPermisoParcialPL", conexion);
			cmdActualizaPL.CommandType = CommandType.StoredProcedure;
			cmdActualizaPL.Parameters.Add("@DatosPL", SqlDbType.Structured).Value = TVPPLPermisoParcial;
			cmdActualizaPL.ExecuteNonQuery();

			TVPPLPermisoParcial.Dispose();
			TVPPLPermisoParcial.Clear();
			TVPPLPermisoParcial = creaTVPPLPermisoParcial();
			#endregion

		}

		private static void GrabaDatosPermisoParcialPL(Int64 idADP, decimal horasPermiso, TipoMotivos motivo, ref DataTable TVPPLDetallePermisos)
		{
			//Datos a actualizar en tabla SQL 
			string observacion, motivo024, motivo011;
			observacion = "";
			motivo024 = "";
			motivo011 = "";
			switch (motivo)
			{
				case TipoMotivos.PermisoParcialMenor4Horas:
					motivo024 = "1";
					motivo011 = "";
					observacion = "PERMISO PARCIAL -4 HRS.";
					break;
				case TipoMotivos.PermisoParcialMayor4Horas:
					motivo024 = "";
					motivo011 = "1";
					observacion = "PERMISO PARCIAL +4 HORAS";
					break;
			}
			TVPPLDetallePermisos.Rows.Add(new object[] { idADP, observacion, motivo024, motivo011 });
		}

		#endregion

		public void PasarLista()
		{
			SqlDataAdapter daTablas = new SqlDataAdapter();
			SqlCommand cmdTabla = new SqlCommand();
			SqlCommandBuilder cmdBuilder = new SqlCommandBuilder();
			SqlParameterCollection paramCollection = new SqlCommand().Parameters;

			DataTable tab_AsisDiaPerm = new DataTable();
			DataTable tab_Faltas = new DataTable();
			DataTable tab_MotFals = new DataTable();
			DataTable tab_Configuracion = new DataTable();

			DataRow[] rowMotFals;

			string lsHoraEnt, lsHoraSal, lsHoraEnt2, lsHoraSal2, lsHoraEnt3, lsHoraSal3, lsHoraEnt4, lsHoraSal4, lsWObserva, lsWDia, lsWTipoMov;
			int lniCveEmp, lniWTipoEmp, lniWGrabar;
			int lniIndex = 0;
			decimal lnfHoraEntrada, lnfHoraSalida;
			decimal lniToleraEnt, lniToleraSal, lnfMinutosTolera, lnfADPHorasNor;
			decimal lnfRetardoRedondeado;
			decimal lnfTextr, lnfTnextr, lnfTextrent, lnfTnextrent, lnfMintnextent, lnfMintexent,
							lnfHoraChecEntrada, lnfHoraChecSalida, lnfHoraChecEntrada2, lnfHoraChecSalida2,
							lnfHoraChecEntrada3, lnfHoraChecSalida3, lnfHoraChecEntrada4, lnfHoraChecSalida4,
							lnfWSumaTmpo, lnfWRestaTmpo, lnfSalida4hrs,
							lnfWHorasNor, lnfWHorasLab, lnfWHorasExt,
							lnfDiferen, lnfRetardo;
			bool boolDiaHabil;
			DateTime lfADPFechaMov;
			if (conexion.State == ConnectionState.Closed) conexion.Open();

			#region [CARGA MOT_FALS]
			cmdTabla = new SqlCommand("SELECT * FROM mot_fals", conexion);
			daTablas.SelectCommand = cmdTabla;
			tab_MotFals.Clear();
			daTablas.Fill(tab_MotFals);
			#endregion

			#region [CARGA CONFIGURACION]
			cmdTabla = new SqlCommand("SELECT mintolent, mintolsal FROM configuracion", conexion);
			daTablas.SelectCommand = cmdTabla;
			tab_Configuracion.Clear();
			daTablas.Fill(tab_Configuracion);
			#endregion

			#region TABLA FALTAS

			#region [SE BORRAN FALTAS AGREGADAS POR EL SISTEMA DE ASISTENCIA (RETARDOS Y FALTAS INJUSTIFICADAS)]
			cmdTabla = new SqlCommand();
			cmdTabla.Connection = conexion;
			cmdTabla.CommandText = "DELETE FROM faltas WHERE semana_fal=@pniWSFiscal AND a_fiscal=@pniWAFiscal AND RTRIM(LTRIM(referencia))='SISTEMA'";
			if (!string.IsNullOrEmpty(psClaveEmp))
			{
				cmdTabla.CommandText += " AND clave_emp=@psWClaveEmp";
				cmdTabla.Parameters.Add("@psWClaveEmp", SqlDbType.NVarChar).Value = psClaveEmp;
			}
			cmdTabla.Parameters.Add("@pniWSFiscal", SqlDbType.Int).Value = pniWSFiscal;
			cmdTabla.Parameters.Add("@pniWAFiscal", SqlDbType.Int).Value = pniWAFiscal;
			cmdTabla.ExecuteNonQuery();
			#endregion

			#region [SELECCION FILTRADO TABLA FALTAS PARA PROCESO]
			cmdTabla = new SqlCommand();
			cmdTabla.Connection = conexion;
			cmdTabla.CommandText = "SELECT clave_emp, fecha_fal, motivo_fal FROM faltas " +
														 "WHERE semana_fal=@pniWSFiscal AND a_fiscal=@pniWAFiscal";
			#region [FILTRA TABLA FALTAS POR EMPLEADO CONSULTADO]
			if (!string.IsNullOrEmpty(psClaveEmp))
			{
				cmdTabla.CommandText += " AND clave_emp=@psWClaveEmp";
				cmdTabla.Parameters.Add("@psWClaveEmp", SqlDbType.NVarChar).Value = psClaveEmp;
			}
			#endregion

			cmdTabla.CommandText += " ORDER BY clave_emp, fecha_fal";
			cmdTabla.Parameters.Add("@pniWSFiscal", SqlDbType.Int).Value = pniWSFiscal;
			cmdTabla.Parameters.Add("@pniWAFiscal", SqlDbType.Int).Value = pniWAFiscal;
			daTablas.SelectCommand = cmdTabla;
			tab_Faltas.Clear();
			daTablas.Fill(tab_Faltas);
			#endregion

			#endregion

			lnfMinutosTolera = (5 * 1.66667M) / 100.00M;
			lniToleraEnt = Convert.ToInt32(tab_Configuracion.Rows[0]["mintolent"]);
			lniToleraSal = Convert.ToInt32(tab_Configuracion.Rows[0]["mintolsal"]);

			#region [PASAR LISTA PARTE 1-5 , 2-5 , 3-5 , X-5[LIMPIA MOTIVOS] ]
			SqlCommand cmdStoreProcedure = new SqlCommand("dbo.pa_PasarLista", conexion);
			cmdStoreProcedure.CommandTimeout = 0;
			cmdStoreProcedure.CommandType = CommandType.StoredProcedure;
			cmdStoreProcedure.Parameters.Add("@pniWSFiscal", SqlDbType.Int).Value = pniWSFiscal;
			cmdStoreProcedure.Parameters.Add("@pniWAFiscal", SqlDbType.Int).Value = pniWAFiscal;
			//cmdStoreProcedure.Parameters.Add("@Bimestre", SqlDbType.Int).Value = pniWBimestre;
			cmdStoreProcedure.Parameters.Add("@CEmp", SqlDbType.NVarChar).Value = psClaveEmp;
			cmdStoreProcedure.ExecuteNonQuery();
			#endregion

			#region [PASAR LISTA 4 / 5 Apartir de TOM 2017-11-06]
			lsWObserva = "";
			lniIndex = 0;

			//MODIFICACION ANEXO MOTIVOS COMBINADOS            
			//limpia estos motivos para regenerarlos
			cmdTabla = new SqlCommand();
			cmdTabla.Connection = conexion;
			cmdTabla.Parameters.Clear();
			cmdTabla.CommandText = "UPDATE ASIS_DIA_PERM SET mot_010 = '' , mot_011= '' , mot_101= '' , mot_024= '' , mot_025= '' , mot_026= '' , mot_027= '' , mot_028 = '' WHERE s_fiscal = @pniWSFiscal AND a_fiscal = @pniWAFiscal  ";

			if (!string.IsNullOrEmpty(psClaveEmp))
			{
				cmdTabla.CommandText += " AND clave_emp = @psWClaveEmp ";
				cmdTabla.Parameters.Add("@psWClaveEmp", SqlDbType.NVarChar).Value = psClaveEmp;
				//WNum = 1
			}
			cmdTabla.Parameters.Add("@pniWSFiscal", SqlDbType.Int).Value = pniWSFiscal;
			cmdTabla.Parameters.Add("@pniWAFiscal", SqlDbType.Int).Value = pniWAFiscal;
			cmdTabla.ExecuteNonQuery();



			#region [CARGA ASIS_DIA_PERM]

			#region [SELECT QUERY]

			cmdTabla = new SqlCommand();
			cmdTabla.Connection = conexion;

			cmdTabla.CommandText = " SELECT  id_ADP , A.clave_emp,A.fecha_mov ";
			cmdTabla.CommandText += " ,RTRIM(LTRIM(A.hora_ent))AS hora_ent , RTRIM(LTRIM(A.hora_sal)) AS hora_sal,RTRIM(LTRIM(A.hora_ent2))AS hora_ent2 , RTRIM(LTRIM(A.hora_sal2)) AS hora_sal2 ";
			cmdTabla.CommandText += " ,RTRIM(LTRIM(A.hora_ent3))AS hora_ent3 , RTRIM(LTRIM(A.hora_sal3)) AS hora_sal3,RTRIM(LTRIM(A.hora_ent4))AS hora_ent4 , RTRIM(LTRIM(A.hora_sal4)) AS hora_sal4 ";
			cmdTabla.CommandText += " ,A.horas_lab ,A.horas_nor,A.horas_ext ,A.hs_ext_aut,A.hrs_ext_au,A.hrs_ext2_au ,A.hrs_ext3_au ,A.hs_ext3_au ,A.hs_ext2_au      ";
			cmdTabla.CommandText += " ,A.depto  ,A.turno ,A.superv,A.categoria ,A.grado_niv,A.division ,A.cve_pago,A.horario       ";
			cmdTabla.CommandText += " ,A.mot_01,A.mot_02 ,A.mot_03 ,A.mot_04 ,A.mot_05,A.mot_06 ,A.mot_07 ,A.mot_09,A.mot_010,A.mot_011,A.mot_012 ,A.mot_013 ";
			cmdTabla.CommandText += " ,A.mot_016 ,A.mot_017 ,A.mot_018,A.mot_019,A.mot_020 ,A.mot_023,A.mot_101 ";
			cmdTabla.CommandText += " ,A.fecha_alt,A.tipo_mov ,A.tipo_proc       ";
			cmdTabla.CommandText += " ,A.fuera_turn ,A.nvohorario ,A.diasem ,A.dia_sem       ";
			cmdTabla.CommandText += " ,A.imp_ext2  ,A.imp_ext3 ,A.tesem ,A.tedesc ,A.textra_tot ";
			cmdTabla.CommandText += " ,A.textra_dob ,A.textra_tri ,A.sdo_integ ";
			cmdTabla.CommandText += " ,A.s_fiscal ,A.a_fiscal ";
			cmdTabla.CommandText += " ,A.subido,A.observa  ";
			cmdTabla.CommandText += " ,ISNULL(H.horas_lun,'0') AS Lunes2 , ISNULL(H.horas_mar ,'0') AS Martes2 , ISNULL(H.horas_mie ,'0') AS Miercoles2 ";
			cmdTabla.CommandText += " ,ISNULL(H.horas_jue ,'0') AS Jueves2 , ISNULL( H.horas_vie ,'0') AS Viernes2 , ISNULL( H.horas_sab ,'0') AS Sabado2 , ISNULL( H.horas_dom ,'0') AS Domingo2 ";
			cmdTabla.CommandText += " ,ISNULL( H.dias_asist ,'') AS dias_asist  ";
			cmdTabla.CommandText += " ,lunesEntrada , lunesSalida , martesEntrada , martesSalida , miercolesEntrada , miercolesSalida , juevesEntrada , juevesSalida ";
			cmdTabla.CommandText += " ,viernesEntrada, viernesSalida, sabadoEntrada , sabadoSalida , domingoEntrada , domingoSalida ";
			cmdTabla.CommandText += " ,AgregaTiempo , quitaTiempo  ";
			cmdTabla.CommandText += " FROM ASIS_DIA_PERM AS A ";
			cmdTabla.CommandText += " LEFT JOIN HORARIOS AS H ON A.horario = H.horario  ";
			cmdTabla.CommandText += " LEFT JOIN vistaobtieneHorario AS V ON A.HORARIO = V.horario  ";
			cmdTabla.CommandText += " LEFT JOIN vistaQuitaAgregaTiempo AS V2 ON A.HORARIO = V2.horario  ";
			cmdTabla.CommandText += " WHERE ";
			cmdTabla.CommandText += " A.s_fiscal = @SFiscal AND A.a_fiscal = @AFiscal ";
			cmdTabla.CommandText += " AND  RTRIM(LTRIM(A.tipo_mov)) NOT IN ('D','T','X','E','Z','C','O','W')  ";
			cmdTabla.CommandText += " AND ";
			cmdTabla.CommandText += " (";
			cmdTabla.CommandText += " (";
			cmdTabla.CommandText += " LEN(RTRIM(LTRIM(A.HORA_ENT)))  = 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL)))  = 5 AND ";
			cmdTabla.CommandText += " LEN(LTRIM(RTRIM(A.HORA_ENT2))) < 2 AND LEN(RTRIM(LTRIM(A.HORA_SAL2))) < 2 AND ";
			cmdTabla.CommandText += " LEN(LTRIM(RTRIM(A.HORA_ENT3))) < 2 AND LEN(RTRIM(LTRIM(A.HORA_SAL3))) < 2 AND ";
			cmdTabla.CommandText += " LEN(RTRIM(LTRIM(A.HORA_ENT4))) < 2 AND LEN(RTRIM(LTRIM(A.HORA_SAL4))) < 2";
			cmdTabla.CommandText += " )          ";
			cmdTabla.CommandText += " OR ";
			cmdTabla.CommandText += " (";
			cmdTabla.CommandText += " LEN(RTRIM(LTRIM(A.HORA_ENT)))  = 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL)))  = 5 AND ";
			cmdTabla.CommandText += " LEN(RTRIM(LTRIM(A.HORA_ENT2))) = 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL2))) = 5 AND  ";
			cmdTabla.CommandText += " LEN(RTRIM(LTRIM(A.HORA_ENT3))) < 2 AND LEN(RTRIM(LTRIM(A.HORA_SAL3))) < 2 AND";
			cmdTabla.CommandText += " LEN(RTRIM(LTRIM(A.HORA_ENT4))) < 2 AND LEN(RTRIM(LTRIM(A.HORA_SAL4))) < 2";
			cmdTabla.CommandText += " )";
			cmdTabla.CommandText += " OR ";
			cmdTabla.CommandText += " (";
			cmdTabla.CommandText += " LEN(RTRIM(LTRIM(A.HORA_ENT))) = 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL)))  = 5 AND ";
			cmdTabla.CommandText += " LEN(RTRIM(LTRIM(A.HORA_ENT2)))= 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL2))) = 5 AND  ";
			cmdTabla.CommandText += " LEN(RTRIM(LTRIM(A.HORA_ENT3)))= 5 AND LEN(RTRIM(LTRIM(A.HORA_SAL3))) = 5 AND ";
			cmdTabla.CommandText += " LEN(RTRIM(LTRIM(A.HORA_ENT4)))< 2 AND LEN(RTRIM(LTRIM(A.HORA_SAL4))) < 2";
			cmdTabla.CommandText += " )";
			cmdTabla.CommandText += " OR";
			cmdTabla.CommandText += " (";
			cmdTabla.CommandText += " LEN(RTRIM(LTRIM(A.HORA_ENT))) =5 AND  LEN(RTRIM(LTRIM(A.HORA_SAL))) = 5 AND ";
			cmdTabla.CommandText += " LEN(RTRIM(LTRIM(A.HORA_ENT2)))=5 AND  LEN(RTRIM(LTRIM(A.HORA_SAL2)))= 5 AND  ";
			cmdTabla.CommandText += " LEN(RTRIM(LTRIM(A.HORA_ENT3)))=5 AND  LEN(RTRIM(LTRIM(A.HORA_SAL3)))= 5 AND ";
			cmdTabla.CommandText += " LEN(RTRIM(LTRIM(A.HORA_ENT4)))=5 AND  LEN(RTRIM(LTRIM(A.HORA_SAL4)))= 5";
			cmdTabla.CommandText += " )";
			cmdTabla.CommandText += " )";

			#endregion

			if (!string.IsNullOrEmpty(psClaveEmp.Trim()))
			{
				cmdTabla.CommandText += " AND clave_emp = @psClaveEmp";
				cmdTabla.Parameters.Add("@psClaveEmp", SqlDbType.NVarChar).Value = psClaveEmp;
				cmdTabla.CommandText += " ";
			}
			cmdTabla.CommandText += " ORDER BY  A.clave_emp ,A.fecha_mov";

			cmdTabla.Parameters.Add("@SFiscal", SqlDbType.Int).Value = pniWSFiscal;
			cmdTabla.Parameters.Add("@AFiscal", SqlDbType.Int).Value = pniWAFiscal;
			daTablas.SelectCommand = cmdTabla;

			tab_AsisDiaPerm.Clear();
			daTablas.Fill(tab_AsisDiaPerm);

			#endregion

			cmdBuilder.DataAdapter = daTablas;

			//POSIBLE SOLUCION CURSOR CON WHILE SI EXISTE REGISTRO LO BORRA DE LISTADO(TABLA TEMPORAL) HASTA QUE TERMINE EL PASE DE LISTA
			//IRA ACTUALIZANDO REGISTRO POR REGISTRO , EN CASO DE ESTAR LENTO NO IMPORTARA ESTA REALIZANDO EL PROCESO POR FUERA 
			//PUDIERA SER DE ESTE MODO

			foreach (DataRow rowADP_HOR in tab_AsisDiaPerm.Rows)
			{
				#region [INI. VARIABLES]

				lnfMintnextent = lnfMintexent =
				lnfTextr = lnfTnextr = lnfTextrent = lnfTnextrent =
				lnfHoraChecEntrada = lnfHoraChecSalida = lnfHoraChecEntrada2 = lnfHoraChecSalida2 =
				lnfHoraChecEntrada3 = lnfHoraChecSalida3 = lnfHoraChecEntrada4 = lnfHoraChecSalida4 =
				lnfHoraEntrada = lnfHoraSalida =
				lnfSalida4hrs = lnfWHorasNor = lnfWHorasLab = lnfWHorasExt =
				lnfDiferen = lnfRetardo =
				 lniWTipoEmp = lniWGrabar = 0;
				lnfWRestaTmpo = lnfWSumaTmpo = 0;

				#endregion

				lniCveEmp = Convert.ToInt32(0 + rowADP_HOR["clave_emp"].ToString().Trim());

				lfADPFechaMov = Convert.ToDateTime(rowADP_HOR["fecha_mov"]);
				lnfADPHorasNor = Convert.ToDecimal(rowADP_HOR["horas_nor"]);

				lnfWSumaTmpo = Convert.ToDecimal(rowADP_HOR["agregaTiempo"].ToString().Trim());
				lnfWRestaTmpo = Convert.ToDecimal(rowADP_HOR["quitaTiempo"].ToString().Trim());

				#region [OBTIENE HORAS ENTRADAS SALIDAS]
				lsWDia = FuncionesComunes.oDia_Char(lfADPFechaMov.DayOfWeek.ToString());

				lnfHoraEntrada = 0;
				switch (lsWDia.ToUpper().Trim())
				{
					case "LUNES":
						lnfHoraEntrada = Convert.ToDecimal(rowADP_HOR["lunesEntrada"]);
						lnfHoraSalida = Convert.ToDecimal(rowADP_HOR["lunesSalida"]);
						break;
					case "MARTES":
						lnfHoraEntrada = Convert.ToDecimal(rowADP_HOR["martesEntrada"]);
						lnfHoraSalida = Convert.ToDecimal(rowADP_HOR["martesSalida"]);
						break;
					case "MIERCOLES":
						lnfHoraEntrada = Convert.ToDecimal(rowADP_HOR["miercolesEntrada"]);
						lnfHoraSalida = Convert.ToDecimal(rowADP_HOR["miercolesSalida"]);
						break;
					case "JUEVES":
						lnfHoraEntrada = Convert.ToDecimal(rowADP_HOR["juevesEntrada"]);
						lnfHoraSalida = Convert.ToDecimal(rowADP_HOR["juevesSalida"]);
						break;
					case "VIERNES":
						lnfHoraEntrada = Convert.ToDecimal(rowADP_HOR["viernesEntrada"]);
						lnfHoraSalida = Convert.ToDecimal(rowADP_HOR["viernesSalida"]);
						break;
					case "SABADO":
						lnfHoraEntrada = Convert.ToDecimal(rowADP_HOR["sabadoEntrada"]);
						lnfHoraSalida = Convert.ToDecimal(rowADP_HOR["sabadoSalida"]);
						break;
					case "DOMINGO":
						lnfHoraEntrada = Convert.ToDecimal(rowADP_HOR["domingoEntrada"]);
						lnfHoraSalida = Convert.ToDecimal(rowADP_HOR["domingoSalida"]);
						break;
				}

				if (lnfHoraEntrada != -1.00M)
				{
					boolDiaHabil = true;

					if (lnfHoraSalida < lnfHoraEntrada)
					{
						lnfHoraSalida += 24;
					}

					lnfTextr = lnfHoraSalida + (((lniToleraSal + 1) * 1.66667M) / 100.00M);
					lnfTnextr = lnfHoraSalida + ((lniToleraSal * 1.66667M) / 100.00M);

					lnfMintexent = (((lniToleraEnt + 1) * 1.66667M) / 100.00M);
					lnfTextrent = lnfHoraEntrada - lnfMintexent;

					lnfMintnextent = ((lniToleraEnt * 1.66667M) / 100.00M);
					lnfTnextrent = lnfHoraEntrada - lnfMintnextent;
				}
				else
				{
					lnfHoraEntrada = 0.00M;
					lnfHoraSalida = 0.00M;

					boolDiaHabil = false;

					#region [ACTUALIZA ASIS_DIA_PERM]
					rowADP_HOR["horas_lab"] =
					rowADP_HOR["horas_ext"] =
					rowADP_HOR["horas_nor"] = 0.00;
					rowADP_HOR["observa"] = "";
					#endregion

					lnfADPHorasNor = 0.00M;
				}

				rowADP_HOR["tipo_mov"] = "N";

				lsHoraEnt = rowADP_HOR["hora_ent"].ToString().Trim();
				lsHoraSal = rowADP_HOR["hora_sal"].ToString().Trim();
				lsHoraEnt2 = rowADP_HOR["hora_ent2"].ToString().Trim();
				lsHoraSal2 = rowADP_HOR["hora_sal2"].ToString().Trim();
				lsHoraEnt3 = rowADP_HOR["hora_ent3"].ToString().Trim();
				lsHoraSal3 = rowADP_HOR["hora_sal3"].ToString().Trim();
				//NO SE UTILIZAN SEGUN CONDICIONES DE IF				
				//lsHoraEnt4 = rowADP_HOR["hora_ent4"].ToString().Trim();
				//lsHoraSal4 = rowADP_HOR["hora_sal4"].ToString().Trim();

				//TODO : PL SE ELIMINO ESTE FILTRO EN TODOS LOS IF YA QUE SE FILTRA DESDE QUERY A QUE LENTH SEA = 5 EN AMBOS CASOS lsHoraEnt.Length == 5 && lsHoraSal.Length == 5 &&
				lnfHoraChecEntrada = FuncionesComunes.obt_horario(lsHoraEnt);
				lnfHoraChecSalida = FuncionesComunes.obt_horario(lsHoraSal);

				if (lsHoraEnt2.Length < 5 && lsHoraSal2.Length < 5)
				{
					#region [PROCESO]
					//TODO : PL SE GENERALIZA PARA DEPURAR CODIGO 
					//lnfHoraChecEntrada = obt_horario(lsHoraEnt);
					//lnfHoraChecSalida = obt_horario(lsHoraSal);

					if (lnfHoraChecSalida < lnfHoraChecEntrada)
					{
						lnfHoraChecSalida += 24;
					}

					if (boolDiaHabil == true)
					{

						if (lnfHoraSalida > 24)
						{
							lnfHoraChecSalida += 24;
						}
						lnfSalida4hrs = lnfHoraSalida - lnfHoraChecSalida;
						if (lnfHoraChecSalida > 24)
						{
							lnfHoraChecSalida -= 24;
						}
					}
					#endregion
				}
				else if (lsHoraEnt2.Length == 5 && lsHoraSal2.Length == 5 && (lsHoraEnt3 == "" && lsHoraSal3 == ""))
				{
					#region [PROCESO]

					lnfHoraChecEntrada2 = FuncionesComunes.obt_horario(lsHoraEnt2);
					lnfHoraChecSalida2 = FuncionesComunes.obt_horario(lsHoraSal2);

					if (lnfHoraChecSalida < lnfHoraChecEntrada)
					{ lnfHoraChecSalida += 24; }

					if (lnfHoraChecSalida2 < lnfHoraChecEntrada2)
					{
						lnfHoraChecSalida2 += 24;
					}


					lnfSalida4hrs =
																																	 (lnfHoraChecSalida2 - lnfHoraChecEntrada2) +
																																	 (lnfHoraChecSalida - lnfHoraChecEntrada);


					//lnfSalida4hrs = lnfHoraSalida - lnfHoraChecSalida2;
					#endregion
				}
				#region [proceso obsoleto]
				//TODO: PL SE ELIMINAN ESTOS IF NUNCA ENTRARA AQUI SIEMPRE ENTRARA EN IF ANTERIOR ES EL MISMO CASO QUE ESTE
				else if (lsHoraEnt3.Length == 5 && lsHoraSal3.Length == 5)
				{
					#region [PROCESO]

					lnfHoraChecEntrada2 = FuncionesComunes.obt_horario(lsHoraEnt2);
					lnfHoraChecSalida2 = FuncionesComunes.obt_horario(lsHoraSal2);
					lnfHoraChecEntrada3 = FuncionesComunes.obt_horario(lsHoraEnt3);
					lnfHoraChecSalida3 = FuncionesComunes.obt_horario(lsHoraSal3);

					if (lnfHoraChecSalida < lnfHoraChecEntrada)
					{
						lnfHoraChecSalida += 24;
					}
					if (lnfHoraChecEntrada2 < lnfHoraChecEntrada)
					{
						lnfHoraChecEntrada2 += 24;
					}
					if (lnfHoraChecSalida2 < lnfHoraChecEntrada)
					{
						lnfHoraChecSalida2 += 24;
					}
					if (lnfHoraChecEntrada3 < lnfHoraChecEntrada)
					{
						lnfHoraChecEntrada3 += 24;
					}
					if (lnfHoraChecSalida3 < lnfHoraChecEntrada)
					{
						lnfHoraChecSalida3 += 24;
					}

					#endregion
				}
				//else if (lsHoraEnt2.Length == 5 && lsHoraSal2.Length == 5 && lsHoraEnt3.Length == 5 && lsHoraSal3.Length == 5 && lsHoraEnt4.Length == 5 && lsHoraSal4.Length == 5)
				//{
				//    #region [PROCESO]
				//    //TODO : PL SE GENERALIZA PARA DEPURAR CODIGO 
				//    //lnfHoraChecEntrada = obt_horario(lsHoraEnt);
				//    lnfHoraChecSalida = obt_horario(lsHoraSal);
				//    lnfHoraChecEntrada2 = obt_horario(lsHoraEnt2);
				//    lnfHoraChecSalida2 = obt_horario(lsHoraSal2);
				//    lnfHoraChecEntrada3 = obt_horario(lsHoraEnt3);
				//    lnfHoraChecSalida3 = obt_horario(lsHoraSal3);
				//    lnfHoraChecEntrada4 = obt_horario(lsHoraEnt4);
				//    lnfHoraChecSalida4 = obt_horario(lsHoraSal4);

				//    if (lnfHoraChecSalida < lnfHoraChecEntrada)
				//    {
				//        lnfHoraChecSalida += 24;
				//    }
				//    if (lnfHoraChecEntrada2 < lnfHoraChecEntrada)
				//    {
				//        lnfHoraChecEntrada2 += 24;
				//    }
				//    if (lnfHoraChecSalida2 < lnfHoraChecEntrada)
				//    {
				//        lnfHoraChecSalida2 += 24;
				//    }
				//    if (lnfHoraChecEntrada3 < lnfHoraChecEntrada)
				//    {
				//        lnfHoraChecEntrada3 += 24;
				//    }
				//    if (lnfHoraChecSalida3 < lnfHoraChecEntrada)
				//    {
				//        lnfHoraChecSalida3 += 24;
				//    }
				//    if (lnfHoraChecEntrada4 < lnfHoraChecEntrada)
				//    {
				//        lnfHoraChecEntrada4 += 24;
				//    }
				//    if (lnfHoraChecSalida4 < lnfHoraChecEntrada)
				//    {
				//        lnfHoraChecSalida4 += 24;
				//    }

				//    #endregion
				//}

				#endregion
				#endregion
				//TODO : NOMENCLATURA

				switch (FuncionesComunes.ObtenerTipoTrabajador(lniCveEmp))
				{
					//MODDIFICACION TOM
					case TipoTrabajador.Personal:
					case TipoTrabajador.TOMPersonal:
						#region [PERSONAL]

						lniWTipoEmp = 3;

						if (boolDiaHabil == true)
						{
							#region [DIA HABIL]
							lnfWHorasNor = lnfADPHorasNor;

							if (lnfHoraChecEntrada > 0 && lnfHoraChecEntrada < 2)
							{
								lnfHoraChecEntrada += 24;
								//PROCESO EMPLEADOS TIEMPO EXTRA ANTICIPADO
								if (lnfHoraEntrada > 0 && lnfHoraEntrada < 9)
								{
									lnfHoraEntrada += 24;
								}
							}

							if (lnfHoraChecEntrada < lnfTnextrent && lnfHoraChecEntrada >= 0 && lnfHoraChecEntrada <= 6 && lnfTnextrent >= 10)
							{
								lnfTnextrent += 24;
							}

							if (lnfHoraChecEntrada < 1 && lnfHoraChecSalida > 24)
							{
								lnfHoraChecEntrada += 24;
							}

							#region [DETERMINA SI TIENE TIEMPO EXTRA DE ENTRADA]
							lnfDiferen = Math.Round(lnfHoraEntrada - lnfHoraChecEntrada, 2);
							lnfRetardo = FuncionesComunes.oObtieneRetardoPaseLista(lnfHoraEntrada, lnfHoraChecEntrada);

							if (lnfRetardo >= 20)
							{
								lnfRetardo = 0;
							}

							//PROCESO EMPLEADOS TIEMPO EXTRA ANTICIPADO
							if (lnfHoraEntrada > 24)
							{
								lnfHoraEntrada -= 24;
							}

							if (lnfHoraChecEntrada < lnfHoraEntrada && lnfDiferen < lnfMintnextent)
							{
								lnfHoraChecEntrada = lnfHoraEntrada;
							}


							lnfDiferen = Math.Round(lnfHoraChecSalida - lnfHoraSalida, 2);

							if (lnfHoraChecSalida > lnfHoraSalida && lnfDiferen < lnfMintexent)
							{
								lnfHoraChecSalida = lnfHoraSalida;
							}

							#endregion

							#region [DETERMINA SI TIENE TIEMPO EXTRA DE SALIDA]

							lnfDiferen = Math.Round(lnfHoraChecSalida - lnfHoraSalida, 2);

							if (lnfHoraChecSalida > lnfHoraSalida && lnfDiferen < lnfMintexent)
							{
								lnfHoraChecSalida = lnfHoraSalida;
							}

							lnfDiferen = Math.Round(lnfHoraSalida - lnfHoraChecSalida, 2);

							if (lnfHoraChecSalida < lnfHoraSalida && lnfDiferen <= lnfMinutosTolera)
							{
								lnfHoraChecSalida = lnfHoraSalida;
							}

							if (rowADP_HOR["HORARIO"].ToString().Trim() != "54")
							{
								#region [NUEVO PROCESO ]
								switch (Convert.ToDateTime(rowADP_HOR["fecha_mov"]).DayOfWeek)
								{
									case DayOfWeek.Monday:
										lnfWHorasNor =
										 Convert.ToDecimal(rowADP_HOR["lunes2"]);
										break;
									case DayOfWeek.Tuesday:
										lnfWHorasNor
										 = Convert.ToDecimal(rowADP_HOR["martes2"]);
										break;
									case DayOfWeek.Wednesday:
										lnfWHorasNor
										 = Convert.ToDecimal(rowADP_HOR["miercoles2"]);
										break;
									case DayOfWeek.Thursday:
										lnfWHorasNor
										 = Convert.ToDecimal(rowADP_HOR["jueves2"]);
										break;
									case DayOfWeek.Friday:
										lnfWHorasNor
										 = Convert.ToDecimal(rowADP_HOR["viernes2"]);
										break;
									case DayOfWeek.Saturday:
										lnfWHorasNor
										 = Convert.ToDecimal(rowADP_HOR["sabado2"]);
										break;
									case DayOfWeek.Sunday:
										lnfWHorasNor
										= Convert.ToDecimal(rowADP_HOR["domingo2"]);
										break;
									default:
										lnfWHorasNor = 0M;
										break;
								}
								#endregion
							}


							lnfADPHorasNor = lnfWHorasNor;
							rowADP_HOR["horas_nor"] = lnfWHorasNor;

							#endregion

							#region [SE CALCULAN LAS HORAS LABORADAS ]
							if (lnfHoraChecSalida2 < lnfHoraChecEntrada2)
							{
								lnfHoraChecSalida2 += 24;
							}

							if (lnfHoraChecSalida3 < lnfHoraChecEntrada3)
							{
								lnfHoraChecSalida3 += 24;
							}

							if (lnfHoraChecEntrada < 1 && lnfHoraChecSalida > 24)
							{
								lnfHoraChecEntrada -= 24;
							}

							if (lnfHoraChecEntrada > lnfHoraChecSalida)
							{
								lnfHoraChecSalida += 24;
							}

							lnfWHorasLab = (decimal)oCALCULAR_TIEMPOS(lnfHoraChecEntrada, lnfHoraChecSalida,
																																																																							 lnfHoraChecEntrada2, lnfHoraChecSalida2,
																																																																							 lnfHoraChecEntrada3, lnfHoraChecSalida3,
																																																																							 lnfHoraChecEntrada4, lnfHoraChecSalida4,
																																																																							 lnfWSumaTmpo, lnfWRestaTmpo,
																																																																							 lniWTipoEmp, lnfWHorasNor, boolDiaHabil,
																																																																							 rowADP_HOR["clave_emp"].ToString().Trim());

							if (Math.Round(lnfWHorasLab, 2) >= Math.Round(lnfWHorasNor, 2))
							{
								lnfWHorasExt = lnfWHorasLab - lnfWHorasNor;
								lnfWHorasLab = lnfWHorasNor;
							}

							if (lnfHoraChecEntrada > 0 && lnfHoraChecEntrada < 2)
							{
								lnfHoraChecEntrada += 24;
							}

							if (lnfHoraChecEntrada > 24)
							{
								lnfHoraChecEntrada -= 24;
							}

							lnfRetardoRedondeado = Math.Round(lnfRetardo, 2);


							if ((lnfRetardoRedondeado > Math.Round(lnfMinutosTolera, 2) && lnfRetardoRedondeado <= 4) || (lnfRetardoRedondeado > 4 && lnfSalida4hrs < 4))
							{
								Graba_Retardos_PL(lnfRetardo, lnfMinutosTolera, lfADPFechaMov, rowADP_HOR["clave_emp"].ToString().Trim(),
																																				 ref tab_Faltas, ref tab_MotFals, ref lsHoraEnt, ref lsHoraSal, rowADP_HOR);

							}
							else if (lnfRetardoRedondeado > 4)
							{
								Graba_Retardos_PL(lnfRetardo, lnfMinutosTolera, lfADPFechaMov, rowADP_HOR["clave_emp"].ToString().Trim(),
																												 ref tab_Faltas, ref tab_MotFals, ref lsHoraEnt, ref lsHoraSal, rowADP_HOR);
							}
							else if (lnfSalida4hrs > 4 && ((lnfRetardoRedondeado > Math.Round(lnfMinutosTolera, 2) && lnfRetardoRedondeado <= 4) || lnfRetardoRedondeado > 4) && boolDiaHabil == true)
							{
								rowMotFals = tab_MotFals.Select("motivo = '101'");
								if (rowMotFals.Length > 0)
								{
									rowADP_HOR["observa"] = rowMotFals[0]["des_falta"].ToString().Trim();
									rowADP_HOR["mot_101"] = "1";
								}
							}


							if (lnfWHorasExt < .50M)
							{
								lnfWHorasExt = 0.00M;
							}

							if (lsHoraEnt.Trim() == "00:00")
							{
								rowADP_HOR["hora_ent"] = "24:00";
							}
							if (lsHoraSal.Trim() == "00:00")
							{
								rowADP_HOR["hora_sal"] = "24:00";
							}
							lsWTipoMov = "N";
							lniWGrabar = 1;
							#endregion

							#region [GRABA LAS HORAS LABORADAS ]
							//UTILILZA VALOR DE VARIABLE WObserva mas esta variable siempre sera = '' nunca cambia su valor 
							lniWGrabar = 1;
							Grabar_PL(lniWGrabar, lnfWHorasNor, lnfWHorasLab, lnfWHorasExt, lsWObserva, lsWTipoMov, rowADP_HOR);
							#endregion
							#endregion

						}
						else
						{
							#region [ DIA INHABIL  ]
							boolDiaHabil = false;
							lnfWSumaTmpo = 0;
							lnfWRestaTmpo = 0;
							lnfWHorasExt = (decimal)oCALCULAR_TIEMPOS(lnfHoraChecEntrada, lnfHoraChecSalida,
																																																																							 lnfHoraChecEntrada2, lnfHoraChecSalida2,
																																																																							 lnfHoraChecEntrada3, lnfHoraChecSalida3,
																																																																							 lnfHoraChecEntrada4, lnfHoraChecSalida4,
																																																																							 lnfWSumaTmpo, lnfWRestaTmpo,
																																																																							 lniWTipoEmp, lnfWHorasNor, boolDiaHabil,
																																																																							 rowADP_HOR["clave_emp"].ToString().Trim());

							lniWGrabar = 1;
							lsWTipoMov = "N";

							#region [GRABA LAS HORAS LABORADAS ]
							Grabar_PL(lniWGrabar, lnfWHorasNor, lnfWHorasLab, lnfWHorasExt, lsWObserva, lsWTipoMov, rowADP_HOR);
							#endregion
							#endregion
						}

						#endregion
						break;
					case TipoTrabajador.Honorarios:
					case TipoTrabajador.Practicante:
					case TipoTrabajador.Becate:
					case TipoTrabajador.TOMHonorarios:
					case TipoTrabajador.TOMPracticante:
					case TipoTrabajador.TOMBecate:




						#region [HONORARIOS -- BECATE -- PRACTICANTE]
						//CONSIDERA HONORARIOS -- BECATE - PRACTICANTE
						lsWTipoMov = "N";

						//AQUI BUSCA EL TIPO DE EMPLEADO SEGUN CLAVE 
						//PRACTICANTE = 1
						//HONORARIOS = 2 
						//BECATE = 2

						//TODO : NOMENCLATURA

						switch (FuncionesComunes.ObtenerTipoTrabajador(lniCveEmp))
						{
							case TipoTrabajador.Honorarios:
							case TipoTrabajador.Becate:
							case TipoTrabajador.TOMHonorarios:
							case TipoTrabajador.TOMBecate:
								lniWTipoEmp = 2;
								break;
							case TipoTrabajador.Practicante:
							case TipoTrabajador.TOMPracticante:
								lniWTipoEmp = 1;
								//PROCESO SOLO PARA PRACTICANTE
								lsWObserva = "";
								rowADP_HOR["observa"] = "";

								break;

						}
						//PROCESO SOLO PARA HONORARIOS -- PRACTICANTE
						if (boolDiaHabil)
						{
							switch (FuncionesComunes.ObtenerTipoTrabajador(lniCveEmp))
							{
								case TipoTrabajador.Honorarios:
								case TipoTrabajador.Practicante:
								case TipoTrabajador.TOMHonorarios:
								case TipoTrabajador.TOMPracticante:
									lnfWHorasNor = Convert.ToDecimal(rowADP_HOR["horas_nor"]);

									break;

							}
						}


						//PROCESO GENERAL 
						lnfWHorasLab = (decimal)oCALCULAR_TIEMPOS(lnfHoraChecEntrada, lnfHoraChecSalida,
																																																																						 lnfHoraChecEntrada2, lnfHoraChecSalida2,
																																																																						 lnfHoraChecEntrada3, lnfHoraChecSalida3,
																																																																						 lnfHoraChecEntrada4, lnfHoraChecSalida4,
																																																																						 lnfWSumaTmpo, lnfWRestaTmpo,
																																																																						 lniWTipoEmp, lnfWHorasNor, boolDiaHabil,
																																																																						 rowADP_HOR["clave_emp"].ToString().Trim());


						#region [GRABA LAS HORAS LABORADAS ]
						lniWGrabar = 1;
						Grabar_PL(lniWGrabar, lnfWHorasNor, lnfWHorasLab, lnfWHorasExt, lsWObserva, lsWTipoMov, rowADP_HOR);
						#endregion
						#endregion
						break;
						//case TipoTrabajador.Vigilante:						
						//case TipoTrabajador.ServicioMedico:						
						//case TipoTrabajador.Limpieza:						
						//case TipoTrabajador.Facilitador:						
						//case TipoTrabajador.Sistema:
						//case TipoTrabajador.NoDefinido:
						//default:
						//	break;
				}

				lniIndex += 1;

			}

			daTablas.Update(tab_AsisDiaPerm);

			#endregion

			//MODIFICACION DM  2017/07/28 ANEXO PROCESO PARA COMBINACION DE PERMISOS PARCIALES CON RETARDOS
			pasarListaPermisosParciales(psClaveEmp, pniWSFiscal, pniWAFiscal, ref conexion);

			#region [PASAR LISTA PARTE 5-5 , OTROS BLOQUE FINAL]
			cmdStoreProcedure = new SqlCommand("dbo.pa_PasarListaBloqueFinal", conexion);
			cmdStoreProcedure.CommandTimeout = 0;
			cmdStoreProcedure.CommandType = CommandType.StoredProcedure;
			cmdStoreProcedure.Parameters.Add("@pniWSFiscal", SqlDbType.Int).Value = pniWSFiscal;
			cmdStoreProcedure.Parameters.Add("@pniWAFiscal", SqlDbType.Int).Value = pniWAFiscal;
			cmdStoreProcedure.Parameters.Add("@Bimestre", SqlDbType.Int).Value = pniWBimestre;
			cmdStoreProcedure.Parameters.Add("@CEmp", SqlDbType.NVarChar).Value = psClaveEmp;
			cmdStoreProcedure.ExecuteNonQuery();
			#endregion

			//MODIFICACION DM  2017/07/28 ANEXO PROCESO PARA COMBINACION DE PERMISOS PARCIALES CON RETARDOS
			paselistaPermisosretardos(pniWSFiscal, pniWAFiscal, psClaveEmp, ref conexion);

			#region [CALCULO TIEMPO EXTRA]
			if (string.IsNullOrEmpty(psClaveEmp.Trim())) calc_tiempo_extra(pniWSFiscal, pniWAFiscal, ref conexion);
			#endregion

			FuncionesComunes.GuardarLog("Termina Pase de Lista.");
		}

		/// <summary>
		/// LIMPIA MOTIVOS UTILIZADA EN FUNCION PASAR_LISTA
		/// </summary>
		/// <param name="FiltroCmdAsisDiaPerm"></param> Filtro Tabla AsisDiaPerm
		/// <param name="paramCollection"></param>Collection de parametros para filtro 
		public void LIMPIA_MOTIVOS(ref SqlConnection conexion, string FiltroCmdAsisDiaPerm = "", SqlParameterCollection paramCollection = null)
		{
			if (conexion.State == ConnectionState.Closed) conexion.Open();
			SqlCommand actAsisDiaPerm = new SqlCommand();
			actAsisDiaPerm.Connection = conexion;
			////MODIFICACION ANEXO MOTIVOS COMBINADOS     2017/07/28
			actAsisDiaPerm.CommandText = "UPDATE asis_dia_perm SET mot_01 ='',mot_02 ='',mot_03 ='',mot_04 ='',mot_05 ='',mot_06 ='',mot_07 ='',mot_09 ='',mot_010 ='',mot_011 ='',mot_012 ='',mot_013 ='',mot_016 ='',mot_017='',mot_018 ='',mot_019 ='',mot_020 ='',mot_023 ='' ,mot_024 ='',mot_025 ='',mot_026 ='',mot_027 ='',mot_028 ='', mot_101 ='',fuera_turn = 0,observa = '',hrs_ext_au = 0.00,tesem = 0.00,tedesc = 0.00  ";

			if (!string.IsNullOrEmpty(FiltroCmdAsisDiaPerm.Trim()))
			{
				//ACTUALIZA REGISTROS SEGUN EL FILTRO SOLAMENTE
				#region [ACTUALIZA REGISTROS]
				actAsisDiaPerm.CommandText += " WHERE " + FiltroCmdAsisDiaPerm;

				#region [PARAM COLLECTION]
				SqlParameter[] param = new SqlParameter[paramCollection.Count];
				paramCollection.CopyTo(param, 0);
				paramCollection.Clear();
				actAsisDiaPerm.Parameters.AddRange(param);
				#endregion

				#endregion
			}
			actAsisDiaPerm.ExecuteNonQuery();
		}

		/// <summary>
		/// LIMPIA MOTIVOS UTILIZADA EN FR_ACTMARCAJES
		/// </summary>
		/// <param name="rowAsisDiaPerm"></param>
		public void LIMPIA_MOTIVOS(ref DataRow rowAsisDiaPerm)
		{
			rowAsisDiaPerm["mot_01"] = rowAsisDiaPerm["mot_02"] = rowAsisDiaPerm["mot_03"] =
			rowAsisDiaPerm["mot_04"] = rowAsisDiaPerm["mot_05"] = rowAsisDiaPerm["mot_06"] =
			rowAsisDiaPerm["mot_07"] = rowAsisDiaPerm["mot_09"] = rowAsisDiaPerm["mot_010"] =
			rowAsisDiaPerm["mot_011"] = rowAsisDiaPerm["mot_012"] = rowAsisDiaPerm["mot_013"] =
			rowAsisDiaPerm["mot_016"] = rowAsisDiaPerm["mot_017"] = rowAsisDiaPerm["mot_018"] =
			rowAsisDiaPerm["mot_019"] = rowAsisDiaPerm["mot_020"] = rowAsisDiaPerm["mot_023"] =
			rowAsisDiaPerm["mot_024"] = rowAsisDiaPerm["mot_025"] = rowAsisDiaPerm["mot_026"] =
			rowAsisDiaPerm["mot_027"] = rowAsisDiaPerm["mot_028"] =
			rowAsisDiaPerm["mot_101"] = "";
			rowAsisDiaPerm["observa"] = "";
			rowAsisDiaPerm["fuera_turn"] = 0;
			rowAsisDiaPerm["hrs_ext_au"] = rowAsisDiaPerm["tesem"] = rowAsisDiaPerm["tedesc"] = 0.00;
		}

		#region [INSERA AUSENTISMO]

		public void INSERTA_AUSENTISMO(string lsCveEmp, string lsWDepto, string lsWCategoria, string lsWTurno, decimal lnfWSueldo, DateTime lfWInicia, DateTime lfWTermina, string lsMotFal, int lniWWDias, string lsWSupervisor, string lsWPersona, string lsWDiasAsist, string lsWCvePago, int lniWGradoNiv, string lsWReferencia, int lniWAFiscal, string lsWHorario, bool boolWDiaHabil, int lniWBimestre, int lniWSFiscal, decimal lnfWHorasNor, string lsWContDiv, ref SqlConnection conexion)
		{
			//NOTA: Revisar parametros que no se utilicen
			try
			{
				if (boolWDiaHabil == true)
				{
					SqlCommand cmdTablas = new SqlCommand();
					cmdTablas = new SqlCommand("INSERT INTO FALTAS(clave_emp, fecha_fal, horas_fal, semana_fal, motivo_fal, " +
																		 "supervisor, personal, depto, categ, turno, sdo_integ, grado, cve_pago, " +
																		 "bimestre, activo, captura, tipo_capt, referencia, laborable, a_fiscal, cont_div) " +
																		 "VALUES(@clave_emp, @fecha_fal, @horas_fal, @semana_fal, @motivo_fal, " +
																		 "@supervisor, @personal, @depto, @categ, @turno, @sdo_integ, @grado, @cve_pago, " +
																		 "@bimestre, 1, @captura, 'A', 'SISTEMA', 1, @a_fiscal, @cont_div)", conexion);

					#region  VALORES PARAMETROS
					cmdTablas.Parameters.Add("@clave_emp", SqlDbType.NVarChar).Value = lsCveEmp;
					cmdTablas.Parameters.Add("@fecha_fal", SqlDbType.DateTime).Value = lfWInicia;
					cmdTablas.Parameters.Add("@semana_fal", SqlDbType.Int).Value = lniWSFiscal;
					cmdTablas.Parameters.Add("@motivo_fal", SqlDbType.NVarChar).Value = lsMotFal;
					cmdTablas.Parameters.Add("@supervisor", SqlDbType.NVarChar).Value = lsWSupervisor;
					cmdTablas.Parameters.Add("@personal", SqlDbType.NVarChar).Value = lsWPersona;
					cmdTablas.Parameters.Add("@depto", SqlDbType.NVarChar).Value = lsWDepto;
					cmdTablas.Parameters.Add("@Categ", SqlDbType.NVarChar).Value = lsWCategoria;
					cmdTablas.Parameters.Add("@turno", SqlDbType.NVarChar).Value = lsWTurno;
					cmdTablas.Parameters.Add("@sdo_integ", SqlDbType.Decimal).Value = lnfWSueldo;
					cmdTablas.Parameters.Add("@bimestre", SqlDbType.Int).Value = lniWBimestre;
					cmdTablas.Parameters.Add("@cve_pago", SqlDbType.NVarChar).Value = lsWCvePago;
					cmdTablas.Parameters.Add("@grado", SqlDbType.Int).Value = lniWGradoNiv;
					cmdTablas.Parameters.Add("@horas_fal", SqlDbType.Decimal).Value = lnfWHorasNor;
					cmdTablas.Parameters.Add("@a_fiscal", SqlDbType.Decimal).Value = lniWAFiscal;
					cmdTablas.Parameters.Add("@captura", SqlDbType.DateTime).Value = DateTime.Now;
					cmdTablas.Parameters.Add("@cont_div", SqlDbType.NVarChar).Value = lsWContDiv;
					#endregion

					if (conexion.State == ConnectionState.Closed) conexion.Open();
					cmdTablas.ExecuteNonQuery();
				}
			}
			catch (SqlException ex)
			{
				FuncionesComunes.GuardarLog(String.Format("PasaLista(2549) | ERROR INSERTA_AUSENTISMO | SQL | {0}", ex.Message));
				System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
				//FuncionesComunes.GuardaStackTraceLog(st);
				//MessageBox.Show(ex.Message, "MENSAJE DEL SISTEMA [LFS -- ERROR]", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				FuncionesComunes.GuardarLog(String.Format("PasaLista(2554) | ERROR INSERTA_AUSENTISMO | GENERAL | {0}", ex.Message));
				System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
				//FuncionesComunes.GuardaStackTraceLog(st);
				//MessageBox.Show(ex.Message, "MENSAJE DEL SISTEMA [LFS -- ERROR]", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#endregion

		#region [oCALCULAR TIEMPOS]

		public decimal oCALCULAR_TIEMPOS(decimal lnfHoraChecEntrada, decimal lnfHoraChecSalida, decimal lnfHoraChecEntrada2, decimal lnfHoraChecSalida2, decimal lnfHoraChecEntrada3, decimal lnfHoraChecSalida3, decimal lnfHoraChecEntrada4, decimal lnfHoraChecSalida4, decimal lnfSumaTiempo, decimal lnfRestaTiempo, int lniTipoEmp, decimal lfHorasNor, bool boolDiaHabil, string lsCveEmp)
		{
			//NOTA: horas_nor y cve_empl no se utilizan pero se declaran en los parametros         
			decimal lnfHorasTrab = 0;
			switch (lniTipoEmp)
			{
				case 1:
				case 2:
				case 3:
					lnfHorasTrab = (lnfHoraChecSalida4 - lnfHoraChecEntrada4) + (lnfHoraChecSalida3 - lnfHoraChecEntrada3) + (lnfHoraChecSalida2 - lnfHoraChecEntrada2) + (lnfHoraChecSalida - lnfHoraChecEntrada);
					switch (lniTipoEmp)
					{
						case 2:
						case 3:
							if (boolDiaHabil == true && lnfHorasTrab > 5) lnfHorasTrab = lnfHorasTrab - lnfRestaTiempo + lnfSumaTiempo;
							break;
					}
					break;
			}
			return (lnfHorasTrab);
		}

		#endregion

		public void Grabar(int lniGrabar, decimal lnfHrsNor, decimal lnfHrsLab, decimal lnfHrsExt, string lsObserva, string lsTipoMov, ref DataRow rowUpdate)
		{
			if (lniGrabar == 1)
			{
				rowUpdate["horas_lab"] = lnfHrsLab;
				rowUpdate["horas_ext"] = lnfHrsExt;
				rowUpdate["horas_nor"] = lnfHrsNor;
				rowUpdate["tipo_mov"] = lsTipoMov;
				rowUpdate["observa"] = lsObserva;
			}
			else
			{
				rowUpdate["observa"] = lsObserva;
			}
		}

		public void Graba_Retardos(decimal lnfRetardo, decimal lnfMinutosTolera, DateTime lfFec, string lsCveEmp, ref DataTable tab_Faltas, ref DataTable tab_MotFals, ref string lsHoraEnt, ref string lsHoraSal, ref DataRow rowUpdate)
		{
			string lsMovFal, lsMotfal, lsMotFal;
			DataRow[] rowMotFals;
			DataRow[] rowFaltas;

			if (Math.Round(lnfRetardo, 2) > Math.Round(lnfMinutosTolera, 2) && Math.Round(lnfRetardo, 2) <= 4)
			{
				lsMovFal = "010";
				#region [ACTUALIZA ASIS_DIA_PERM ]
				rowUpdate["observa"] = "RETARDO +5 MINUTOS";
				rowUpdate["mot_010"] = "1";
				#endregion
			}
			else if (Math.Round(lnfRetardo, 2) > 4)
			{
				rowFaltas = tab_Faltas.Select("fecha_fal = '" + lfFec.ToShortDateString() + "' AND clave_emp = '" + lsCveEmp + "' AND motivo_fal = '011' ");
				if (rowFaltas.Length > 0)
				{
					lsMotfal = rowFaltas[0]["motivo_fal"].ToString().Trim();
					lsMovFal = lsMotfal.Trim().Substring(1, 1) == "0" ? lsMotfal.Trim().Substring(1, 2) : lsMotfal.Trim().Substring(0, 3); ;

					#region [ACTUALIZA ASIS_DIA_PERM]
					if (lsMovFal.Contains("M") || lsMovFal.Contains("m")) rowUpdate[lsMovFal] = "1";
					else rowUpdate["mot_" + lsMovFal] = "1";

					#region [TIPO DE MOV]
					if (lsMotfal == "001" || lsMotfal == "003" || lsMotfal == "012" || lsMotfal == "002") rowUpdate["tipo_mov"] = "I";
					else if (lsMotfal == "004" || lsMotfal == "006" || lsMotfal == "011" || lsMotfal == "013") rowUpdate["tipo_mov"] = "P";
					else if (lsMotfal == "005" || lsMotfal == "016" || lsMotfal == "017" || lsMotfal == "018" || lsMotfal == "019" || lsMotfal == "020") rowUpdate["tipo_mov"] = "A";
					else if (lsMotfal == "009") rowUpdate["tipo_mov"] = "V";
					#endregion

					if (lsMotfal == "001" || lsMotfal == "003" || lsMotfal == "012" || lsMotfal == "002")
					{
						rowUpdate["hora_ent"] = "";
						rowUpdate["hora_sal"] = "";
						lsHoraEnt = lsHoraSal = "";
					}

					rowMotFals = tab_MotFals.Select("motivo = '" + lsMotfal + "'");
					if (rowMotFals.Length > 0) rowUpdate["observa"] = rowFaltas.Length > 0 ? rowMotFals[0]["des_falta"].ToString().Trim() : "";
					else rowUpdate["observa"] = "";
					#endregion
				}
				else
				{
					lsMotFal = "101";
					lsMotfal = "101";
					lsMovFal = "101";
					#region [ACTUALIZA ASIS_DIA_PERM]
					rowUpdate["observa"] = "RETARDO +4 HORAS";
					rowUpdate["mot_101"] = "1";
					rowUpdate["fuera_turn"] = "1";
					#endregion
				}
			}
		}

		public void Grabar_PL(int lniGrabar, decimal lnfHrsNor, decimal lnfHrsLab, decimal lnfHrsExt, string lsObserva, string lsTipoMov, DataRow rowUpdate)
		{
			rowUpdate["horas_lab"] = lnfHrsLab;
			rowUpdate["horas_ext"] = lnfHrsExt;
			rowUpdate["horas_nor"] = lnfHrsNor;
			rowUpdate["tipo_mov"] = lsTipoMov;
		}

		public void Graba_Retardos_PL(decimal lnfRetardo, decimal lnfMinutosTolera, DateTime lfFec, string lsCveEmp, ref DataTable tab_Faltas, ref DataTable tab_MotFals, ref string lsHoraEnt, ref string lsHoraSal, DataRow rowUpdate)
		{
			string lsMovFal, lsMotfal, lsMotFal;
			DataRow[] rowMotFals;
			DataRow[] rowFaltas;

			if (Math.Round(lnfRetardo, 2) > Math.Round(lnfMinutosTolera, 2) && Math.Round(lnfRetardo, 2) <= 4)
			{
				lsMovFal = "010";
				#region [ACTUALIZA ASIS_DIA_PERM ]
				rowUpdate["observa"] = "RETARDO +5 MINUTOS";
				rowUpdate["mot_010"] = "1";
				#endregion
			}
			else if (Math.Round(lnfRetardo, 2) > 4)
			{
				rowFaltas = tab_Faltas.Select("fecha_fal = '" + lfFec.ToShortDateString() + "' AND clave_emp = '" + lsCveEmp + "' AND motivo_fal = '011' ");
				if (rowFaltas.Length > 0)
				{
					lsMotfal = rowFaltas[0]["motivo_fal"].ToString().Trim();
					lsMovFal = lsMotfal.Trim().Substring(1, 1) == "0" ? lsMotfal.Trim().Substring(1, 2) : lsMotfal.Trim().Substring(0, 3); ;

					#region [ACTUALIZA ASIS_DIA_PERM]
					rowUpdate[lsMovFal] = "1";

					#region [TIPO DE MOV]
					if (lsMotfal == "001" || lsMotfal == "003" || lsMotfal == "012" || lsMotfal == "002") rowUpdate["tipo_mov"] = "I";
					else if (lsMotfal == "004" || lsMotfal == "006" || lsMotfal == "011" || lsMotfal == "013") rowUpdate["tipo_mov"] = "P";
					else if (lsMotfal == "005" || lsMotfal == "016" || lsMotfal == "017" || lsMotfal == "018" || lsMotfal == "019" || lsMotfal == "020") rowUpdate["tipo_mov"] = "A";
					else if (lsMotfal == "009") rowUpdate["tipo_mov"] = "V";
					#endregion

					if (lsMotfal == "001" || lsMotfal == "003" || lsMotfal == "012" || lsMotfal == "002")
					{
						rowUpdate["hora_ent"] = "";
						rowUpdate["hora_sal"] = "";
						lsHoraEnt = lsHoraSal = "";
					}
					rowMotFals = tab_MotFals.Select("motivo = '" + lsMotfal + "'");
					if (rowMotFals.Length > 0) rowUpdate["observa"] = rowFaltas.Length > 0 ? rowMotFals[0]["des_falta"].ToString().Trim() : "";
					else rowUpdate["observa"] = "";
					#endregion
				}
				else
				{
					lsMotFal = "101"; //NOTA: Revisar uso de variables con nombres parecidos, si no es que iguales (diferencia de mayusculas)
					lsMotfal = "101";
					lsMovFal = "101";
					#region [ACTUALIZA ASIS_DIA_PERM]
					rowUpdate["observa"] = "RETARDO +4 HORAS";
					rowUpdate["mot_101"] = "1";
					rowUpdate["fuera_turn"] = "1";
					#endregion
				}
			}
		}

		public void calc_tiempo_extra(int lniWSFiscal, int lniWAFiscal, ref SqlConnection conexion)
		{
			SqlCommand cmdCommand = new SqlCommand();
			SqlCommandBuilder cmdBuilder = new SqlCommandBuilder();
			SqlDataAdapter daTab = new SqlDataAdapter();
			DataTable tab_AsisDiaPerm = new DataTable();
			DataTable tab_Textra = new DataTable();
			DataRow rowUpdate;
			string lsWCveEmp = "";
			double lnfWTotalExtra, lnfWHorasExtra1, lnfWExtra, lnfUt, lnfWTesem, lnfWTedesc, lnfWHoras;
			int lniIndex;
			lnfWTotalExtra = lnfWExtra = lnfUt = lnfWTesem = lnfWTedesc = 0;
			DateTime lfWFecha;
			if (conexion.State == ConnectionState.Closed) conexion.Open();

			#region [CARGA TEXTRA]
			cmdCommand = new SqlCommand("SELECT id_Textra, clave_emp, HORAS, hor_dob, hor_tri, hrs_aut, hrs_autd, semana, estado, acepta, " +
																	"tipo_jorn, CONVERT(DATE, entra) AS entra, CONVERT(DATE, sale) AS sale " +
																	"FROM textra WHERE semana=@SFiscal AND horas>0 ORDER BY clave_emp", conexion);
			cmdCommand.Parameters.Add("@SFiscal", SqlDbType.Int).Value = lniWSFiscal;
			daTab.SelectCommand = cmdCommand;
			tab_Textra.Clear();
			daTab.Fill(tab_Textra);
			#endregion

			DataTable tab_AsisDiaPerm2 = new DataTable();
			DataRow[] rowADP2;

			#region [CARGA ASIS_DIA_PERM]
			cmdCommand = new SqlCommand();
			cmdCommand.Connection = conexion;
			cmdCommand.CommandText = "UPDATE asis_dia_perm SET hrs_ext2_au=0, hrs_ext3_au=0 " +
															 "WHERE s_fiscal=@SFiscal AND a_fiscal=@AFiscal " +
															 "AND tipo_proc=0 AND horas_ext>=.50 " +
															 "AND (RTRIM(LTRIM(tipo_mov))='N' OR RTRIM(LTRIM(tipo_mov))='T')";
			cmdCommand.Parameters.Add("@SFiscal", SqlDbType.Int).Value = lniWSFiscal;
			cmdCommand.Parameters.Add("@AFiscal", SqlDbType.Int).Value = lniWAFiscal;
			cmdCommand.ExecuteNonQuery();

			#region [LLENA ASIS_DIA_PERM2]
			tab_AsisDiaPerm2.Columns.Add("tesem", System.Type.GetType("System.Decimal"));
			tab_AsisDiaPerm2.Columns.Add("tedesc", System.Type.GetType("System.Decimal"));
			tab_AsisDiaPerm2.Columns.Add("hrs_ext_au", System.Type.GetType("System.Decimal"));
			tab_AsisDiaPerm2.Columns["tesem"].DefaultValue = 0.00;
			tab_AsisDiaPerm2.Columns["tedesc"].DefaultValue = 0.00;
			tab_AsisDiaPerm2.Columns["hrs_ext_au"].DefaultValue = 0.00;

			cmdCommand = new SqlCommand("SELECT id_ADP, clave_emp, tipo_mov, s_fiscal, a_fiscal " +
																	"FROM asis_dia_perm " +
																	"WHERE a_fiscal=@pniWAFiscal AND s_fiscal=@pniWSFiscal AND grado_niv<=10 " +
																	"AND (RTRIM(LTRIM(tipo_mov))='N' OR RTRIM(LTRIM(tipo_mov))='T') " +
																	"AND tipo_proc=0 AND horas_ext>=.51 " +
																	"AND CONVERT(INT, clave_emp) IN (" +
																																	"SELECT distinct(clave_Emp) FROM TEXTRA " +
																																	"WHERE semana=@pniWSFiscal AND horas>0" +
																																	") " +
																	"ORDER BY clave_emp", conexion);
			cmdCommand.Parameters.Add("@pniWSFiscal", SqlDbType.Int).Value = lniWSFiscal;
			cmdCommand.Parameters.Add("@pniWAFiscal", SqlDbType.Int).Value = lniWAFiscal;
			daTab.SelectCommand = cmdCommand;
			tab_AsisDiaPerm2.Clear();
			daTab.Fill(tab_AsisDiaPerm2);
			#endregion

			cmdCommand.Parameters.Clear();
			cmdCommand = new SqlCommand();
			cmdCommand.Connection = conexion;
			cmdCommand.CommandText = "SELECT A.id_ADP, A.clave_emp, A.fecha_mov" +
															 ", A.horas_ext, A.hrs_ext_au, A.hrs_ext2_au, A.hrs_ext3_au, tesem, tedesc" +
															 ", RTRIM(LTRIM(A.tipo_mov)) AS tipo_mov, A.diasem" +
															 ", V.DescansoLaborado " +
															 "FROM ASIS_DIA_PERM AS A " +
															 "LEFT JOIN vistaDiasDescansoPorHorario AS V ON A.horario=V.horario " +
															 "WHERE A.s_fiscal=@SFiscal AND A.a_fiscal=@AFiscal " +
															 "AND (RTRIM(LTRIM(A.tipo_mov))='N' OR RTRIM(LTRIM(A.tipo_mov))='T') " +
															 "AND A.tipo_proc=0 AND A.horas_ext>=.51" +
															 "AND CONVERT(INT, A.clave_emp) IN (" +
																																	"SELECT distinct(clave_Emp) FROM TEXTRA " +
																																	"WHERE semana=@SFiscal AND horas>0" +
																																	") " +
															 "ORDER BY A.fecha_mov, A.clave_emp";
			cmdCommand.Parameters.Add("@SFiscal", SqlDbType.Int).Value = lniWSFiscal;
			cmdCommand.Parameters.Add("@AFiscal", SqlDbType.Int).Value = lniWAFiscal;
			daTab.SelectCommand = cmdCommand;
			tab_AsisDiaPerm.Clear();
			daTab.Fill(tab_AsisDiaPerm);
			cmdBuilder.DataAdapter = daTab;
			#endregion

			lniIndex = 0;
			foreach (DataRow rowADP_HOR in tab_AsisDiaPerm.Rows)
			{
				lnfWHoras = Convert.ToDouble(rowADP_HOR["horas_ext"]);
				rowADP2 = tab_AsisDiaPerm2.Select("id_ADP = '" + rowADP_HOR["id_ADP"] + "'");
				rowUpdate = tab_AsisDiaPerm.Rows[lniIndex];
				lsWCveEmp = rowADP_HOR["clave_emp"].ToString().Trim();
				lfWFecha = Convert.ToDateTime(rowADP_HOR["fecha_mov"]);
				//if (lsWCveEmp == "01571")
				//{
				//    string sbreak = ""; 
				//}
				lnfWExtra = 0;
				lnfWTotalExtra = Convert.ToDouble(0 + tab_AsisDiaPerm2.Compute("SUM(hrs_ext_au)", "clave_emp  = '" + lsWCveEmp + "' AND TRIM(tipo_mov) <> 'T' ").ToString().Trim());
				lnfWHorasExtra1 = Convert.ToDouble(0 + tab_Textra.Compute("SUM(horas)", "clave_emp = '" + lsWCveEmp + "' AND entra  = '" + lfWFecha.Date + "'  AND horas > 0  ").ToString());
				foreach (DataRow rowTextra in tab_Textra.Select("clave_emp = '" + lsWCveEmp + "' AND  entra = '" + lfWFecha.Date + "' "))
				{
					rowUpdate["hrs_ext2_au"] = rowUpdate["hrs_ext3_au"] = 0.00;
					rowADP2[0]["hrs_ext_au"] = rowUpdate["hrs_ext_au"] = lnfWHoras > lnfWHorasExtra1 ? lnfWHorasExtra1 : lnfWHoras;
					if (rowADP_HOR["tipo_mov"].ToString() != "T")
					{
						if (rowADP_HOR["diasem"].ToString().Trim().ToUpper() == rowADP_HOR["DescansoLaborado"].ToString().Trim().ToUpper())
						{
							rowADP2[0]["tedesc"] = rowUpdate["tedesc"] = rowUpdate["hrs_ext3_au"] = lnfWHoras > lnfWHorasExtra1 ? lnfWHorasExtra1 : lnfWHoras;
							continue;
						}
						else if (lnfWHoras > lnfWHorasExtra1)
						{
							if ((lnfWTotalExtra + lnfWHorasExtra1) <= 9)
							{
								rowADP2[0]["tesem"] = rowUpdate["tesem"] = rowUpdate["hrs_ext2_au"] = lnfWHorasExtra1;
								continue;
							}
							else
							{
								if (lnfWHoras >= 9)
								{
									lnfWTesem = 9;
									lnfWTedesc = lnfWHoras - 9;
									lnfUt = Convert.ToDouble(0 + tab_AsisDiaPerm2.Compute("SUM(tesem)", "clave_emp = '" + lsWCveEmp + "' ").ToString().Trim());
									if ((lnfWTotalExtra + lnfWHorasExtra1) > 9)
									{
										lnfWTesem = (lnfUt <= 9) ? (9 - lnfUt) : 0;
										lnfWTedesc = lnfWHorasExtra1 - lnfWTesem;
										rowADP2[0]["tesem"] = rowUpdate["tesem"] = rowUpdate["hrs_ext2_au"] = lnfWTesem;
										rowADP2[0]["tedesc"] = rowUpdate["tedesc"] = rowUpdate["hrs_ext3_au"] = lnfWTedesc;
										continue;
									}
									else if ((lnfWTotalExtra + lnfWHorasExtra1) <= 9)
									{
										rowADP2[0]["tesem"] = rowUpdate["tesem"] = rowUpdate["hrs_ext2_au"] = lnfWHoras;
										continue;
									}
									else
									{
										if ((lnfWTotalExtra + lnfWHoras) > 9)
										{
											lnfUt = Convert.ToDouble(0 + tab_AsisDiaPerm2.Compute("SUM(tesem)", "clave_emp = '" + lsWCveEmp + "' ").ToString().Trim());
											lnfWTesem = lnfUt <= 9 ? (9 - lnfUt) : 0;
											lnfWTedesc = lnfWHoras - lnfWTesem;
											rowADP2[0]["tesem"] = rowUpdate["tesem"] = rowUpdate["hrs_ext2_au"] = lnfWTesem;
											rowADP2[0]["tedesc"] = rowUpdate["tedesc"] = rowUpdate["hrs_ext3_au"] = lnfWTedesc;
											continue;
										}
										else
										{
											rowUpdate["tesem"] = rowADP2[0]["tesem"] = lnfWHoras;
											continue;
										}
									}
								}
							}
						}
						else if (lnfWHoras <= lnfWHorasExtra1)
						{
							if ((lnfWTotalExtra + lnfWHoras) <= 9)
							{
								rowADP2[0]["tesem"] = rowUpdate["tesem"] = rowUpdate["hrs_ext2_au"] = lnfWHoras;
								continue;
							}
							else if ((lnfWTotalExtra + lnfWHoras) > 9)
							{
								lnfWTesem = 9 - lnfWTotalExtra;
								if (lnfWTesem < 0) lnfWTesem = 0; //NOTA: Revisar esta linea y dos mas abajo. Las dos incluyen lnfWTesem
								if (lnfWHoras > lnfWHorasExtra1) lnfWHoras = lnfWHorasExtra1;
								if (lnfWTesem == 0) lnfWTedesc = lnfWHoras;
								else if ((lnfWTotalExtra + lnfWHoras) > 9) lnfWTedesc = lnfWHoras - lnfWTesem;
								rowADP2[0]["tesem"] = rowUpdate["tesem"] = rowUpdate["hrs_ext2_au"] = lnfWTesem;
								rowADP2[0]["tedesc"] = rowUpdate["tedesc"] = rowUpdate["hrs_ext3_au"] = lnfWTedesc;
								continue;
							}
						}
					}
				}
				lniIndex += 1;
			}
			daTab.Update(tab_AsisDiaPerm);
		}

		public void FestivosTrabajados()
		{

			SqlDataAdapter daTablas = new SqlDataAdapter();
			SqlCommand cmdTabla = new SqlCommand();
			SqlCommandBuilder cmdBuilder = new SqlCommandBuilder();
			SqlParameterCollection paramCollection = new SqlCommand().Parameters;
			DataTable tab_AsisDiaPerm = new DataTable();
			DataRow rowUpdate;

			string lsHoraEnt, lsHoraSal, lsHoraEnt2, lsHoraSal2, lsHoraEnt3, lsHoraSal3, lsHoraEnt4, lsHoraSal4, lsRegresaCont, lsWCont1, lsWCont2;
			int lniWContador, lniWContadorSigno;

			decimal lnfHoraChecEntrada, lnfHoraChecSalida, lnfHoraChecEntrada2, lnfHoraChecSalida2,
						 lnfHoraChecEntrada3, lnfHoraChecSalida3, lnfHoraChecEntrada4, lnfHoraChecSalida4,
						 lnfWHorasExt;
			int lniIndex = 0;
			if (conexion.State == ConnectionState.Closed) conexion.Open();

			#region [ELIMINA FALTAS DE SISTEMA]
			cmdTabla = new SqlCommand();
			cmdTabla.Connection = conexion;
			cmdTabla.CommandText = " DELETE faltas FROM faltas F ";
			cmdTabla.CommandText += " INNER JOIN asis_dia_perm A ON F.clave_emp = A.clave_emp AND F.fecha_fal = A.fecha_mov ";
			cmdTabla.CommandText += " WHERE F.semana_fal = @SFiscal AND F.a_fiscal = @AFiscal AND RTRIM(LTRIM(F.referencia)) = 'SISTEMA' AND  A.s_fiscal = @SFiscal AND A.a_fiscal = @AFiscal AND RTRIM(LTRIM(A.tipo_mov )) = 'T' ";

			if (!string.IsNullOrEmpty(psClaveEmp.Trim()))
			{
				cmdTabla.CommandText += " AND F.clave_emp  = @psClaveEmp ";
				cmdTabla.Parameters.Add("@psClaveEmp", SqlDbType.NVarChar).Value = psClaveEmp;
			}

			cmdTabla.Parameters.Add("@SFiscal", SqlDbType.Int).Value = pniWSFiscal;
			cmdTabla.Parameters.Add("@AFiscal", SqlDbType.Int).Value = pniWAFiscal;

			cmdTabla.ExecuteNonQuery();
			cmdTabla.Parameters.Clear();
			#endregion

			#region [CARGA ASIS_DIA_PERM]

			#region [SELECT QUERY]
			cmdTabla = new SqlCommand();
			cmdTabla.Connection = conexion;

			cmdTabla.CommandText = " SELECT id_ADP , clave_emp,fecha_mov , horas_lab , horas_ext , horas_nor , observa , tipo_mov, ";
			cmdTabla.CommandText += " hora_ent , hora_sal, hora_ent2 , hora_sal2, hora_ent3 , hora_sal3, hora_ent4 , hora_sal4 ";
			cmdTabla.CommandText += " FROM ASIS_DIA_PERM ";
			cmdTabla.CommandText += " WHERE ";
			cmdTabla.CommandText += " s_fiscal = @SFiscal AND a_fiscal = @AFiscal AND RTRIM(LTRIM(tipo_mov)) = 'T' ";

			#endregion

			#region [FILTRO ASIS_DIA_PERM]
			if (string.IsNullOrEmpty(psClaveEmp.Trim()))
			{
				cmdTabla.CommandText += " ";
				#region [LIMPIA MOTIVOS]
				#region [PARAM COLLECTION]
				paramCollection.Clear();
				paramCollection.Add("@pniWSFiscal", SqlDbType.Int).Value = pniWSFiscal;
				paramCollection.Add("@pniWAFiscal", SqlDbType.Int).Value = pniWAFiscal;
				#endregion

				LIMPIA_MOTIVOS(ref conexion, " s_fiscal = @pniWSFiscal AND a_fiscal = @pniWAFiscal  AND  RTRIM(LTRIM(tipo_mov)) = 'T' ", paramCollection);
				#endregion
			}
			else
			{
				cmdTabla.CommandText += " AND clave_emp = @psClaveEmp";
				cmdTabla.Parameters.Add("@psClaveEmp", SqlDbType.NVarChar).Value = psClaveEmp;
				#region [LIMPIA MOTIVOS]
				#region [PARAM COLLECTION]
				paramCollection.Clear();
				paramCollection.Add("@pniWSFiscal", SqlDbType.Int).Value = pniWSFiscal;
				paramCollection.Add("@pniWAFiscal", SqlDbType.Int).Value = pniWAFiscal;
				paramCollection.Add("@psClaveEmp", SqlDbType.NVarChar).Value = psClaveEmp;
				#endregion

				LIMPIA_MOTIVOS(ref conexion, " s_fiscal = @pniWSFiscal AND a_fiscal = @pniWAFiscal AND clave_emp = @psClaveEmp AND RTRIM(LTRIM(tipo_mov)) = 'T' ", paramCollection);
				#endregion
			}

			cmdTabla.CommandText += " ORDER BY fecha_mov, clave_emp ";

			cmdTabla.Parameters.Add("@SFiscal", SqlDbType.Int).Value = pniWSFiscal;
			cmdTabla.Parameters.Add("@AFiscal", SqlDbType.Int).Value = pniWAFiscal;
			daTablas.SelectCommand = cmdTabla;

			tab_AsisDiaPerm.Clear();
			daTablas.Fill(tab_AsisDiaPerm);
			#endregion

			#endregion

			cmdBuilder.DataAdapter = daTablas;

			foreach (DataRow rowADP in tab_AsisDiaPerm.Rows)
			{
				rowUpdate = tab_AsisDiaPerm.Rows[lniIndex];

				lnfHoraChecEntrada = lnfHoraChecSalida = lnfHoraChecEntrada2 = lnfHoraChecSalida2 =
				lnfHoraChecEntrada3 = lnfHoraChecSalida3 = lnfHoraChecEntrada4 = lnfHoraChecSalida4 = 0.00M;

				rowUpdate["horas_lab"] =
				rowUpdate["horas_ext"] =
				rowUpdate["horas_nor"] = 0.00;
				rowUpdate["observa"] = "";

				lsHoraEnt = rowADP["hora_ent"].ToString().Trim();
				lsHoraSal = rowADP["hora_sal"].ToString().Trim();
				lsHoraEnt2 = rowADP["hora_ent2"].ToString().Trim();
				lsHoraSal2 = rowADP["hora_sal2"].ToString().Trim();
				lsHoraEnt3 = rowADP["hora_ent3"].ToString().Trim();
				lsHoraSal3 = rowADP["hora_sal3"].ToString().Trim();
				lsHoraEnt4 = rowADP["hora_ent4"].ToString().Trim();
				lsHoraSal4 = rowADP["hora_sal4"].ToString().Trim();

				lsRegresaCont = FuncionesComunes.oContadores(lsHoraEnt, lsHoraSal, lsHoraEnt2, lsHoraSal2, lsHoraEnt3, lsHoraSal3, lsHoraSal4, lsHoraEnt4);

				lsWCont1 = lsRegresaCont.PadRight(1, ' ').Substring(0, 1).Trim();
				lsWCont2 = lsRegresaCont.PadRight(2, ' ').Substring(0, 2).Trim();

				lniWContador = Convert.ToInt16(0 + lsWCont1);
				lniWContadorSigno = Convert.ToInt16(0 + lsWCont2);


				if (lniWContadorSigno > 0) continue;
				else
				{
					lnfHoraChecEntrada = FuncionesComunes.obt_horario(lsHoraEnt);
					lnfHoraChecSalida = FuncionesComunes.obt_horario(lsHoraSal);
					if (lnfHoraChecSalida < lnfHoraChecEntrada) lnfHoraChecSalida += 24;

					switch (lniWContador)
					{
						case 2:
							lnfHoraChecEntrada2 =
							lnfHoraChecSalida2 =
							lnfHoraChecEntrada3 =
							lnfHoraChecSalida3 =
							lnfHoraChecEntrada4 =
							lnfHoraChecSalida4 = 0.00M;
							break;
						case 4:
							lnfHoraChecEntrada2 = FuncionesComunes.obt_horario(lsHoraEnt2);
							lnfHoraChecSalida2 = FuncionesComunes.obt_horario(lsHoraSal2);
							if (lnfHoraChecSalida < lnfHoraChecEntrada) lnfHoraChecSalida += 24;
							if (lnfHoraChecSalida2 < lnfHoraChecEntrada2) lnfHoraChecSalida2 += 24;
							lnfHoraChecEntrada3 =
							lnfHoraChecSalida3 =
							lnfHoraChecEntrada4 =
							lnfHoraChecSalida4 = 0.00M;
							break;
						case 6:
							lnfHoraChecEntrada3 = FuncionesComunes.obt_horario(lsHoraEnt3);
							lnfHoraChecSalida3 = FuncionesComunes.obt_horario(lsHoraSal3);
							lnfHoraChecEntrada4 =
							lnfHoraChecSalida4 = 0.00M;
							if (lnfHoraChecSalida3 < lnfHoraChecEntrada3) lnfHoraChecSalida3 += 24;
							break;
						case 8:
							lnfHoraChecEntrada4 = FuncionesComunes.obt_horario(lsHoraEnt4);
							lnfHoraChecSalida4 = FuncionesComunes.obt_horario(lsHoraSal4);
							if (lnfHoraChecSalida4 < lnfHoraChecEntrada4) lnfHoraChecSalida4 += 24;
							break;
					}
				}
				lnfWHorasExt = (decimal)oCALCULAR_TIEMPOS(
																				 lnfHoraChecEntrada,
																				 lnfHoraChecSalida,
																				 lnfHoraChecEntrada,
																				 lnfHoraChecSalida,
																				 lnfHoraChecEntrada,
																				 lnfHoraChecSalida,
																				 lnfHoraChecEntrada,
																				 lnfHoraChecSalida,
																				 0.00M, 0.00M, 1, 0.00M, true,
																				 rowADP["clave_emp"].ToString().Trim()
																			 );

				Grabar(1, 0.00M, 0.00M, lnfWHorasExt, "FESTIVO TRABAJADO", "T", ref rowUpdate);

				lniIndex += 1;
			}

			daTablas.Update(tab_AsisDiaPerm);
			FuncionesComunes.GuardarLog("Termina Pase de Festivos trabajados.");
		}

		public PasaListaNuevo(string fecha, ref SqlConnection cn)
		{
			conexion = cn;
			SqlDataAdapter daPeriodoActual = new SqlDataAdapter("SELECT s_fiscal AS semana, a_fiscal AS Anio, bimestre " +
																													"FROM PERIODOS WHERE fecha=@fechaActual", conexion);
			daPeriodoActual.SelectCommand.Parameters.Add(@"fechaActual", SqlDbType.Date).Value = Convert.ToDateTime(fecha).Date;
			DataTable dtPeriodoActual = new DataTable();
			if (daPeriodoActual.Fill(dtPeriodoActual) > 0)
			{
				pniWSFiscal = Convert.ToInt32(dtPeriodoActual.Rows[0]["semana"].ToString());
				pniWAFiscal = Convert.ToInt32(dtPeriodoActual.Rows[0]["anio"].ToString());
				pniWBimestre = Convert.ToInt32(dtPeriodoActual.Rows[0]["bimestre"].ToString());
				psClaveEmp = "";
			}

		}

	}
}
