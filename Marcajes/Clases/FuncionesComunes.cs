using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Xml;
using System.Diagnostics;
using static Marcajes.Clases.EnumsComunes;

namespace Marcajes.Clases
{
	class FuncionesComunes
	{
		
		static public void GuardaStackTraceLog(StackTrace stack)
		{
			System.Diagnostics.StackFrame[] sf = stack.GetFrames();
			string metodo = String.Format("Metodo: {0}", sf[sf.Length - 1].GetMethod());
			string archivo = String.Format("Archivo: {0}", System.IO.Path.GetFileName(sf[sf.Length - 1].GetFileName()));
			string linea = String.Format("Linea: {0}", sf[sf.Length - 1].GetFileLineNumber());
			GuardarLog(String.Format("{0}\r\n{1}\r\n{2}\r\n", metodo, archivo, linea));
		}

		static public void GuardarLog(string mensaje)
		{
			TextWriter tw = new StreamWriter(@"D:\FDS\LogDescargaMarcajes.txt", true);
			tw.WriteLine(String.Format("{0} | {1}", DateTime.Now, mensaje));
			tw.Close();
		}

		public static string oDia_Char(string lsNombreDiaSemana)
		{
			string lsDiaEsp = "";

			switch (lsNombreDiaSemana.ToUpper())
			{
				case "MONDAY":
				case "LUNES":
					lsDiaEsp = "Lunes";
					break;
				case "TUESDAY":
				case "MARTES":
					lsDiaEsp = "Martes";
					break;
				case "WEDNESDAY":
				case "MIERCOLES":
				case "MIÉRCOLES":
					lsDiaEsp = "Miercoles";
					break;
				case "THURSDAY":
				case "JUEVES":
					lsDiaEsp = "Jueves";
					break;
				case "FRIDAY":
				case "VIERNES":
					lsDiaEsp = "Viernes";
					break;
				case "SATURDAY":
				case "SABADO":
				case "SÁBADO":
					lsDiaEsp = "Sabado";
					break;
				case "SUNDAY":
				case "DOMINGO":
					lsDiaEsp = "Domingo";
					break;
			}

			return (lsDiaEsp);

		}

		/// <summary>
		/// Obtiene la hora de entrada del empelado añadiendole los minutos de tolerancia
		/// </summary>
		/// <param name="lsHora"></param>
		/// <returns></returns>
		public static decimal obt_horario(string lsHora)
		{
			decimal lnfHoras, lnfMinutos, lnfHoraConv;

			if ((lsHora.Trim() == "") || (lsHora.Trim() == ":"))
			{
				lsHora = "".PadLeft(5, ' ');
			}

			lnfHoras = (Convert.ToDecimal(0 + lsHora.Substring(0, 2).Trim()));
			lnfMinutos = (Convert.ToDecimal(0 + lsHora.Substring(3, 2).Trim()));
			lnfMinutos = Math.Round(((lnfMinutos * 1.66667M) / 100.00M), 2);
			lnfHoraConv = lnfHoras + lnfMinutos;

			return lnfHoraConv;
		}

		public static decimal oObtieneRetardoPaseLista(decimal lnfHoraDeEntrada, decimal lnfHoraChecEntrada)
		{
			decimal lnfRetardo, lnfHDE;
			if (lnfHoraDeEntrada >= 0 && lnfHoraDeEntrada < 3)
			{
				lnfHDE = lnfHoraDeEntrada + 24;
				lnfRetardo = lnfHoraChecEntrada - lnfHDE;
			}
			else
			{
				//if ((lnfHoraChecEntrada >= 0 && lnfHoraChecEntrada < 6) && lnfHoraDeEntrada >= 14)
				//{
				//    lnfHoraChecEntrada += 24;
				//}
				lnfRetardo = Math.Round(lnfHoraChecEntrada - lnfHoraDeEntrada, 2);
			}
			//if (lnfHoraChecEntrada > 24)
			//{
			//    lnfHoraChecEntrada -= 24;
			//}
			if (lnfRetardo < 0.1M || lnfRetardo < 0)
			{
				lnfRetardo = 0;
			}
			return lnfRetardo;
		}

