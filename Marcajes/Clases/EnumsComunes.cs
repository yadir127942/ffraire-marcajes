namespace Marcajes.Clases
{
	class EnumsComunes
	{
		public enum TipoLog
		{
			Proceso
				, Error
		};

		public enum TipoTrabajador
		{
			NoDefinido = 0,
			Personal = 1,
			TOMPersonal = 80000,
			Honorarios = 90000,
			TOMHonorarios = 88000,
			Practicante = 91000,
			TOMPracticante = 88500,
			Vigilante = 94000,
			Becate = 95000,
			TOMBecate = 89000,
			ServicioMedico = 98000,
			Limpieza = 98500,
			Facilitador = 99500,
			TOMFacilitador = -3,
			Sistema = 99900
		}

		public enum TipoMotivos
		{
			SinIncidencia = 0,
			Retardo5Min = 10,
			PermisoParcialMayor4Horas = 11,
			RetardoMas4Horas = 101,
			PermisoParcialMenor4Horas = 24
		}

	}
}