		public static string oContadores(string lsHoraEnt, string lsHoraSal, string lsHoraEnt2, string lsHoraSal2, string lsHoraEnt3, string lsHoraSal3, string lsHoraEnt4, string lsHoraSal4)
		{
			int lniContador, lniContadorSigno;
			string lsTotCont = "";
			lniContador = lniContadorSigno = 0;

			if (lsHoraEnt.Trim().Length > 2)
			{
				lniContador += 1;
			}
			else if (lsHoraEnt.Trim() == "?")
			{
				lniContadorSigno += 1;
			}
			if (lsHoraSal.Trim().Length > 2)
			{
				lniContador += 1;
			}
			else if (lsHoraSal.Trim() == "?")
			{
				lniContadorSigno += 1;
			}
			if (lsHoraEnt2.Trim().Length > 2)
			{
				lniContador += 1;
			}
			else if (lsHoraEnt2.Trim() == "?")
			{
				lniContadorSigno += 1;
			}
			if (lsHoraSal2.Trim().Length > 2)
			{
				lniContador += 1;
			}
			else if (lsHoraSal2.Trim() == "?")
			{
				lniContadorSigno += 1;
			}
			if (lsHoraEnt3.Trim().Length > 2)
			{
				lniContador += 1;
			}
			else if (lsHoraEnt3.Trim() == "?")
			{
				lniContadorSigno += 1;
			}
			if (lsHoraSal3.Trim().Length > 2)
			{
				lniContador += 1;
			}
			else if (lsHoraSal3.Trim() == "?")
			{
				lniContadorSigno += 1;
			}
			if (lsHoraEnt4.Trim().Length > 2)
			{
				lniContador += 1;
			}
			else if (lsHoraEnt4.Trim() == "?")
			{
				lniContadorSigno += 1;
			}
			if (lsHoraSal4.Trim().Length > 2)
			{
				lniContador += 1;
			}
			else if (lsHoraSal4.Trim() == "?")
			{
				lniContadorSigno += 1;
			}
			lsTotCont = lniContador.ToString() + lniContadorSigno.ToString();   // en vfp  le asignan un resultado string a wtotcont  cuando lo declaran como  un numeric asignandole un  valor inicial de 0                        
			return (lsTotCont);
		}

		public static TipoTrabajador ObtenerTipoTrabajador(int clave)
		{
			TipoTrabajador respuesta = TipoTrabajador.NoDefinido;

			if (clave > 0)
			{

				if (clave < (int)TipoTrabajador.TOMPersonal) respuesta = TipoTrabajador.Personal;               //00000-79999 (80000)
				else if (clave < (int)TipoTrabajador.TOMHonorarios) respuesta = TipoTrabajador.TOMPersonal;     //80000-87999 ( 8000)                                
				else if (clave < (int)TipoTrabajador.TOMPracticante) respuesta = TipoTrabajador.TOMHonorarios;  //88000-88499 (  500)
				else if (clave < (int)TipoTrabajador.TOMBecate) respuesta = TipoTrabajador.TOMPracticante;      //88500-88999 (  500)
				else if (clave < (int)TipoTrabajador.Honorarios) respuesta = TipoTrabajador.TOMBecate;          //89000-89999 ( 1000)
				else if (clave < (int)TipoTrabajador.Practicante) respuesta = TipoTrabajador.Honorarios;        //90000-90999 ( 1000)
				else if (clave < (int)TipoTrabajador.Vigilante) respuesta = TipoTrabajador.Practicante;         //91000-93999 ( 3000)
				else if (clave < (int)TipoTrabajador.Becate) respuesta = TipoTrabajador.Vigilante;              //94000-94999 ( 1000)
				else if (clave < (int)TipoTrabajador.ServicioMedico) respuesta = TipoTrabajador.Becate;         //95000-97999 ( 3000)
				else if (clave < (int)TipoTrabajador.Limpieza) respuesta = TipoTrabajador.ServicioMedico;       //98000-98499 (  500)
				else if (clave < (int)TipoTrabajador.Facilitador) respuesta = TipoTrabajador.Limpieza;          //98500-99499 ( 1000)
				else if (clave < (int)TipoTrabajador.Sistema) respuesta = TipoTrabajador.Facilitador;           //99500-99899 (  400)
				else respuesta = TipoTrabajador.Sistema;                                                        //99900-99999 (  100)

			}
			return respuesta;
		}

	}
}
