using System;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace Marcajes.Clases
{
  class PasaMarcajes
  {

    #region VARIABLES
    
    SqlConnection ConexionFestivo;

    SqlCommand cmdBusca, cmdHorario, cmdInsert, cmdDivision, cmdMarcajes, cmdAsistencia, cmdActualiza, cmdAsistencia_Ant, cmdActmar, cmdDia_Ant, cmdFestivo, cmdDia_Ant2;
    SqlDataAdapter daBusca, daHorario, daFestivo, daBusca2;
    DataTable dtBusca, dtHorario, dtMarcaje, dtAsistencia, dtAsistencia_Ant, dtActualiza, dtDiaHorario, dtDia_Ant, dt_Festivo, dtDia_Ant2;
    DateTime wfechaRevAnt;

    #endregion

    #region METODOS PERSONALES
    
    public void IniciarProceso(string fechaRevision)
    {

      #region [VALORES]
      //DECLARACION VARIABLES
      string wtipo_dia1 = "", wtipo_dia2 = "", wclave_emp = "", wcategoria = "", wcont_div = "", wdepto = "", wsupervisor = "", whorario = "", wturno = "";
      string wcve_pago = "", wdia_horario = "", wtipo_dia = "", whora_ent = "", whora_sal = "", whora_ent2 = "", whora_sal2 = "", whora_ent3 = "";
      string whora_sal3 = "", whora_ent4 = "", whora_sal4 = "", whora_str = "", wtipo_mov = "", wEntrada = "", wSalida = "", whora_entAnt = "", whora_salAnt = "", whora_ent2Ant = "";
      string whora_sal2Ant = "", whora_ent3Ant = "", whora_sal3Ant = "", wtipo_reg = "";
      string wcampo = "", wcampo2 = "", wdiasem = "", wdia_Ant = "";
      int ws_fiscal = 0, wa_fiscal = 0, wgrado_niv = 0, wtotal = 0, whoras_nor = 0, whoras_norAnt = 0;
      double went = 0, wsal = 0, wmar = 0, wdifEntrada = 0, wdifSalida = 0, wdifEntradaAnt = 0, wdifSalidaAnt = 0;
      Int32 wbandera = 0;
      DateTime wmarcaje, wEntrada_Horario, wSalida_Horario, wfecha_lista, wfecha_lista_ant;

      //PROGRAMA CON MODIFICACION: 2015-04-29
      wfecha_lista = Convert.ToDateTime(fechaRevision).Date;
      wfecha_lista_ant = wfecha_lista.AddDays(-1);

      string nombreDia = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(wfecha_lista.ToString("dddd", new CultureInfo("es-ES")));
      wdia_horario = nombreDia.Replace("é", "e").Replace("á", "a").ToUpper();
      wdiasem = nombreDia.Replace("é", "e").Replace("á", "a");
      wdiasem = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(wdiasem);
      //OBTIENE DATOS DE TABLA PERIODOS
      cmdBusca = new SqlCommand("SELECT TIPO_DIA, TIPO_DIA_2, S_FISCAL, A_FISCAL FROM PERIODOS WHERE FECHA=@wfecha_lista", ConexionFestivo);
      cmdBusca.Parameters.Add("@wfecha_lista", SqlDbType.Date).Value = wfecha_lista;
      daBusca.SelectCommand = cmdBusca;
      daBusca.Fill(dtBusca);
      //TODO: ForEach innecesario si solo regresa un renglon
      foreach (DataRow PERIODOS in dtBusca.Rows)
      {
        wtipo_dia1 = PERIODOS["TIPO_DIA"].ToString();
        wtipo_dia2 = PERIODOS["TIPO_DIA_2"].ToString();
        ws_fiscal = Convert.ToInt32(PERIODOS["S_FISCAL"]);
        wa_fiscal = Convert.ToInt32(PERIODOS["A_FISCAL"]);
      }
      #endregion valores

      #region Ingresa nuevos registros en ADP
      SqlCommand insertaNuevos = new SqlCommand("promedatos.dbo.paADPGeneraDia", ConexionFestivo);
      insertaNuevos.CommandType = CommandType.StoredProcedure;
      insertaNuevos.Parameters.Add("@fechaLista", SqlDbType.Date).Value = wfecha_lista;
      insertaNuevos.Parameters.Add("@nombreDia", SqlDbType.NVarChar).Value = nombreDia;

      if (ConexionFestivo.State == ConnectionState.Open) ConexionFestivo.Close();

      try
      {
        ConexionFestivo.Open();
        if (insertaNuevos.ExecuteNonQuery() < 0)
        {
          FuncionesComunes.GuardarLog("ERROR AL INSERTAR NUEVOS REGISTROS");
          return;
        }
      }
      catch (Exception ex)
      {
        FuncionesComunes.GuardarLog(String.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
      }
      finally
      {
        ConexionFestivo.Close();
      }

      #endregion

      #region [DIASRECORRE]

      wfechaRevAnt = Convert.ToDateTime(fechaRevision).AddDays(-1);

      while (wfechaRevAnt <= Convert.ToDateTime(fechaRevision))
      {

        #region recorre_tabla_marcajes
        //PASAR MARCAJES A ASIS_DIA_PERM
        Boolean wtipo_fest = false;
        try
        {
          cmdMarcajes = new SqlCommand("SELECT * FROM MARCAJES " +
                                       "WHERE MARCADO=0 " +
                                             "AND (CONVERT(DATE, MARCAJE)=@wfecha_lista) " +
                                       "ORDER BY MARCAJE", ConexionFestivo);
          cmdMarcajes.Parameters.Add("@wfecha_lista", SqlDbType.Date).Value = wfechaRevAnt;
          //cmdMarcajes.Parameters.Add("@wfecha_lista_ant", SqlDbType.Date).Value = wfecha_lista.AddDays(-1);
          dtMarcaje.Clear();
          cmdMarcajes.Connection = ConexionFestivo;
          daBusca.SelectCommand = cmdMarcajes;
          daBusca.Fill(dtMarcaje);

          foreach (DataRow MARCAJES in dtMarcaje.Rows)
          {
            //INICIA SELECCION DE MARCAJES
            wclave_emp = MARCAJES["CLAVE_EMP"].ToString().Trim();
            //TODO: Brinca hasta este empleado
            //		if (!wclave_emp.Equals("06690")) continue;
            //		if (wclave_emp == "06690") {
            //			MessageBox.Show("Aqui");
            //		}
            whora_entAnt = "";
            whora_salAnt = "";
            whora_ent2Ant = "";
            whora_sal2Ant = "";
            whora_ent3Ant = "";
            whora_sal3Ant = "";
            wdifEntradaAnt = 0;
            wdifSalidaAnt = 0;
            whoras_norAnt = 0;
            wmarcaje = Convert.ToDateTime(MARCAJES["MARCAJE"]);
            wmar = Convert.ToDouble(wmarcaje.Hour);
            whora_str = string.Format("{0 :HH:mm}", wmarcaje);
            cmdAsistencia = new SqlCommand("SELECT HORA_ENT,HORA_SAL,HORA_ENT2,HORA_SAL2,HORA_ENT3,HORA_SAL3,HORA_ENT4,HORA_SAL4, " +
                                                  "HORAS_NOR,HORARIO,TIPO_MOV,DEPTO,DIVISION,TURNO,DIASEM " +
                                           "FROM ASIS_DIA_PERM " +
                                           "WHERE CLAVE_EMP=@wclave_emp AND fecha_mov=@wfecha_lista", ConexionFestivo);
            cmdAsistencia.Parameters.Add("@wfecha_lista", SqlDbType.Date).Value = wfechaRevAnt;
            //	cmdAsistencia.Parameters.Add("@wfecha_lista_ant", SqlDbType.Date).Value = wfechaRevAnt.AddDays(-1);
            cmdAsistencia.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
            daBusca.SelectCommand = cmdAsistencia;
            dtAsistencia.Clear(); //TODO: Posiblemente innecesario
            daBusca.Fill(dtAsistencia);
            Boolean wProcesado = false;
            foreach (DataRow ASISTENCIA in dtAsistencia.Rows)
            {    //INICIA SELECCION EN ASIS_DIA_PERM
              whora_ent = ASISTENCIA["HORA_ENT"].ToString().Trim();
              whora_sal = ASISTENCIA["HORA_SAL"].ToString().Trim();
              whora_ent2 = ASISTENCIA["HORA_ENT2"].ToString().Trim();
              whora_sal2 = ASISTENCIA["HORA_SAL2"].ToString().Trim();
              whora_ent3 = ASISTENCIA["HORA_ENT3"].ToString().Trim();
              whora_sal3 = ASISTENCIA["HORA_SAL3"].ToString().Trim();
              whora_ent4 = ASISTENCIA["HORA_ENT4"].ToString().Trim();
              whora_sal4 = ASISTENCIA["HORA_SAL4"].ToString().Trim();
              whoras_nor = Convert.ToInt32(ASISTENCIA["HORAS_NOR"]);
              whorario = ASISTENCIA["HORARIO"].ToString().Trim();
              wtipo_mov = ASISTENCIA["TIPO_MOV"].ToString().Trim();
              wdepto = ASISTENCIA["DEPTO"].ToString().Trim();
              wcont_div = ASISTENCIA["DIVISION"].ToString().Trim();
              wturno = ASISTENCIA["TURNO"].ToString().Trim();
              wdia_horario = ASISTENCIA["DIASEM"].ToString().Trim().ToUpper();            //NOTA: ESTAS ASIGNACIONES QUEDAN INUTILES YA QUE AL SER DIA LABORABLE SE REEMPLAZA
              wdia_horario = wdia_horario.ToLower().Replace("é", "e").Replace("á", "a").ToUpper();  //			POR EL DIA EN QUE SE PASAN LOS MARCAJES, QUE YA TENIA EN UN INICIO

              wtipo_dia = wtipo_mov;
              dtDiaHorario.Clear();

              cmdHorario = new SqlCommand("SELECT LUNES,MARTES,MIERCOLES,JUEVES,VIERNES,SABADO,DOMINGO " +
                                          "FROM HORARIOS " +
                                          "WHERE HORARIO=@whorario", ConexionFestivo);
              cmdHorario.Parameters.Add("@whorario", SqlDbType.Char).Value = whorario;
              daBusca.SelectCommand = cmdHorario;
              daBusca.Fill(dtDiaHorario);
              if (whoras_nor > 0)
              {

                #region DIA LABORABLE
                //foreach (DataRow HORARIOS in dtDiaHorario.Rows) {
                //wdia_horario = (wdia.Replace("é", "e").Replace("á", "a")).Trim().ToUpper();

                //NOTA: De las siguientes instrucciones, solo un par es necesario ya que el nombre del campo corresponde a la variable.
                //if (!dtDiaHorario.Rows[0][wdia_horario].ToString().Trim().Equals(String.Empty)) {
                //	wEntrada = dtDiaHorario.Rows[0][wdia_horario].ToString().Trim().ToUpper().Substring(0, 5);
                //	wSalida = dtDiaHorario.Rows[0][wdia_horario].ToString().Trim().ToUpper().Substring(5, 5);
                //} else continue;
                switch (wdia_horario)
                {     //ENTRADA Y SALIDA DE HORARIOS
                  case "LUNES":
                    if (!dtDiaHorario.Rows[0]["LUNES"].ToString().Trim().Equals(String.Empty))
                    {
                      wEntrada = dtDiaHorario.Rows[0]["LUNES"].ToString().Trim().ToUpper().Substring(0, 5);
                      wSalida = dtDiaHorario.Rows[0]["LUNES"].ToString().Trim().ToUpper().Substring(5, 5);
                    }
                    else continue;
                    break;
                  case "MARTES":
                    if (!dtDiaHorario.Rows[0]["MARTES"].ToString().Trim().Equals(String.Empty))
                    {
                      wEntrada = dtDiaHorario.Rows[0]["MARTES"].ToString().Trim().ToUpper().Substring(0, 5);
                      wSalida = dtDiaHorario.Rows[0]["MARTES"].ToString().Trim().ToUpper().Substring(5, 5);
                    }
                    else continue;
                    break;
                  case "MIERCOLES":
                    if (!dtDiaHorario.Rows[0]["MIERCOLES"].ToString().Trim().Equals(String.Empty))
                    {
                      wEntrada = dtDiaHorario.Rows[0]["MIERCOLES"].ToString().Trim().ToUpper().Substring(0, 5);
                      wSalida = dtDiaHorario.Rows[0]["MIERCOLES"].ToString().Trim().ToUpper().Substring(5, 5);
                    }
                    else continue;
                    break;
                  case "JUEVES":
                    if (!dtDiaHorario.Rows[0]["JUEVES"].ToString().Trim().Equals(String.Empty))
                    {
                      wEntrada = dtDiaHorario.Rows[0]["JUEVES"].ToString().Trim().ToUpper().Substring(0, 5);
                      wSalida = dtDiaHorario.Rows[0]["JUEVES"].ToString().Trim().ToUpper().Substring(5, 5);
                    }
                    else continue;
                    break;
                  case "VIERNES":
                    if (!dtDiaHorario.Rows[0]["VIERNES"].ToString().Trim().Equals(String.Empty))
                    {
                      wEntrada = dtDiaHorario.Rows[0]["VIERNES"].ToString().Trim().ToUpper().Substring(0, 5);
                      wSalida = dtDiaHorario.Rows[0]["VIERNES"].ToString().Trim().ToUpper().Substring(5, 5);
                    }
                    else continue;
                    break;
                  case "SABADO":
                    if (!dtDiaHorario.Rows[0]["SABADO"].ToString().Trim().Equals(String.Empty))
                    {
                      wEntrada = dtDiaHorario.Rows[0]["SABADO"].ToString().Trim().ToUpper().Substring(0, 5);
                      wSalida = dtDiaHorario.Rows[0]["SABADO"].ToString().Trim().ToUpper().Substring(5, 5);
                    }
                    else continue;
                    break;
                  case "DOMINGO":
                    if (!dtDiaHorario.Rows[0]["DOMINGO"].ToString().Trim().Equals(String.Empty))
                    {
                      wEntrada = dtDiaHorario.Rows[0]["DOMINGO"].ToString().Trim().ToUpper().Substring(0, 5);
                      wSalida = dtDiaHorario.Rows[0]["DOMINGO"].ToString().Trim().ToUpper().Substring(5, 5);
                    }
                    else continue;
                    break;
                }    //ENTRADA Y SALIDA DE HORARIOS

                #region VALIDA_ENTRADAYSALIDA
                if (wSalida.Trim() == "24:00") wSalida = "00:00";
                //	wEntrada = DateTime.Now.ToShortDateString() + " " + wEntrada;
                //wEntrada = wfechaRevAnt.ToShortDateString() + " " + wEntrada;
                wEntrada = wfechaRevAnt.ToString("yyyy-MM-dd") + " " + wEntrada;
                //FuncionesComunes.GuardarLog(String.Format("Empleado {1} | wEntrada={0}", wEntrada, wclave_emp));
                if (wEntrada.Trim().Substring(11, 5) == "24:00") wEntrada = "00:00";
                wEntrada_Horario = Convert.ToDateTime(wEntrada);
                //	wSalida = DateTime.Now.ToShortDateString() + " " + wSalida;
                //wSalida = wfechaRevAnt.ToShortDateString() + " " + wSalida;
                wSalida = wfechaRevAnt.ToString("yyyy-MM-dd") + " " + wSalida;
                if (wSalida.Trim().Substring(11, 5) == "24:00") wSalida = "00:00";

                wSalida_Horario = Convert.ToDateTime(wSalida);

                went = Convert.ToDouble(wEntrada_Horario.Hour);
                wsal = Convert.ToDouble(wSalida_Horario.Hour);
                wmar = Convert.ToDouble(wmarcaje.Hour);
                wdifEntrada = (wEntrada_Horario.Subtract(wmarcaje).TotalMinutes);
                if (wdifEntrada < 0) wdifEntrada = (-1 * wdifEntrada);
                //		string xx = wmarcaje.Subtract(wEntrada_Horario).TotalMinutes.ToString();
                wdifSalida = (wSalida_Horario.Subtract(wmarcaje).TotalMinutes);

                wdifEntrada = Math.Abs(wdifEntrada);
                wdifSalida = Math.Abs(wdifSalida);
                //if (wdifSalida < 0) wdifSalida = (-1 * wdifSalida);	//Instruccion inútil debido al Math.Abs de arriba
                if (whorario == "71" && (wmar >= 0 && wmar < 5)) wdifSalida = ((wmar + 24) - wsal) * 60;

                #endregion

                wtipo_fest = false;
                if (wtipo_dia == "F") wtipo_fest = true;
                if (!wtipo_fest)
                {

                  #region DIA NO FESTIVO
                  switch (wturno)
                  {
                    //NOTA: PREGUNTAR POR wipo_mov!="F" ES REDUNDANTE, YA QUE ES LA PRIMERA CONDICION PARA ENTRAR A ESTA PARTE
                    #region TURNO 1
                    case "1":
                      if ((wdifEntrada < wdifSalida) && Convert.ToInt32(whora_ent.Trim().Length) < 5 && wtipo_mov != "F" && wProcesado == false)
                      {

                        #region SI NO TIENE ENTRADA 1 Y LA DIFERENCIA DE ENTRADA ES MENOR QUE LA DE SALIDA Y EL MARCAJE NO HA SIDO PROCESADO
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 1;
                        wtipo_reg = "V";
                        wProcesado = true;
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        #endregion

                      }
                      else if ((wdifSalida < wdifEntrada) && Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && wtipo_mov != "F" && wProcesado == false)
                      {

                        #region SI NO TIENE PAR 1 Y LA DIFERENCIA DE SALID ES MENOR A LA DE ENTRADA Y EL MARCAJE NO SE HA PROCESADO
                        wcampo2 = "HORA_ENT";
                        wcampo = "HORA_SAL";
                        wbandera = 1;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) < 5 && wtipo_mov != "F" && wProcesado == false)
                      {

                        #region SI TIENE PAR 1 Y NO TIENE ENTRADA 2 Y EL MARCAJE NO HA SIDO PROCESADO
                        wcampo = "HORA_ENT2";
                        wcampo2 = "HORA_SAL2";
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_sal, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      if (Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && wtipo_mov != "F" && wProcesado == false)
                      {
                        //NOTA: QUITAR ESTE COMENTARIO CUANDO SE JUNTE ESTA CONDICION CON LA ANTERIOR
                        #region SI NO TIENE PAR 1 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 1;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) < 5 && wtipo_mov != "F" && wProcesado == false)
                      {

                        #region SI TIENE PAR 1 Y TIENE ENTRADA 2 Y NO TIENE SALIDA 2 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL2";
                        wbandera = 0;
                        wtipo_reg = Diferencia_Marcaje(whora_ent2, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) == 5 && Convert.ToInt32(whora_ent3.Trim().Length) < 5 && wtipo_mov != "F" && wProcesado == false)
                      {

                        #region SI TIENE PAR 1 Y TIENE PAR 2 Y NO TIENE ENTRADA 3 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT3";
                        wcampo2 = "HORA_SAL3";
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_sal2, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) == 5 && Convert.ToInt32(whora_ent3.Trim().Length) == 5 && Convert.ToInt32(whora_sal3.Trim().Length) < 5 && wtipo_mov != "F" && wProcesado == false)
                      {

                        #region SI TIENE PAR 1 Y TIENE PAR 2 Y TIENE ENTRADA 3 Y NO TIENE SALIDA 3 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL3";
                        wbandera = 0;
                        wtipo_reg = Diferencia_Marcaje(whora_ent3, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && wtipo_mov != "F" && wProcesado == false)
                      {

                        #region SI TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL";
                        wbandera = 0;
                        wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && wtipo_mov != "F" && wProcesado == false)
                      {

                        #region SI NO TIENE ENTRADA 1 Y TIENE SALIDA 1 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT2";
                        wcampo2 = "HORA_SAL2";
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_sal, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      //TODO: Posible mejor ubicacion para un solo llamado
                      //Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                      break;
                    #endregion

                    #region TURNO 2
                    case "2":
                      if ((wdifEntrada < wdifSalida) && (Convert.ToInt32(whora_ent.Trim().Length) < 5) && wProcesado == false && whorario != "71")
                      {

                        #region SI NO TIENE ENTRADA 1 Y LA DIFERENCIA DE ENTRADA ES MENOR QUE LA DE SALIDA Y EL MARCAJE NO SE HA PROCESADO Y EL HORARIO ES DIFERENTE DEL 71
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 1;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion
                        //CAMBIO: 2015-04-15
                      }
                      else if ((wdifEntrada < wdifSalida) && (Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5) && wProcesado == false && whorario == "71")
                      {

                        #region SI NO TIENE PAR 1 Y LA DIFERENCIA DE ENTRADA ES MENOR QUE LA DE SALIDA Y EL MARCAJE NO SE HA PROCESADO Y EL HORARIO ES EL 71
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 1;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if ((wdifSalida < wdifEntrada) && (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5) && wsal >= 22 && wmar >= 22 && wProcesado == false)
                      {

                        #region SI TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y LA DIFERENCIA DE SALIDA ES MENOR QUE LA DE ENTRADA Y LA SALIDA ES DESPUES DE LAS 22:00 Y EL MARCAJE ES DESPUES DE LAS 22:00 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL";
                        wcampo2 = "HORA_ENT";
                        wbandera = 0;
                        wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if ((wdifEntrada < wdifSalida) && (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5) && wProcesado == false)
                      {

                        #region SI TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y LA DIFERENCIA DE ENTRADA ES MENOR QUE LA DE SALIDA Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL";
                        wcampo2 = "HORA_ENT";
                        wbandera = 0;
                        wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if ((wdifSalida < wdifEntrada) && (Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5) && wProcesado == false)
                      {

                        #region SI NO TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y LA DIFERENCIA DE SALIDA ES MENOR QUE LA DE ENTRADA Y EL MARCAJE NO SE HA PROCESADO
                        cmdDia_Ant = new SqlCommand();
                        cmdDia_Ant.CommandText = "SELECT HORA_ENT,HORA_SAL,HORA_ENT2,HORA_SAL2,HORA_ENT3,HORA_SAL3,HORAS_NOR FROM ASIS_DIA_PERM WHERE CLAVE_EMP=@wclave_emp AND FECHA_MOV=@wfecha_mov";
                        cmdDia_Ant.Parameters.Clear();
                        cmdDia_Ant.Parameters.Add("@wfecha_mov", SqlDbType.Date).Value = wfechaRevAnt.AddDays(-1);
                        cmdDia_Ant.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                        cmdDia_Ant.Connection = ConexionFestivo;
                        daBusca.SelectCommand = cmdDia_Ant;
                        dtDia_Ant.Clear();
                        daBusca.Fill(dtDia_Ant);
                        //TODO: ForEach innecesario si solo regresa un renglon
                        foreach (DataRow ASIS_DIA_PERM in dtDia_Ant.Rows)
                        {
                          whora_entAnt = ASIS_DIA_PERM["HORA_ENT"].ToString().Trim();
                          whora_salAnt = ASIS_DIA_PERM["HORA_SAL"].ToString().Trim();
                          whora_ent2Ant = ASIS_DIA_PERM["HORA_ENT2"].ToString().Trim();
                          whora_sal2Ant = ASIS_DIA_PERM["HORA_SAL2"].ToString().Trim();
                          whora_ent3Ant = ASIS_DIA_PERM["HORA_ENT3"].ToString().Trim();
                          whora_sal3Ant = ASIS_DIA_PERM["HORA_SAL3"].ToString().Trim();
                          whoras_norAnt = Convert.ToInt32(ASIS_DIA_PERM["HORAS_NOR"]);
                        }
                        #endregion

                      }
                      if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5 && wProcesado == false)
                      {
                        //NOTA: QUITAR ESTE COMENTARIO CUANDO SE JUNTE ESTA CONDICION CON LA ANTERIOR
                        #region SI EL DIA ANTERIOR TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y EL MARCAJE NO HA SIDO PROCESADO
                        wcampo = "HORA_SAL";
                        wbandera = 2;
                        wtipo_reg = Diferencia_Marcaje(whora_entAnt, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) < 5 && whoras_norAnt > 0 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR NO TIENE ENTRADA 1 Y EL DIA ANTERIOR ES LABORABLE Y EL MARCAJE NO SE HA PROCESADO
                        wcampo2 = "HORA_ENT";
                        wcampo = "HORA_SAL";
                        wbandera = 3;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion
                        //	CAMBIO: 2015-04-16
                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && Convert.ToInt32(whora_ent2Ant.Trim().Length) < 5 && wProcesado == false && Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5)
                      {

                        #region SI EL DIA ANTERIOR TIENE PAR 1 Y NO TIENE ENTRADA 2 Y EL DIA ACTUAL NO TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT2";
                        wcampo2 = "HORA_SAL2";
                        wbandera = 3;
                        wtipo_reg = Diferencia_Marcaje(whora_salAnt, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && Convert.ToInt32(whora_ent2Ant.Trim().Length) == 5 && Convert.ToInt32(whora_sal2Ant.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR TIENE PAR 1 Y TIENE ENTRADA 2 Y NO TIENE SALIDA 2 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL2";
                        wbandera = 2;
                        wtipo_reg = Diferencia_Marcaje(whora_ent2Ant, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && Convert.ToInt32(whora_ent2Ant.Trim().Length) == 5 && Convert.ToInt32(whora_sal2Ant.Trim().Length) == 5 && Convert.ToInt32(whora_ent3Ant.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR TIENE PAR 1 Y TIENE PAR 2 Y NO TIENE ENTRADA 3 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT3";
                        wcampo2 = "HORA_SAL3";
                        wbandera = 3;
                        wtipo_reg = Diferencia_Marcaje(whora_sal2Ant, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && Convert.ToInt32(whora_ent2Ant.Trim().Length) == 5 && Convert.ToInt32(whora_sal2Ant.Trim().Length) == 5 && Convert.ToInt32(whora_ent3Ant.Trim().Length) == 5 && Convert.ToInt32(whora_sal3Ant.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR TIENE PAR 1 Y TIENE PAR 2 Y TIENE ENTRADA 3 Y NO TIENE SALIDA 3 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL3";
                        wbandera = 2;
                        wtipo_reg = Diferencia_Marcaje(whora_ent3Ant, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI TIENE PAR 1 Y NO TIENE ENTRADA 2 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT2";
                        wcampo2 = "HORA_SAL2";
                        wtipo_reg = Diferencia_Marcaje(whora_sal, whora_str);
                        wbandera = 1;
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI TIENE PAR 1 Y TIENE ENTRADA 2 Y NO TIENE SALIDA 2 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL2";
                        wtipo_reg = Diferencia_Marcaje(whora_ent2, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) == 5 && Convert.ToInt32(whora_ent3.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI TIENE PAR 1 Y TIENE PAR 2 Y NO TIENE ENTRADA 3 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT3";
                        wcampo2 = "HORA_SAL3";
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_sal2, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) == 5 && Convert.ToInt32(whora_ent3.Trim().Length) == 5 && Convert.ToInt32(whora_sal3.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI TIENE PAR 1 Y TIENE PAR 2 Y TIENE ENTRADA 3 Y NO TIENE SALIDA 3 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL3";
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_ent3, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL";
                        wbandera = 0;
                        wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) < 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5 && Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR NO TIENE PAR 1 Y EL DIA ACTUAL NO TIENE PAR 1 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 1;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      break;
                    #endregion

                    #region TURNO 3
                    case "3":
                      if ((wdifEntrada < wdifSalida) && (Convert.ToInt32(whora_ent.Trim().Length) < 5) && (Convert.ToInt32(whora_sal.Trim().Length) == 5) && wProcesado == false)
                      {

                        #region SI NO TIENE ENTRADA 1 Y TIENE SALIDA 1 Y LA DIFERENCIA DE ENTRADA ES MENOR QUE LA DE SALIDA Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT";
                        //wcampo2 = "HORA_SAL";
                        wbandera = 0;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      if ((wdifEntrada < wdifSalida) && (Convert.ToInt32(whora_ent.Trim().Length) < 5) && wProcesado == false)
                      {
                        //NOTA: QUITAR ESTE COMENTARIO CUANDO SE JUNTE ESTA CONDICION CON LA ANTERIOR
                        #region SI NO TIENE ENTRADA 1 Y LA DIFERENCIA DE ENTRADA ES MENOR A LA DE SALIDA Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 1;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (wdifEntrada < wdifSalida && (Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5) && wProcesado == false)
                      {

                        #region SI NO TIENE PAR 1 Y LA DIFERENCIA DE ENTRADA ES MENOR A LA DE SALIDA Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (wdifEntrada < wdifSalida && (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5) && wProcesado == false)
                      {

                        #region SI TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y LA DIFERENCIA DE ENTRADA ES MENOR A LA DE SALIDA Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL";
                        wbandera = 0;
                        wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (wdifSalida < wdifEntrada && (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5) && went >= 22 && wsal <= 6 && (wmar >= 0 && wmar <= 2) && wProcesado == false)
                      {

                        #region SI TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y LA DIFERENCIA DE SALIDA ES MENOR A LA DE ENTRADA Y LA ENTRADA ES DESDE LAS 22:00 Y LA SALIDA HASTA LAS 06:00 Y EL MARCAJE ES ENTRE LAS 00:00 Y LAS 02:00 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (
                      wdifEntrada < wdifSalida && (Convert.ToInt32(whora_ent.Trim().Length) == 5
                      && Convert.ToInt32(whora_sal.Trim().Length) == 5
                      && Convert.ToInt32(whora_ent2.Trim().Length) < 5)
                      && went >= 22 && wsal <= 6
                      && (wmar >= 0 && wmar <= 2 || (wmar > 22 && wmar < 24))
                      && wProcesado == false)
                      {

                        #region SI TIENE PAR 1 Y NO TIENE ENTRADA 2 Y LA ENTRADA ES DESDE LAS 22:00 Y LA SALIDA HASTA LAS 06:00 Y EL MARCAJE ES ENTRE LAS 00:00 Y LAS 02:00 O DESPUES DE LAS 22:00 Y ANTES DE LAS 00:00 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT2";
                        wcampo2 = "HORA_SAL2";
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_sal, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (wdifEntrada < wdifSalida && (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) < 5) && went >= 22 && wsal <= 6 && (wmar >= 0 && wmar <= 2) && wProcesado == false)
                      {

                        #region SI TIENE PAR 1 Y TIENE ENTRADA 2 Y NO TIENE SALIDA 2 Y LA DIFERENCIA DE ENTRADA ES MENOR A LA DE SALIDA Y LA ENTRADA ES DESDE LAS 22:00 Y LA SALIDA HASTA LAS 06:00 Y EL MARCAJE ES ENTRE LAS 00:00 Y LAS 02:00 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL2";
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_ent2, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (wdifSalida < wdifEntrada && wmar >= 0 && wmar <= 13 && wProcesado == false)
                      {

                        #region SI LA DIFERENCIA DE SALIDA ES MENOR A LA DE ENTRADA Y EL MARCAJE ES ENTRE LAS 00:00 Y LAS 13:00 Y EL MARCAJE NO SE HA PROCESADO
                        cmdDia_Ant = new SqlCommand();
                        cmdDia_Ant.CommandText = "SELECT HORA_ENT,HORA_SAL,HORA_ENT2,HORA_SAL2,HORA_ENT3,HORA_SAL3,HORAS_NOR FROM ASIS_DIA_PERM WHERE CLAVE_EMP=@wclave_emp AND FECHA_MOV=@wfecha_mov";
                        cmdDia_Ant.Parameters.Clear();
                        cmdDia_Ant.Parameters.Add("@wfecha_mov", SqlDbType.Date).Value = wfecha_lista.AddDays(-1);
                        cmdDia_Ant.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                        cmdDia_Ant.Connection = ConexionFestivo;
                        daBusca.SelectCommand = cmdDia_Ant;
                        dtDia_Ant.Clear();
                        daBusca.Fill(dtDia_Ant);
                        //TODO: ForEach innecesario si solo regresa un renglon
                        foreach (DataRow ASIS_DIA_PERM in dtDia_Ant.Rows)
                        {
                          whora_entAnt = ASIS_DIA_PERM["HORA_ENT"].ToString().Trim();
                          whora_salAnt = ASIS_DIA_PERM["HORA_SAL"].ToString().Trim();
                          whora_ent2Ant = ASIS_DIA_PERM["HORA_ENT2"].ToString().Trim();
                          whora_sal2Ant = ASIS_DIA_PERM["HORA_SAL2"].ToString().Trim();
                          whora_ent3Ant = ASIS_DIA_PERM["HORA_ENT3"].ToString().Trim();
                          whora_sal3Ant = ASIS_DIA_PERM["HORA_SAL3"].ToString().Trim();
                          whoras_norAnt = Convert.ToInt16(ASIS_DIA_PERM["HORAS_NOR"]);
                        }
                        #endregion

                      }
                      if (Convert.ToInt32(whora_entAnt.Trim().Length) < 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5 && whoras_norAnt > 0 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR NO TIENE PAR 1 Y ES LABORABLE Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 3;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      if (Convert.ToInt32(whora_entAnt.Trim().Length) < 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5 /*&& Convert.ToInt32(whora_entAnt.Trim().Length) < 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5 */&& whoras_norAnt == 0 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR NO TIENE PAR 1 Y NO ES LABORABLE Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 1;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR TIENE ENTRADA 1 Y NO TIENE LA SALIDA 1 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL";
                        wbandera = 2;
                        wtipo_reg = Diferencia_Marcaje(whora_entAnt, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && Convert.ToInt32(whora_ent2Ant.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR TIENE PAR 1 Y NO TIENE ENTRADA 2 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT2";
                        wcampo2 = "HORA_SAL2";
                        wbandera = 3;
                        wtipo_reg = Diferencia_Marcaje(whora_salAnt, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && Convert.ToInt32(whora_ent2Ant.Trim().Length) == 5 && Convert.ToInt32(whora_sal2Ant.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR TIENE PAR 1 Y TIENE ENTRADA 2 Y NO TIENE SALIDA 2 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL2";
                        wbandera = 2;
                        wtipo_reg = Diferencia_Marcaje(whora_ent2Ant, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && Convert.ToInt32(whora_ent2Ant.Trim().Length) == 5 && Convert.ToInt32(whora_sal2Ant.Trim().Length) == 5 && Convert.ToInt32(whora_ent3Ant.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR TIENE PAR 1 Y TIENE PAR 2 Y NO TIENE ENTRADA 3 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT3";
                        wcampo2 = "HORA_SAL3";
                        wbandera = 3;
                        wtipo_reg = Diferencia_Marcaje(whora_sal2Ant, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && Convert.ToInt32(whora_ent2Ant.Trim().Length) == 5 && Convert.ToInt32(whora_sal2Ant.Trim().Length) == 5 && Convert.ToInt32(whora_ent3Ant.Trim().Length) == 5 && Convert.ToInt32(whora_sal3Ant.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR TIENE PAR 1 Y TIENE PAR 2 Y TIENE ENTRADA 2 Y NO TIENE SALIDA 3 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL3";
                        wbandera = 2;
                        wtipo_reg = Diferencia_Marcaje(whora_ent3Ant, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR NO TIENE ENTRADA 1 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL";
                        wcampo2 = "HORA_ENT";
                        wbandera = 3;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if ((Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5) && (wmar >= 22 && wmar < 24) && wProcesado == false)
                      {

                        #region SI TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y EL MARCAJE ES ENTRE LAS 22:00 Y LAS 00:00 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL";
                        wbandera = 0;
                        wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfecha_lista, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      break;
                    #endregion

                    #region TURNO 4 Y 5
                    //CAMBIO: 2015/04/19
                    case "4":
                    case "5":
                      if ((wdifEntrada < wdifSalida) && (Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5) && wProcesado == false && wturno == "4")
                      {

                        #region SI NO TIENE PAR 1 Y LA DIFERENCIA DE ENTRADA ES MENOR A LA SALIDA Y ES DE TURNO 4 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 1;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }

                      if ((wdifSalida < wdifEntrada) && (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5) && wProcesado == false && wturno == "4")
                      {

                        #region SI TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y LA DIFERENCIA DE SALIDA ES MENOR A LA ENTRADA Y ES DE TURNO 4 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL";
                        wcampo2 = "HORA_ENT";
                        wbandera = 0;
                        wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      if ((wdifEntrada > wdifSalida) && (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5) && wProcesado == false && wturno == "5" && wmar >= 12)
                      {

                        #region SI TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y LA DIFERENCIA DE ENTRADA ES MAYOR A LA SALIDA Y ES DE TURNO 5 Y EL MARCAJE ES DESDE LAS 12:00 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL";
                        //wcampo2 = "HORA_SAL";
                        wbandera = 0;
                        wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      if ((wdifEntrada < wdifSalida) && (Convert.ToInt32(whora_ent.Trim().Length) < 5) && wProcesado == false)
                      {

                        #region SI NO TIENE ENTRADA 1 Y LA DIFERENCIA DE ENTRADA ES MENOR A LA SALIDA Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 1;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (wdifSalida < wdifEntrada && wmar >= 15 && wProcesado == false && Convert.ToInt32(whora_ent.Trim().Length) == 5)
                      {

                        #region SI TIENE ENTRADA 1 Y LA DIFERENCIA DE SALIDA ES MENOR A LA ENTRADA Y EL MARCAJE ES DESPUES DE LAS 15:00 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo2 = "HORA_ENT";
                        wcampo = "HORA_SAL";
                        wbandera = 2;
                        wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (wdifSalida < wdifEntrada && wmar < 8 && wProcesado == false)
                      {

                        #region SI EL MARCAJE ES ANTES DE LAS 08:00 Y LA DIFERENCIA DE SALIDA ES MENOR A LA ENTRADA Y EL MARCAJE NO SE HA PROCESADO
                        cmdDia_Ant = new SqlCommand();
                        cmdDia_Ant.CommandText = "SELECT HORA_ENT,HORA_SAL,HORA_ENT2,HORA_SAL2,HORAS_NOR FROM ASIS_DIA_PERM WHERE CLAVE_EMP=@wclave_emp AND FECHA_MOV=@wfecha_mov AND TIPO_MOV<> 'D'";
                        cmdDia_Ant.Parameters.Clear();
                        cmdDia_Ant.Parameters.Add("@wfecha_mov", SqlDbType.Date).Value = wfechaRevAnt.AddDays(-1);
                        cmdDia_Ant.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                        cmdDia_Ant.Connection = ConexionFestivo;
                        daBusca.SelectCommand = cmdDia_Ant;
                        dtDia_Ant.Clear();
                        daBusca.Fill(dtDia_Ant);
                        //TODO: ForEach innecesario si solo regresa un renglon
                        foreach (DataRow ASIS_DIA_PERM in dtDia_Ant.Rows)
                        {
                          whora_entAnt = ASIS_DIA_PERM["HORA_ENT"].ToString().Trim();
                          whora_salAnt = ASIS_DIA_PERM["HORA_SAL"].ToString().Trim();
                          whora_ent2Ant = ASIS_DIA_PERM["HORA_ENT2"].ToString().Trim();
                          whora_sal2Ant = ASIS_DIA_PERM["HORA_SAL2"].ToString().Trim();
                          whoras_norAnt = Convert.ToInt32(ASIS_DIA_PERM["HORAS_NOR"]);
                          if (whoras_norAnt > 0)
                          {
                            wdia_Ant = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(wfechaRevAnt.AddDays(-1).ToString("dddd", new CultureInfo("es-ES")));
                            wdia_Ant = wdia_Ant.Replace("é", "e").Replace("á", "a").ToUpper();

                            String wEntradaAnt = "", wSalidaAnt = "";
                            //MODIFICACION
                            switch (wdia_Ant)
                            {     //ENTRADA Y SALIDA DE HORARIOS
                              case "LUNES":
                                if (!dtDiaHorario.Rows[0]["LUNES"].ToString().Trim().Equals(String.Empty))
                                {
                                  wEntradaAnt = dtDiaHorario.Rows[0]["LUNES"].ToString().Trim().ToUpper().Substring(0, 5);
                                  wSalidaAnt = dtDiaHorario.Rows[0]["LUNES"].ToString().Trim().ToUpper().Substring(5, 5);
                                }
                                else continue;
                                break;
                              case "MARTES":
                                if (!dtDiaHorario.Rows[0]["MARTES"].ToString().Trim().Equals(String.Empty))
                                {
                                  wEntradaAnt = dtDiaHorario.Rows[0]["MARTES"].ToString().Trim().ToUpper().Substring(0, 5);
                                  wSalidaAnt = dtDiaHorario.Rows[0]["MARTES"].ToString().Trim().ToUpper().Substring(5, 5);
                                }
                                else continue;
                                break;
                              case "MIERCOLES":
                                if (!dtDiaHorario.Rows[0]["MIERCOLES"].ToString().Trim().Equals(String.Empty))
                                {
                                  wEntradaAnt = dtDiaHorario.Rows[0]["MIERCOLES"].ToString().Trim().ToUpper().Substring(0, 5);
                                  wSalidaAnt = dtDiaHorario.Rows[0]["MIERCOLES"].ToString().Trim().ToUpper().Substring(5, 5);
                                }
                                else continue;
                                break;
                              case "JUEVES":
                                if (!dtDiaHorario.Rows[0]["JUEVES"].ToString().Trim().Equals(String.Empty))
                                {
                                  wEntradaAnt = dtDiaHorario.Rows[0]["JUEVES"].ToString().Trim().ToUpper().Substring(0, 5);
                                  wSalidaAnt = dtDiaHorario.Rows[0]["JUEVES"].ToString().Trim().ToUpper().Substring(5, 5);
                                }
                                else continue;
                                break;
                              case "VIERNES":
                                if (!dtDiaHorario.Rows[0]["VIERNES"].ToString().Trim().Equals(String.Empty))
                                {
                                  wEntradaAnt = dtDiaHorario.Rows[0]["VIERNES"].ToString().Trim().ToUpper().Substring(0, 5);
                                  wSalidaAnt = dtDiaHorario.Rows[0]["VIERNES"].ToString().Trim().ToUpper().Substring(5, 5);
                                }
                                else continue;
                                break;
                              case "SABADO":
                                if (!dtDiaHorario.Rows[0]["SABADO"].ToString().Trim().Equals(String.Empty))
                                {
                                  wEntradaAnt = dtDiaHorario.Rows[0]["SABADO"].ToString().Trim().ToUpper().Substring(0, 5);
                                  wSalidaAnt = dtDiaHorario.Rows[0]["SABADO"].ToString().Trim().ToUpper().Substring(5, 5);
                                }
                                else continue;
                                break;
                              case "DOMINGO":
                                if (!dtDiaHorario.Rows[0]["DOMINGO"].ToString().Trim().Equals(String.Empty))
                                {
                                  wEntradaAnt = dtDiaHorario.Rows[0]["DOMINGO"].ToString().Trim().ToUpper().Substring(0, 5);
                                  wSalidaAnt = dtDiaHorario.Rows[0]["DOMINGO"].ToString().Trim().ToUpper().Substring(5, 5);
                                }
                                else continue;
                                break;
                            }    //ENTRADA Y SALIDA DE HORARIOS

                            if (wSalidaAnt.Trim() == "24:00") wSalida = "00:00";
                            //	wEntrada = DateTime.Now.ToShortDateString() + " " + wEntrada;
                            //wEntradaAnt = wfechaRevAnt.ToShortDateString() + " " + wEntradaAnt;
                            wEntradaAnt = wfechaRevAnt.ToString("yyyy-MM-dd") + " " + wEntradaAnt;
                            if (wEntradaAnt.Trim().Substring(11, 5) == "24:00") wEntradaAnt = "00:00";
                            wEntrada_Horario = Convert.ToDateTime(wEntradaAnt);
                            //wSalidaAnt = wfechaRevAnt.ToShortDateString() + " " + wSalidaAnt;
                            wSalidaAnt = wfechaRevAnt.ToString("yyyy-MM-dd") + " " + wSalidaAnt;
                            if (wSalidaAnt.Trim().Substring(11, 5) == "24:00") wSalidaAnt = "00:00";

                            wSalida_Horario = Convert.ToDateTime(wSalidaAnt);

                            went = Convert.ToDouble(wEntrada_Horario.Hour);
                            wsal = Convert.ToDouble(wSalida_Horario.Hour);
                            wmar = Convert.ToDouble(wmarcaje.Hour);
                            wdifEntradaAnt = (wEntrada_Horario.Subtract(wmarcaje).TotalMinutes);
                            if (wdifEntradaAnt < 0) wdifEntradaAnt = (-1 * wdifEntradaAnt);
                            wdifSalidaAnt = (wSalida_Horario.Subtract(wmarcaje).TotalMinutes);
                            wdifEntradaAnt = Math.Abs(wdifEntradaAnt);
                            wdifSalidaAnt = Math.Abs(wdifSalidaAnt);
                            if (wdifSalidaAnt < 0) wdifSalidaAnt = (-1 * wdifSalidaAnt);
                          }
                        }
                        #endregion

                      }
                      if (Convert.ToInt32(whora_entAnt.Trim().Length) < 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5 && wProcesado == false && Convert.ToInt32(whora_ent.Trim().Length) < 5 && wdifSalida < wdifEntrada && wturno == "5")
                      {

                        #region SI EL DIA ANTERIOR NO TIENE PAR 1 Y EL DIA ACTUAL NO TIENE ENTRADA 1 Y LA DIFERENCIA DE SALIDA ES MENOR A LA ENTRADA Y ES DE TURNO 5 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 1;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && wProcesado == false && wdifEntradaAnt < wdifSalidaAnt && Convert.ToInt32(whora_entAnt.Trim().Length) < 5 && wturno == "5")
                      {

                        #region SI EL DIA ANTERIOR TIENE PAR 1 Y NO TIENE ENTRADA 1 Y LA DIFERENCIA DE ENTRADA ES MENOR A LA SALIDA Y ES DE TURNO 5 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 1;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL";
                        wbandera = 0;
                        wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL";
                        wbandera = 2;
                        wtipo_reg = Diferencia_Marcaje(whora_entAnt, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) < 5 && wProcesado == false && whoras_norAnt > 0 && wdifEntrada < wdifSalida)
                      {

                        #region SI EL DIA ANTERIOR NO TIENE ENTRADA 1 Y ES LABORABLE Y LA DIFERENCIA DE ENTRADA ES MENOR A LA SALIDA Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 2;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && Convert.ToInt32(whora_ent2Ant.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR TIENE PAR 1 Y NO TIENE ENTRADA 2 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT2";
                        wcampo2 = "HORA_SAL2";
                        wbandera = 3;
                        wtipo_reg = Diferencia_Marcaje(whora_salAnt, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && Convert.ToInt32(whora_ent2Ant.Trim().Length) == 5 && Convert.ToInt32(whora_sal2Ant.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR TIENE PAR 1 Y TIENE ENTRADA 2 Y NO TIENE SALIDA 2 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo2 = "HORA_SAL2";
                        wbandera = 3;
                        wtipo_reg = Diferencia_Marcaje(whora_ent2Ant, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest && wProcesado == false);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && Convert.ToInt32(whora_ent2Ant.Trim().Length) == 5 && Convert.ToInt32(whora_sal2Ant.Trim().Length) == 5 && Convert.ToInt32(whora_ent3Ant.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR TIENE PAR 1 Y TIENE PAR 2 Y NO TIENE ENTRADA 3 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT3";
                        wcampo2 = "HORA_SAL3";
                        wbandera = 3;
                        wtipo_reg = Diferencia_Marcaje(whora_sal2Ant, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && Convert.ToInt32(whora_ent2Ant.Trim().Length) == 5 && Convert.ToInt32(whora_sal2Ant.Trim().Length) == 5 && Convert.ToInt32(whora_ent3Ant.Trim().Length) == 5 && Convert.ToInt32(whora_sal3Ant.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI EL DIA ANTERIOR TIENE PAR 1 Y TIENE PAR 2 Y TIENE ENTRADA 3 Y NO TIENE SALIDA 3 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL3";
                        wbandera = 3;
                        wtipo_reg = Diferencia_Marcaje(whora_ent3Ant, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI TIENE PAR 1 Y NO TIENE ENTRADA 2 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT2";
                        wcampo2 = "HORA_SAL2";
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_sal, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI TIENE PAR 1 Y TIENE ENTRADA 2 Y NO TIENE SALIDA 2 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo2 = "HORA_SAL2";
                        wbandera = 2;
                        wtipo_reg = Diferencia_Marcaje(whora_ent2, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) == 5 && Convert.ToInt32(whora_ent3.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI TIENE PAR 1 Y TIENE PAR 2 Y NO TIENE ENTRADA 3 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT3";
                        wcampo2 = "HORA_SAL3";
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_sal2, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) == 5 && Convert.ToInt32(whora_ent3.Trim().Length) == 5 && Convert.ToInt32(whora_sal3.Trim().Length) < 5 && wProcesado == false)
                      {

                        #region SI TIENE PAR 1 Y TIENE PAR 2 Y TIENE ENTRADA 3 Y NO TIENE SALIDA 3 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL3";
                        wbandera = 3;
                        wtipo_reg = Diferencia_Marcaje(whora_ent3, whora_str);
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && whoras_norAnt == 0 && wProcesado == false && (wdifEntrada < wdifSalida))
                      {

                        #region SI NO TIENE PAR 1 Y EL DIA ANTERIOR NO ES LABORABLE Y LA DIFERENCIA DE ENTRADA ES MENOR A LA SALIDA Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_ENT";
                        wcampo2 = "HORA_SAL";
                        wbandera = 1;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && whoras_norAnt == 0 && wProcesado == false && (wdifEntrada > wdifSalida))
                      {

                        #region SI NO TIENE PAR 1 Y EL DIA ANTERIOR NO ES LABORABLE Y LA DIFERENCIA DE ENTRADA ES MAYOR A LA SALIDA Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL";
                        wcampo2 = "HORA_ENT";
                        wbandera = 1;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && (wdifEntrada > wdifSalida) && wProcesado == false && wturno == "5")
                      {

                        #region SI TIENE PAR 1 Y LA DIFERENCIA DE ENTRADA ES MAYOR A LA SALIDA Y ES DE TURNO 5 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL";
                        //wcampo2 = "HORA_TURNO";
                        wbandera = 0;
                        wtipo_reg = "V";
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && (wdifEntrada < wdifSalida) && wProcesado == false && wturno == "5")
                      {

                        #region SI TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y LA DIFERENCIA DE ENTRADA ES MENOR A LA SALIDA Y ES DE TURNO 5 Y EL MARCAJE NO SE HA PROCESADO
                        wcampo = "HORA_SAL";
                        //wcampo2 = "HORA_TURNO";
                        wbandera = 0;
                        wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str); ;
                        Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                        wProcesado = true;
                        #endregion

                      }

                      break;
                      #endregion
                  }
                  #endregion

                }
                else
                {

                  #region DIA FESTIVO

                  #region BUSCAR SI EXISTEN REGISTROS DE FESTIVOS TRABAJADOS EN ASIS_DIA_PERM
                  dt_Festivo.Clear();
                  cmdFestivo = new SqlCommand("SELECT COUNT(*) FROM ASIS_DIA_PERM WHERE CLAVE_EMP=@wclave_emp AND FECHA_MOV=@wfecha_lista AND TIPO_MOV='T'", ConexionFestivo);
                  cmdFestivo.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                  cmdFestivo.Parameters.Add("@wfecha_lista", SqlDbType.Date).Value = wfechaRevAnt;
                  dt_Festivo = new DataTable();
                  try
                  {
                    if (ConexionFestivo.State == ConnectionState.Closed) { ConexionFestivo.Open(); }
                    wtotal = Convert.ToInt32(cmdFestivo.ExecuteScalar());
                  }
                  catch (SqlException ex)
                  {
                    FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(1498) | {0} | Error al obtener total de festivos ( {1} ).", wclave_emp, ex.Message));
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
                    FuncionesComunes.GuardaStackTraceLog(st);
                  }
                  finally
                  {
                    //cmdFestivo.Dispose(); //TODO: Activar
                    ConexionFestivo.Close();
                  }
                  #endregion

                  //TODO: Tratar de cambiar a switch
                  if (wtotal == 0)
                  {

                    #region NO HAY REGISTROS DE DIAS FESTIVOS TRABAJADOS
                    if ((wturno == "1") || ((wturno == "2" && (wdifSalida > wdifEntrada)) || ((wturno == "3" && (wdifSalida > wdifEntrada)))))
                    {

                      #region SI ES DE TURNO 1 O ES DE TURNO 3 Y LA DIFERENCIA DE SALIDA ES MENOR A LA ENTRADA O ES DE TURNO 3 Y LA DIFERENCIA DE SALIDA ES MAYOR A LA ENTRADA
                      cmdFestivo = new SqlCommand("SELECT DEPTO,TURNO,SUPERV,CATEGORIA,GRADO_NIV,DIASEM,HORARIO,DIVISION,CVE_PAGO,S_FISCAL,A_FISCAL  FROM ASIS_DIA_PERM WHERE CLAVE_EMP=@wclave_emp AND FECHA_MOV=@wfecha_lista", ConexionFestivo);
                      dt_Festivo.Clear();
                      cmdFestivo.Parameters.Add("@wfecha_lista", SqlDbType.Date).Value = wfechaRevAnt;
                      cmdFestivo.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                      daFestivo.SelectCommand = cmdFestivo;
                      daFestivo.Fill(dt_Festivo);
                      foreach (DataRow ASIS_DIA_PERM in dt_Festivo.Rows)
                      {    //INICIA SELECCION EN ASIS_DIA_PERM
                        wdepto = ASIS_DIA_PERM["DEPTO"].ToString().Trim();
                        wturno = ASIS_DIA_PERM["TURNO"].ToString().Trim();
                        wsupervisor = ASIS_DIA_PERM["SUPERV"].ToString().Trim();
                        wcategoria = ASIS_DIA_PERM["CATEGORIA"].ToString().Trim();
                        wgrado_niv = Convert.ToInt32(ASIS_DIA_PERM["GRADO_NIV"]);
                        wdiasem = ASIS_DIA_PERM["DIASEM"].ToString().Trim();
                        whorario = ASIS_DIA_PERM["HORARIO"].ToString().Trim();
                        wcont_div = ASIS_DIA_PERM["DIVISION"].ToString().Trim();
                        wcve_pago = ASIS_DIA_PERM["CVE_PAGO"].ToString().Trim();
                        ws_fiscal = Convert.ToInt32(ASIS_DIA_PERM["S_FISCAL"]);
                        wa_fiscal = Convert.ToInt32(ASIS_DIA_PERM["A_FISCAL"]);
                        if (Convert.ToInt32(wclave_emp) > 90000)
                        {
                          if (Convert.ToInt32(wclave_emp) >= 95000)
                          {
                            wcont_div = "11211";
                            wcve_pago = "0";
                            wgrado_niv = 0;
                            wcategoria = "0";
                            wtipo_dia = "N";
                          }
                          else if (Convert.ToInt32(wclave_emp) >= 91000 && Convert.ToInt32(wclave_emp) <= 92000)
                          {
                            wcve_pago = "0";
                            wtipo_dia = "T";
                            wcategoria = "0";
                          }
                          else if (Convert.ToInt32(wclave_emp) >= 90000 && Convert.ToInt32(wclave_emp) <= 91000)
                          {
                            wcve_pago = "0";
                            wtipo_dia = "T";
                            wcategoria = "0";
                          }
                        }
                      }
                      cmdFestivo = new SqlCommand("INSERT INTO ASIS_DIA_PERM (CLAVE_EMP,FECHA_MOV,HORA_ENT,HORA_SAL,DEPTO,DIVISION,SUPERV, " +
                                                                             "CATEGORIA,HORARIO,TURNO,GRADO_NIV,CVE_PAGO,TIPO_MOV,OBSERVA,S_FISCAL,A_FISCAL,DIASEM) " +
                                                  "VALUES (@wclave_emp,@wfecha_lista,@whora_str,'?',@wdepto,@wcont_div,@wsupervisor, " +
                                                          "@wcategoria,@whorario,@wturno,@wgrado_niv,@wcve_pago,'T','FESTIVO TRABAJADO',@ws_fiscal,@wa_fiscal,@Wdiasem)", ConexionFestivo);
                      cmdFestivo.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                      cmdFestivo.Parameters.Add("@wfecha_lista", SqlDbType.Date).Value = wfechaRevAnt;
                      cmdFestivo.Parameters.Add("@wdepto", SqlDbType.Char).Value = wdepto;
                      cmdFestivo.Parameters.Add("@wcont_div", SqlDbType.Char).Value = wcont_div;
                      cmdFestivo.Parameters.Add("@wsupervisor", SqlDbType.Char).Value = wsupervisor;
                      cmdFestivo.Parameters.Add("@wcategoria", SqlDbType.Char).Value = wcategoria;
                      cmdFestivo.Parameters.Add("@whorario", SqlDbType.Char).Value = whorario;
                      cmdFestivo.Parameters.Add("@wturno", SqlDbType.Char).Value = wturno;
                      cmdFestivo.Parameters.Add("@wgrado_niv", SqlDbType.Int).Value = wgrado_niv;
                      cmdFestivo.Parameters.Add("@wcve_pago", SqlDbType.Char).Value = wcve_pago;
                      cmdFestivo.Parameters.Add("@wtipo_mov", SqlDbType.Char).Value = wtipo_dia;
                      cmdFestivo.Parameters.Add("@ws_fiscal", SqlDbType.Char).Value = ws_fiscal;
                      cmdFestivo.Parameters.Add("@wa_fiscal", SqlDbType.Char).Value = wa_fiscal;
                      cmdFestivo.Parameters.Add("@wdiasem", SqlDbType.Char).Value = wdiasem;
                      cmdFestivo.Parameters.Add("@whora_str", SqlDbType.Char).Value = whora_str;

                      try
                      {
                        //	ConexionFestivo.Open();
                        if (ConexionFestivo.State == ConnectionState.Closed) { ConexionFestivo.Open(); }
                        cmdFestivo.ExecuteNonQuery();
                      }
                      catch (SqlException ex)
                      {
                        FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(1575) | {0} | Error al insertar festivo ( {1} ).", wclave_emp, ex.Message));
                        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
                        FuncionesComunes.GuardaStackTraceLog(st);
                      }
                      finally
                      {
                        //cmdInsert.Dispose(); //TODO: Activar
                        ConexionFestivo.Close();
                      }

                      cmdActualiza = new SqlCommand("UPDATE MARCAJES SET MARCADO = 1 WHERE CLAVE_EMP=@wclave_Emp AND MARCAJE=@wmarcaje", ConexionFestivo);
                      cmdActualiza.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                      cmdActualiza.Parameters.Add("@wmarcaje", SqlDbType.DateTime).Value = wmarcaje;
                      daBusca.SelectCommand = cmdActualiza;

                      try
                      {
                        if (ConexionFestivo.State == ConnectionState.Closed) { ConexionFestivo.Open(); }
                        cmdActualiza.ExecuteNonQuery();
                      }
                      catch (SqlException ex)
                      {
                        FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(1592) | {0} | Error al actualizar marcado ( {1} ).", wclave_emp, ex.Message));
                        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
                        FuncionesComunes.GuardaStackTraceLog(st);
                      }
                      finally
                      {
                        //cmdInsert.Dispose(); //TODO: Activar
                        ConexionFestivo.Close();
                      }
                      #endregion

                    }
                    else if ((wturno == "2" || wturno == "3" || wturno == "4" || wturno == "5") && (wdifSalida < wdifEntrada))
                    {

                      #region SI NO ES DE TURNO 1 Y LA DIFERENCIA DE SALIDA ES MENOR A LA ENTRADA
                      string wtipo_movAnt = "";
                      cmdFestivo = new SqlCommand("SELECT HORA_ENT,HORA_SAL,HORA_ENT2,HORA_SAL2,HORA_ENT3,HORA_SAL3,TIPO_MOV FROM ASIS_DIA_PERM WHERE CLAVE_EMP=@wclave_emp AND FECHA_MOV=@wfecha_mov AND TIPO_MOV<>'F'", ConexionFestivo);
                      cmdFestivo.Parameters.Clear();
                      cmdFestivo.Parameters.Add("@wfecha_mov", SqlDbType.Date).Value = wfechaRevAnt.AddDays(-1);
                      cmdFestivo.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                      daFestivo.SelectCommand = cmdFestivo;
                      daFestivo.Fill(dt_Festivo);
                      foreach (DataRow ASIS_DIA_PERM in dt_Festivo.Rows)
                      {
                        whora_entAnt = ASIS_DIA_PERM["HORA_ENT"].ToString().Trim();
                        whora_salAnt = ASIS_DIA_PERM["HORA_SAL"].ToString().Trim();
                        whora_ent2Ant = ASIS_DIA_PERM["HORA_ENT2"].ToString().Trim();
                        whora_sal2Ant = ASIS_DIA_PERM["HORA_SAL2"].ToString().Trim();
                        whora_ent3Ant = ASIS_DIA_PERM["HORA_ENT3"].ToString().Trim();
                        whora_sal3Ant = ASIS_DIA_PERM["HORA_SAL3"].ToString().Trim();
                        wtipo_movAnt = ASIS_DIA_PERM["TIPO_MOV"].ToString().Trim();
                        if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5)
                        {
                          if (wtipo_movAnt == "T")
                          {
                            cmdFestivo = new SqlCommand("UPDATE ASIS_DIA_PERM SET HORA_SAL = @whora_str WHERE CLAVE_EMP=@wclave_Emp AND FECHA_MOV=@wfecha_mov AND TIPO_MOV='T'");
                          }
                          else if (wtipo_movAnt != "T")
                          {
                            cmdFestivo = new SqlCommand("UPDATE ASIS_DIA_PERM SET HORA_SAL = @whora_str WHERE CLAVE_EMP=@wclave_Emp AND FECHA_MOV=@wfecha_mov");
                          }
                          cmdFestivo.Parameters.Clear();
                          cmdFestivo.Parameters.Add("@wfecha_mov", SqlDbType.Date).Value = wfechaRevAnt.AddDays(-1);
                          cmdFestivo.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                          cmdFestivo.Parameters.Add("@whora_str", SqlDbType.Char).Value = whora_str;
                          cmdFestivo.Connection = ConexionFestivo;

                          try
                          {
                            if (ConexionFestivo.State == ConnectionState.Closed) { ConexionFestivo.Open(); }
                            //ConexionFestivo.Open();
                            cmdFestivo.ExecuteNonQuery();
                          }
                          catch (SqlException ex)
                          {
                            FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(1636) | {0} | Error al actualizar festivo ( {1} ).", wclave_emp, ex.Message));
                            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
                            FuncionesComunes.GuardaStackTraceLog(st);
                          }
                          finally
                          {
                            //cmdInsert.Dispose(); //TODO: Activar
                            ConexionFestivo.Close();
                          }

                          cmdActualiza = new SqlCommand("UPDATE MARCAJES SET MARCADO = 1 WHERE CLAVE_EMP=@wclave_Emp AND MARCAJE=@wmarcaje", ConexionFestivo);
                          cmdActualiza.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                          cmdActualiza.Parameters.Add("@wmarcaje", SqlDbType.DateTime).Value = wmarcaje;
                          daBusca.SelectCommand = cmdActualiza; //TODO: Revisar si es necesario ya que se utiliza un command

                          try
                          {
                            if (ConexionFestivo.State == ConnectionState.Closed) { ConexionFestivo.Open(); }
                            //ConexionFestivo.Open();
                            cmdActualiza.ExecuteNonQuery();
                          }
                          catch (SqlException ex)
                          {
                            FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(1654) | {0} | Error al actualizar marcado ( {1} ).", wclave_emp, ex.Message));
                            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
                            FuncionesComunes.GuardaStackTraceLog(st);
                          }
                          finally
                          {
                            //cmdInsert.Dispose(); //TODO: Activar
                            ConexionFestivo.Close();
                          }
                          //MODIFICACION:2015-05-25 FESTIVO TRABAJADO
                          //MODIFICACION:2015-05-25  FESTIVOS
                        }
                        else if ((Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && wdifEntrada < wdifSalida))
                        {
                          cmdFestivo = new SqlCommand("INSERT INTO ASIS_DIA_PERM (CLAVE_EMP,FECHA_MOV,HORA_ENT,HORA_SAL,DEPTO,DIVISION,SUPERV, " +
                                                                             "CATEGORIA,HORARIO,TURNO,GRADO_NIV,CVE_PAGO,TIPO_MOV,OBSERVA,S_FISCAL,A_FISCAL,DIASEM) " +
                                                  "VALUES (@wclave_emp,@wfecha_lista,@whora_str,'?',@wdepto,@wcont_div,@wsupervisor, " +
                                                          "@wcategoria,@whorario,@wturno,@wgrado_niv,@wcve_pago,'T','FESTIVO TRABAJADO',@ws_fiscal,@wa_fiscal,@Wdiasem)", ConexionFestivo);
                          wtipo_dia = "T";
                          wcve_pago = "";
                          cmdFestivo.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                          cmdFestivo.Parameters.Add("@wfecha_lista", SqlDbType.Date).Value = wfechaRevAnt;
                          cmdFestivo.Parameters.Add("@wdepto", SqlDbType.Char).Value = wdepto;
                          cmdFestivo.Parameters.Add("@wcont_div", SqlDbType.Char).Value = wcont_div;
                          cmdFestivo.Parameters.Add("@wsupervisor", SqlDbType.Char).Value = wsupervisor;
                          cmdFestivo.Parameters.Add("@wcategoria", SqlDbType.Char).Value = wcategoria;
                          cmdFestivo.Parameters.Add("@whorario", SqlDbType.Char).Value = whorario;
                          cmdFestivo.Parameters.Add("@wturno", SqlDbType.Char).Value = wturno;
                          cmdFestivo.Parameters.Add("@wgrado_niv", SqlDbType.Int).Value = wgrado_niv;
                          cmdFestivo.Parameters.Add("@wcve_pago", SqlDbType.Char).Value = wcve_pago;
                          cmdFestivo.Parameters.Add("@wtipo_mov", SqlDbType.Char).Value = wtipo_dia;
                          cmdFestivo.Parameters.Add("@ws_fiscal", SqlDbType.Char).Value = ws_fiscal;
                          cmdFestivo.Parameters.Add("@wa_fiscal", SqlDbType.Char).Value = wa_fiscal;
                          cmdFestivo.Parameters.Add("@wdiasem", SqlDbType.Char).Value = wdiasem;
                          cmdFestivo.Parameters.Add("@whora_str", SqlDbType.Char).Value = whora_str;
                          try
                          {
                            if (ConexionFestivo.State == ConnectionState.Closed) { ConexionFestivo.Open(); }
                            cmdFestivo.ExecuteNonQuery();
                          }
                          catch (SqlException ex)
                          {
                            FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(1689) | {0} | Error al insertar festivo ( {1} ).", wclave_emp, ex.Message));
                            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
                            FuncionesComunes.GuardaStackTraceLog(st);
                          }
                          finally
                          {
                            ConexionFestivo.Close();
                          }

                          cmdActualiza = new SqlCommand("UPDATE MARCAJES SET MARCADO = 1 WHERE CLAVE_EMP=@wclave_Emp AND MARCAJE=@wmarcaje", ConexionFestivo);
                          cmdActualiza.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                          cmdActualiza.Parameters.Add("@wmarcaje", SqlDbType.DateTime).Value = wmarcaje;
                          daBusca.SelectCommand = cmdActualiza;

                          try
                          {
                            if (ConexionFestivo.State == ConnectionState.Closed) { ConexionFestivo.Open(); }
                            cmdActualiza.ExecuteNonQuery();
                          }
                          catch (SqlException ex)
                          {
                            FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(1705) | {0} | Error al actualizar marcado ( {1} ).", wclave_emp, ex.Message));
                            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
                            FuncionesComunes.GuardaStackTraceLog(st);
                          }
                          finally
                          {
                            //cmdInsert.Dispose(); //TODO: Activar
                            ConexionFestivo.Close();
                          }
                        }
                        else if ((wturno == "2" || wturno == "3" || wturno == "4" || wturno == "5") && (wdifEntrada > wdifSalida) && Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && Convert.ToInt32(whora_entAnt.Trim().Length) < 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5)
                        {
                          cmdFestivo = new SqlCommand("INSERT INTO ASIS_DIA_PERM (CLAVE_EMP,FECHA_MOV,HORA_ENT,HORA_SAL,DEPTO,DIVISION,SUPERV, " +
                                                                 "CATEGORIA,HORARIO,TURNO,GRADO_NIV,CVE_PAGO,TIPO_MOV,OBSERVA,S_FISCAL,A_FISCAL,DIASEM) " +
                                      "VALUES (@wclave_emp,@wfecha_lista,@whora_str,'?',@wdepto,@wcont_div,@wsupervisor, " +
                                              "@wcategoria,@whorario,@wturno,@wgrado_niv,@wcve_pago,'T','FESTIVO TRABAJADO',@ws_fiscal,@wa_fiscal,@Wdiasem)", ConexionFestivo);
                          wtipo_dia = "T";
                          wcve_pago = "";
                          cmdFestivo.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                          cmdFestivo.Parameters.Add("@wfecha_lista", SqlDbType.Date).Value = wfechaRevAnt;
                          cmdFestivo.Parameters.Add("@wdepto", SqlDbType.Char).Value = wdepto;
                          cmdFestivo.Parameters.Add("@wcont_div", SqlDbType.Char).Value = wcont_div;
                          cmdFestivo.Parameters.Add("@wsupervisor", SqlDbType.Char).Value = wsupervisor;
                          cmdFestivo.Parameters.Add("@wcategoria", SqlDbType.Char).Value = wcategoria;
                          cmdFestivo.Parameters.Add("@whorario", SqlDbType.Char).Value = whorario;
                          cmdFestivo.Parameters.Add("@wturno", SqlDbType.Char).Value = wturno;
                          cmdFestivo.Parameters.Add("@wgrado_niv", SqlDbType.Int).Value = wgrado_niv;
                          cmdFestivo.Parameters.Add("@wcve_pago", SqlDbType.Char).Value = wcve_pago;
                          cmdFestivo.Parameters.Add("@wtipo_mov", SqlDbType.Char).Value = wtipo_dia;
                          cmdFestivo.Parameters.Add("@ws_fiscal", SqlDbType.Char).Value = ws_fiscal;
                          cmdFestivo.Parameters.Add("@wa_fiscal", SqlDbType.Char).Value = wa_fiscal;
                          cmdFestivo.Parameters.Add("@wdiasem", SqlDbType.Char).Value = wdiasem;
                          cmdFestivo.Parameters.Add("@whora_str", SqlDbType.Char).Value = whora_str;

                          try
                          {
                            if (ConexionFestivo.State == ConnectionState.Closed) { ConexionFestivo.Open(); }
                            cmdFestivo.ExecuteNonQuery();
                          }
                          catch (SqlException ex)
                          {
                            FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(1739) | {0} | Error al insertar festivo ( {1} ).", wclave_emp, ex.Message));
                            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
                            FuncionesComunes.GuardaStackTraceLog(st);
                          }
                          finally
                          {
                            ConexionFestivo.Close();
                          }

                          cmdActualiza = new SqlCommand("UPDATE MARCAJES SET MARCADO = 1 WHERE CLAVE_EMP=@wclave_Emp AND MARCAJE=@wmarcaje", ConexionFestivo);
                          cmdActualiza.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                          cmdActualiza.Parameters.Add("@wmarcaje", SqlDbType.DateTime).Value = wmarcaje;
                          daBusca.SelectCommand = cmdActualiza;

                          try
                          {
                            if (ConexionFestivo.State == ConnectionState.Closed) { ConexionFestivo.Open(); }
                            cmdActualiza.ExecuteNonQuery();
                          }
                          catch (SqlException ex)
                          {
                            FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(1755) | {0} | Error al actualizar marcado ( {1} ).", wclave_emp, ex.Message));
                            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
                            FuncionesComunes.GuardaStackTraceLog(st);
                          }
                          finally
                          {
                            //cmdInsert.Dispose(); //TODO: Activar
                            ConexionFestivo.Close();
                          }
                        }
                        else if ((wturno == "2" || wturno == "3" || wturno == "4" || wturno == "5") && (wdifEntrada < wdifSalida))
                        {
                          cmdFestivo = new SqlCommand("INSERT INTO ASIS_DIA_PERM (CLAVE_EMP,FECHA_MOV,HORA_ENT,HORA_SAL,DEPTO,DIVISION,SUPERV, " +
                                                                           "CATEGORIA,HORARIO,TURNO,GRADO_NIV,CVE_PAGO,TIPO_MOV,OBSERVA,S_FISCAL,A_FISCAL,DIASEM) " +
                                                "VALUES (@wclave_emp,@wfecha_lista,@whora_str,'?',@wdepto,@wcont_div,@wsupervisor, " +
                                                        "@wcategoria,@whorario,@wturno,@wgrado_niv,@wcve_pago,'T','FESTIVO TRABAJADO',@ws_fiscal,@wa_fiscal,@Wdiasem)", ConexionFestivo);
                          wtipo_dia = "T";
                          wcve_pago = "";
                          cmdFestivo.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                          cmdFestivo.Parameters.Add("@wfecha_lista", SqlDbType.Date).Value = wfechaRevAnt;
                          cmdFestivo.Parameters.Add("@wdepto", SqlDbType.Char).Value = wdepto;
                          cmdFestivo.Parameters.Add("@wcont_div", SqlDbType.Char).Value = wcont_div;
                          cmdFestivo.Parameters.Add("@wsupervisor", SqlDbType.Char).Value = wsupervisor;
                          cmdFestivo.Parameters.Add("@wcategoria", SqlDbType.Char).Value = wcategoria;
                          cmdFestivo.Parameters.Add("@whorario", SqlDbType.Char).Value = whorario;
                          cmdFestivo.Parameters.Add("@wturno", SqlDbType.Char).Value = wturno;
                          cmdFestivo.Parameters.Add("@wgrado_niv", SqlDbType.Int).Value = wgrado_niv;
                          cmdFestivo.Parameters.Add("@wcve_pago", SqlDbType.Char).Value = wcve_pago;
                          cmdFestivo.Parameters.Add("@wtipo_mov", SqlDbType.Char).Value = wtipo_dia;
                          cmdFestivo.Parameters.Add("@ws_fiscal", SqlDbType.Char).Value = ws_fiscal;
                          cmdFestivo.Parameters.Add("@wa_fiscal", SqlDbType.Char).Value = wa_fiscal;
                          cmdFestivo.Parameters.Add("@wdiasem", SqlDbType.Char).Value = wdiasem;
                          cmdFestivo.Parameters.Add("@whora_str", SqlDbType.Char).Value = whora_str;

                          try
                          {
                            if (ConexionFestivo.State == ConnectionState.Closed) { ConexionFestivo.Open(); }
                            cmdFestivo.ExecuteNonQuery();
                          }
                          catch (SqlException ex)
                          {
                            FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(1789) | {0} | Error al insertar festivo ( {1} ).", wclave_emp, ex.Message));
                            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
                            FuncionesComunes.GuardaStackTraceLog(st);
                          }
                          finally
                          {
                            ConexionFestivo.Close();
                          }

                          cmdActualiza = new SqlCommand("UPDATE MARCAJES SET MARCADO = 1 WHERE CLAVE_EMP=@wclave_Emp AND MARCAJE=@wmarcaje", ConexionFestivo);
                          cmdActualiza.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                          cmdActualiza.Parameters.Add("@wmarcaje", SqlDbType.DateTime).Value = wmarcaje;
                          daBusca.SelectCommand = cmdActualiza;

                          try
                          {
                            if (ConexionFestivo.State == ConnectionState.Closed) { ConexionFestivo.Open(); }
                            cmdActualiza.ExecuteNonQuery();
                          }
                          catch (SqlException ex)
                          {
                            FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(1805) | {0} | Error al actualizar marcado ( {1} ).", wclave_emp, ex.Message));
                            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
                            FuncionesComunes.GuardaStackTraceLog(st);
                          }
                          finally
                          {
                            //cmdInsert.Dispose(); //TODO: Activar
                            ConexionFestivo.Close();
                          }

                        }
                      }
                      #endregion

                    }

                    #endregion
                  }
                  if (wtotal == 1)
                  {

                    #region HAY SOLO UN REGISTRO DE DIA FESTIVO TRABAJADO
                    cmdFestivo = new SqlCommand("SELECT HORA_ENT,HORA_SAL,HORA_ENT2,HORA_SAL2,HORA_ENT3,HORA_SAL3,HORA_ENT4,HORA_SAL4  FROM ASIS_DIA_PERM WHERE CLAVE_EMP=@wclave_emp AND FECHA_MOV=@wfecha_lista AND TIPO_MOV='T'", ConexionFestivo);
                    cmdFestivo.Parameters.Add("@wfecha_lista", SqlDbType.Date).Value = wfechaRevAnt;
                    cmdFestivo.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                    daFestivo.SelectCommand = cmdFestivo;
                    daFestivo.Fill(dt_Festivo);

                    foreach (DataRow ASIS_DIA_PERM in dt_Festivo.Rows)
                    {    //INICIA SELECCION EN ASIS_DIA_PERM
                      whora_ent = ASIS_DIA_PERM["HORA_ENT"].ToString().Trim();
                      whora_sal = ASIS_DIA_PERM["HORA_SAL"].ToString().Trim();
                      whora_ent2 = ASIS_DIA_PERM["HORA_ENT2"].ToString().Trim();
                      whora_sal2 = ASIS_DIA_PERM["HORA_SAL2"].ToString().Trim();
                      whora_ent3 = ASIS_DIA_PERM["HORA_ENT3"].ToString().Trim();
                      whora_sal3 = ASIS_DIA_PERM["HORA_SAL3"].ToString().Trim();
                      whora_ent4 = ASIS_DIA_PERM["HORA_ENT"].ToString().Trim();
                      whora_sal4 = ASIS_DIA_PERM["HORA_SAL"].ToString().Trim();
                      if (Convert.ToInt32(whora_sal.Trim().Length) < 5 && Convert.ToInt32(whora_ent.Trim().Length) == 5)
                      {
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
                        if (wtipo_reg == "V")
                        {
                          cmdFestivo.CommandText = "UPDATE ASIS_DIA_PERM SET HORA_SAL = @whora_str  WHERE CLAVE_EMP=@wclave_Emp AND FECHA_MOV=@wfecha_lista AND TIPO_MOV='T'";
                        }
                        wProcesado = true;
                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) < 5)
                      {
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_sal, whora_str);
                        if (wtipo_reg == "V")
                        {
                          cmdFestivo.CommandText = "UPDATE ASIS_DIA_PERM SET HORA_ENT2 = @whora_str, HORA_SAL2='?' WHERE CLAVE_EMP=@wclave_Emp AND FECHA_MOV=@wfecha_lista AND TIPO_MOV='T'";
                        }
                        wProcesado = true;
                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) < 5)
                      {
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_ent2, whora_str);
                        if (wtipo_reg == "V")
                        {
                          cmdFestivo.CommandText = "UPDATE ASIS_DIA_PERM SET HORA_SAL2 = @whora_str WHERE CLAVE_EMP=@wclave_Emp AND FECHA_MOV=@wfecha_lista AND TIPO_MOV='T'";
                        }
                        wProcesado = true;
                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) == 5 && Convert.ToInt32(whora_ent3.Trim().Length) < 5)
                      {
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_sal2, whora_str);
                        if (wtipo_reg == "V")
                        {
                          cmdFestivo.CommandText = "UPDATE ASIS_DIA_PERM SET HORA_ENT3 = @whora_str, HORA_SAL3='?' WHERE CLAVE_EMP=@wclave_Emp AND    FECHA_MOV=@wfecha_lista AND TIPO_MOV='T'";
                        }
                        wProcesado = true;
                      }
                      else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) < 5 && Convert.ToInt32(whora_ent3.Trim().Length) == 5 && Convert.ToInt32(whora_sal3.Trim().Length) < 5)
                      {
                        wbandera = 1;
                        wtipo_reg = Diferencia_Marcaje(whora_ent3, whora_str);
                        if (wtipo_reg == "V")
                        {
                          cmdFestivo.CommandText = "UPDATE ASIS_DIA_PERM SET HORA_SAL3 = @whora_str WHERE CLAVE_EMP=@wclave_Emp AND FECHA_MOV=@wfecha_lista AND TIPO_MOV='T'";
                        }
                        wProcesado = true;
                      }
                      if (wtipo_reg == "V")
                      {
                        cmdFestivo.Parameters.Add("@whora_str", SqlDbType.Char).Value = whora_str;
                        try
                        {
                          if (ConexionFestivo.State == ConnectionState.Closed) { ConexionFestivo.Open(); }
                          cmdFestivo.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                          FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(1881) | {0} | Error al actualizar festivo ( {1} ).", wclave_emp, ex.Message));
                          System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
                          FuncionesComunes.GuardaStackTraceLog(st);
                        }
                        finally
                        {
                          //cmdInsert.Dispose(); //TODO: Activar
                          ConexionFestivo.Close();
                        }
                      }
                      cmdActualiza = new SqlCommand("UPDATE MARCAJES SET MARCADO = 1 WHERE CLAVE_EMP=@wclave_Emp AND MARCAJE=@wmarcaje", ConexionFestivo);
                      cmdActualiza.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                      cmdActualiza.Parameters.Add("@wmarcaje", SqlDbType.DateTime).Value = wmarcaje;
                      daBusca.SelectCommand = cmdActualiza;
                      try
                      {
                        if (ConexionFestivo.State == ConnectionState.Closed) { ConexionFestivo.Open(); }
                        cmdActualiza.ExecuteNonQuery();
                      }
                      catch (SqlException ex)
                      {
                        FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(1897) | {0} | Error al actualizar marcado ( {1} ).", wclave_emp, ex.Message));
                        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
                        FuncionesComunes.GuardaStackTraceLog(st);
                      }
                      finally
                      {
                        //cmdInsert.Dispose(); //TODO: Activar
                        ConexionFestivo.Close();
                      }
                    }
                    #endregion

                  }
                  #endregion

                }
                #endregion

              }
              else if (whoras_nor == 0)
              {

                #region DIA NO LABORABLE
                switch (wturno)
                {
                  #region DESCANSO TURNO 1
                  case "1":
                    if (Convert.ToInt32(whora_ent.Trim().Length) < 5 && wProcesado == false)
                    {

                      #region SI NO TIENE ENTRADA 1 Y EL MARCAJE NO HA SIDO PROCESADO
                      wcampo = "HORA_ENT";
                      wcampo2 = "HORA_SAL";
                      wbandera = 1;
                      wtipo_reg = "V";
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && wProcesado == false)
                    {

                      #region SI TIENE ENTRADA 1 Y NO TIENE SALIDA 2 Y EL MARCAJE NO SE HA PROCESADO
                      wcampo = "HORA_SAL";
                      wbandera = 0;
                      wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) < 5 && wProcesado == false)
                    {

                      #region SI TIENE PAR 1 Y NO TIENE ENTRADA 2 Y EL MARCAJE NO HA SIDO PROCESADO
                      wcampo = "HORA_ENT2";
                      wcampo2 = "HORA_SAL2";
                      wbandera = 1;
                      wtipo_reg = Diferencia_Marcaje(whora_sal, whora_str);
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) < 5 && wProcesado == false)
                    {

                      #region SI TIENE PAR 1 Y TIENE ENTRADA 2 Y NO TIENE SALIDA 2 Y EL MARCAJE NO HA SIDO PROCESADO
                      wcampo = "HORA_SAL2";
                      wbandera = 0;
                      wtipo_reg = Diferencia_Marcaje(whora_ent2, whora_str);
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) == 5 && Convert.ToInt32(whora_ent3.Trim().Length) < 5 && wProcesado == false)
                    {

                      #region SI TIENE PAR 1 Y TIENE PAR 2 Y NO TIENE ENTRADA 3 Y EL MARCAJE NO HA SIDO PROCESADO
                      wcampo = "HORA_ENT3";
                      wcampo2 = "HORA_SAL3";
                      wbandera = 1;
                      wtipo_reg = Diferencia_Marcaje(whora_sal2, whora_str);
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) == 5 && Convert.ToInt32(whora_ent3.Trim().Length) == 5 && Convert.ToInt32(whora_sal3.Trim().Length) < 5 && wProcesado == false)
                    {

                      #region SI TIENE PAR 1 Y TIENE PAR 2 Y TIENE ENTRADA 2 Y NO TIENE SALIDA 3 Y EL MARCAJE NO HA SIDO PROCESADO
                      wcampo = "HORA_SAL3";
                      wbandera = 0;
                      wtipo_reg = Diferencia_Marcaje(whora_ent3, whora_str);
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else continue;
                    break;
                  #endregion TURNO1_DESCANSO

                  #region DESCANSO TURNO 2 A 5
                  case "2":
                  case "3":
                  case "4":
                  case "5":
                    //	int whoras_norAnt = 0;
                    string wdia_horarioAnt = "";
                    if (Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && wProcesado == false)
                    {

                      #region SI NO TIENE PAR 1 Y EL MARCAJE NO SE HA PROCESADO
                      cmdDia_Ant = new SqlCommand();
                      cmdDia_Ant.CommandText = "SELECT HORA_ENT,HORA_SAL,HORA_ENT2,HORA_SAL2,HORA_ENT3,HORA_SAL3,HORAS_NOR,DIASEM,TIPO_MOV FROM ASIS_DIA_PERM WHERE CLAVE_EMP=@wclave_emp AND FECHA_MOV=@wfecha_mov AND TIPO_MOV <> 'F'";
                      cmdDia_Ant.Parameters.Clear();
                      cmdDia_Ant.Parameters.Add("@wfecha_mov", SqlDbType.Date).Value = wfechaRevAnt.AddDays(-1);
                      cmdDia_Ant.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                      cmdDia_Ant.Connection = ConexionFestivo;
                      daBusca.SelectCommand = cmdDia_Ant;
                      dtDia_Ant.Clear();
                      daBusca.Fill(dtDia_Ant);
                      foreach (DataRow ASIS_DIA_PERM in dtDia_Ant.Rows)
                      {
                        whora_entAnt = ASIS_DIA_PERM["HORA_ENT"].ToString().Trim();
                        whora_salAnt = ASIS_DIA_PERM["HORA_SAL"].ToString().Trim();
                        whora_ent2Ant = ASIS_DIA_PERM["HORA_ENT2"].ToString().Trim();
                        whora_sal2Ant = ASIS_DIA_PERM["HORA_SAL2"].ToString().Trim();
                        whora_ent3Ant = ASIS_DIA_PERM["HORA_ENT3"].ToString().Trim();
                        whora_sal3Ant = ASIS_DIA_PERM["HORA_SAL3"].ToString().Trim();
                        whoras_norAnt = Convert.ToInt32(ASIS_DIA_PERM["HORAS_NOR"]);
                        wdia_horarioAnt = ASIS_DIA_PERM["DIASEM"].ToString().Trim();
                        wdia_horarioAnt = wdia_horarioAnt.Replace("é", "e").Replace("á", "a").ToUpper();
                        wtipo_mov = ASIS_DIA_PERM["TIPO_MOV"].ToString().Trim();
                        wtipo_fest = false;
                        if (wtipo_mov == "F") wtipo_fest = true;
                        if (wtipo_mov == "T")
                        {
                          cmdDia_Ant2 = new SqlCommand("SELECT HORAS_NOR FROM ASIS_DIA_PERM WHERE CLAVE_EMP=@wclave_emp AND FECHA_MOV=@wfecha_mov AND TIPO_MOV = 'F'", ConexionFestivo);
                          //	cmdDia_Ant2.Parameters.Clear();
                          cmdDia_Ant2.Parameters.Add("@wfecha_mov", SqlDbType.Date).Value = wfechaRevAnt.AddDays(-1);
                          cmdDia_Ant2.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
                          //		cmdDia_Ant.Connection = ConexionFestivo;
                          daBusca2 = new SqlDataAdapter();
                          daBusca2.SelectCommand = cmdDia_Ant2;
                          dtDia_Ant2 = new DataTable();
                          daBusca2.Fill(dtDia_Ant2);
                          whoras_norAnt = Convert.ToInt16(dtDia_Ant2.Rows[0]["HORAS_NOR"]);
                        }
                        if (whoras_norAnt > 0)
                        {
                          switch (wdia_horarioAnt)
                          {     //ENTRADA Y SALIDA DE HORARIOS
                            case "LUNES":
                              if (!dtDiaHorario.Rows[0]["LUNES"].ToString().Trim().Equals(String.Empty))
                              {
                                wEntrada = dtDiaHorario.Rows[0]["LUNES"].ToString().Trim().ToUpper().Substring(0, 5);
                                wSalida = dtDiaHorario.Rows[0]["LUNES"].ToString().Trim().ToUpper().Substring(5, 5);
                              }
                              break;
                            case "MARTES":
                              if (!dtDiaHorario.Rows[0]["MARTES"].ToString().Trim().Equals(String.Empty))
                              {
                                wEntrada = dtDiaHorario.Rows[0]["MARTES"].ToString().Trim().ToUpper().Substring(0, 5);
                                wSalida = dtDiaHorario.Rows[0]["MARTES"].ToString().Trim().ToUpper().Substring(5, 5);
                              }
                              break;
                            case "MIERCOLES":
                              if (!dtDiaHorario.Rows[0]["MIERCOLES"].ToString().Trim().Equals(String.Empty))
                              {
                                wEntrada = dtDiaHorario.Rows[0]["MIERCOLES"].ToString().Trim().ToUpper().Substring(0, 5);
                                wSalida = dtDiaHorario.Rows[0]["MIERCOLES"].ToString().Trim().ToUpper().Substring(5, 5);
                              }
                              break;
                            case "JUEVES":
                              if (!dtDiaHorario.Rows[0]["JUEVES"].ToString().Trim().Equals(String.Empty))
                              {
                                wEntrada = dtDiaHorario.Rows[0]["JUEVES"].ToString().Trim().ToUpper().Substring(0, 5);
                                wSalida = dtDiaHorario.Rows[0]["JUEVES"].ToString().Trim().ToUpper().Substring(5, 5);
                              }
                              break;
                            case "VIERNES":
                              if (!dtDiaHorario.Rows[0]["VIERNES"].ToString().Trim().Equals(String.Empty))
                              {
                                wEntrada = dtDiaHorario.Rows[0]["VIERNES"].ToString().Trim().ToUpper().Substring(0, 5);
                                wSalida = dtDiaHorario.Rows[0]["VIERNES"].ToString().Trim().ToUpper().Substring(5, 5);
                              }
                              break;
                            case "SABADO":
                              if (!dtDiaHorario.Rows[0]["SABADO"].ToString().Trim().Equals(String.Empty))
                              {
                                wEntrada = dtDiaHorario.Rows[0]["SABADO"].ToString().Trim().ToUpper().Substring(0, 5);
                                wSalida = dtDiaHorario.Rows[0]["SABADO"].ToString().Trim().ToUpper().Substring(5, 5);
                              }
                              break;
                            case "DOMINGO":
                              if (!dtDiaHorario.Rows[0]["DOMINGO"].ToString().Trim().Equals(String.Empty))
                              {
                                wEntrada = dtDiaHorario.Rows[0]["DOMINGO"].ToString().Trim().ToUpper().Substring(0, 5);
                                wSalida = dtDiaHorario.Rows[0]["DOMINGO"].ToString().Trim().ToUpper().Substring(5, 5);
                              }
                              break;
                          }
                          if (wSalida.Trim() == "24:00") wSalida = "00:00";
                          //wEntrada = wfechaRevAnt.ToShortDateString() + " " + wEntrada;
                          wEntrada = wfechaRevAnt.ToString("yyyy-MM-dd") + " " + wEntrada;
                          //FuncionesComunes.GuardarLog(String.Format("Empleado {1} | wEntrada={0}", wEntrada, wclave_emp));
                          if (wEntrada.Trim().Substring(11, 5) == "24:00") wEntrada = "00:00";
                          wEntrada_Horario = Convert.ToDateTime(wEntrada);
                          //wSalida = wfechaRevAnt.ToShortDateString() + " " + wSalida;
                          wSalida = wfechaRevAnt.ToString("yyyy-MM-dd") + " " + wSalida;
                          if (wSalida.Trim().Substring(11, 5) == "24:00") wSalida = "00:00";

                          wSalida_Horario = Convert.ToDateTime(wSalida);

                          went = Convert.ToDouble(wEntrada_Horario.Hour);
                          wsal = Convert.ToDouble(wSalida_Horario.Hour);
                          wmar = Convert.ToDouble(wmarcaje.Hour);
                          wdifEntrada = (wEntrada_Horario.Subtract(wmarcaje).TotalMinutes);
                          wdifSalida = (wSalida_Horario.Subtract(wmarcaje).TotalMinutes);

                          if (wturno == "3" && (wmar >= 0 && wmar < 2)) wdifEntrada = ((wmar + 24) - went) * 60;

                          wdifEntrada = Math.Abs(wdifEntrada);
                          wdifSalida = Math.Abs(wdifSalida);
                        }
                        if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5 && wProcesado == false)
                        {
                          wcampo = "HORA_SAL";
                          wbandera = 2;
                          wtipo_reg = Diferencia_Marcaje(whora_entAnt, whora_str);
                          Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                          wProcesado = true;
                          //CAMBIO: 2015-04-17
                        }
                        else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && wProcesado == false && (wturno == "2" || wturno == "3") && (wdifSalida < wdifEntrada && wmar < 3))
                        {
                          wcampo = "HORA_ENT2";
                          wcampo2 = "HORA_SAL2";
                          wbandera = 2;
                          wtipo_reg = Diferencia_Marcaje(whora_salAnt, whora_str);
                          Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                          wProcesado = true;
                        }
                        else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && wProcesado == false && wdifSalida > wdifEntrada)
                        {
                          wcampo = "HORA_ENT";
                          wcampo2 = "HORA_SAL";
                          wbandera = 1;
                          wtipo_reg = "V";
                          Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                          wProcesado = true;
                        }
                        else if (Convert.ToInt32(whora_entAnt.Trim().Length) < 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5 && wProcesado == false && (wdifSalida < wdifEntrada) && whoras_norAnt > 0)
                        {
                          wcampo = "HORA_SAL";
                          wcampo2 = "HORA_ENT";
                          wbandera = 3;
                          wtipo_reg = "V";
                          Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                          wProcesado = true;
                        }
                      }
                      #endregion

                    }
                    if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && wProcesado == false)
                    {

                      #region SI TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y EL MARCAJE NO SE HA PROCESADO
                      wcampo = "HORA_SAL";
                      wbandera = 0;
                      wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) < 5 && wProcesado == false)
                    {

                      #region SI TIENE PAR 1 Y NO TIENE ENTRADA 2 Y EL MARCAJE NO SE HA PROCESADO
                      wcampo = "HORA_ENT2";
                      wcampo2 = "HORA_SAL2";
                      wbandera = 1;
                      wtipo_reg = Diferencia_Marcaje(whora_sal, whora_str);
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) < 5 && wProcesado == false)
                    {

                      #region SI TIENE PAR 1 Y TIENE ENTRADA 2 Y NO TIENE SALIDA 2 Y EL MARCAJE NO SE HA PROCESADO
                      wcampo = "HORA_SAL2";
                      wbandera = 0;
                      wtipo_reg = Diferencia_Marcaje(whora_ent2, whora_str);
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest && wProcesado == false);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) == 5 && Convert.ToInt32(whora_ent3.Trim().Length) < 5 && wProcesado == false)
                    {

                      #region SI TIENE PAR 1 Y PAR 2 Y NO TIENE ENTRADA 3 Y EL MARCAJE NO SE HA PROCESADO
                      wcampo = "HORA_ENT3";
                      wcampo2 = "HORA_SAL3";
                      wbandera = 1;
                      wtipo_reg = Diferencia_Marcaje(whora_sal2, whora_str);
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) == 5 && Convert.ToInt32(whora_ent3.Trim().Length) == 5 && Convert.ToInt32(whora_sal3.Trim().Length) < 5 && wProcesado == false)
                    {

                      #region SI TIENE PAR 1 Y PAR 2 Y TIENE ENTRADA 3 Y NO TIENE SALIDA 3 Y EL MARCAJE NO SE HA PROCESADO
                      wcampo = "HORA_SAL3";
                      wbandera = 0;
                      wtipo_reg = Diferencia_Marcaje(whora_ent3, whora_str);
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_entAnt.Trim().Length) < 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5 && (wturno != "3") && (wturno != "5") && wProcesado == false)
                    {

                      #region SI EL DIA ANTERIOR NO TIENE ENTRADA 1 NI SALIDA 1 Y EL TURNO ES DIFERENTE DE 3 Y DE 5 Y EL MARCAJE NO SE HA PROCESADO
                      wcampo = "HORA_ENT";
                      wcampo2 = "HORA_SAL";
                      wbandera = 1;
                      wtipo_reg = "V";
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                      #region CONDICIONES REPETIDAS
                      /*} else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && wProcesado == false) {

											#region [REPETIDO-COMENTADO] SI TIENE ENTRADA 1 Y NO TIENE SALIDA 1 Y EL MARCAJE NO SE HA PROCESADO
											//wcampo = "HORA_SAL";
											//wbandera = 0;
											//wtipo_reg = Diferencia_Marcaje(whora_ent, whora_str);
											//Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
											//wProcesado = true;
											#endregion

										} else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) < 5 && wProcesado == false) {

											#region [REPETIDO-COMENTADO] SI TIENE PAR 1 Y NO TIENE ENTRADA 2 Y EL MARCAJE NO SE HA PROCESADO
											//wcampo = "HORA_ENT2";
											//wcampo2 = "HORA_SAL2";
											//wbandera = 1;
											//wtipo_reg = Diferencia_Marcaje(whora_sal, whora_str);
											//Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
											//wProcesado = true;
											#endregion

										} else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) < 5 && wProcesado == false) {

											#region [REPETIDO-COMENTADO] SI TIENE PAR 1 Y TIENE ENTRADA 2 Y NO TIENE SALIDA 2 Y EL MARCAJE NO SE HA PROCESADO
											//wcampo = "HORA_SAL2";
											//wbandera = 0;
											//wtipo_reg = Diferencia_Marcaje(whora_ent2, whora_str);
											//Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
											//wProcesado = true;
											#endregion

										} else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) == 5 && Convert.ToInt32(whora_ent3.Trim().Length) < 5 && wProcesado == false) {

											#region [REPETIDO/INCOMPLETO-COMENTADO] SI TIENE PAR 1 Y PAR 2 Y NO TIENE ENTRADA 3 Y EL MARCAJE NO SE HA PROCESADO
											//wcampo = "HORA_ENT3";
											//wcampo2 = "HORA_SAL3";
											//wbandera = 1;
											//wtipo_reg = Diferencia_Marcaje(whora_sal2, whora_str);
											#endregion

										} else if (Convert.ToInt32(whora_ent.Trim().Length) == 5 && Convert.ToInt32(whora_sal.Trim().Length) == 5 && Convert.ToInt32(whora_ent2.Trim().Length) == 5 && Convert.ToInt32(whora_sal2.Trim().Length) == 5 && Convert.ToInt32(whora_ent3.Trim().Length) == 5 && Convert.ToInt32(whora_sal3.Trim().Length) < 5 && wProcesado == false) {

											#region [REPETIDO-COMENTADO] SI TIENE PAR 1 Y PAR 2 Y TIENE ENTRADA 3 Y NO TIENE SALIDA 3 Y EL MARCAJE NO SE HA PROCESADO
											//wcampo = "HORA_SAL3";
											//wbandera = 0;
											//wtipo_reg = Diferencia_Marcaje(whora_ent3, whora_str);
											//Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
											//wProcesado = true;
											#endregion
											*/
                      #endregion
                    }
                    else if (Convert.ToInt32(whora_entAnt.Trim().Length) < 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5 && wProcesado == false && (wturno == "3") && (wmar >= 0 && wmar < 2))
                    {

                      #region SI EL DIA ANTERIOR NO TIENE PAR 1 Y EL MARCAJE NO SE HA PROCESADO Y ES DE TURNO 3 Y EL MARCAJE ES ENTRE LAS 00:00 Y LAS 02:00
                      wcampo = "HORA_ENT";
                      wcampo2 = "HORA_SAL";
                      wbandera = 3;
                      wtipo_reg = "V";
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_entAnt.Trim().Length) < 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5 && wProcesado == false && (wturno == "3") && (wmar >= 5) && whoras_norAnt > 0 && whoras_nor == 0)
                    {

                      #region	SI EL DIA ANTERIOR NO TIENE PAR 1 Y EL MARCAJE NO SE HA PROCESADO Y ES DE TURNO 3 Y EL MARCAJE ES DESPUES LAS 05:00 Y EL DIA ANTERIOR ES LABORABLE Y EL DIA ACTUAL NO ES LABORABLE
                      wcampo = "HORA_ENT";
                      wcampo2 = "HORA_SAL";
                      wbandera = 3;
                      wtipo_reg = "V";
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && wProcesado == false && (wmar >= 15))
                    {

                      #region SI NO TIENE PAR 1 Y EL MARCAJE NO SE HA PROCESADO Y EL MARCAJE ES DESPUES DE LAS 15:00
                      wcampo = "HORA_ENT";
                      wcampo2 = "HORA_SAL";
                      wbandera = 1;
                      wtipo_reg = "V";
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && wProcesado == false && (wturno == "5"))
                    {
                      //PUEDE UNIRSE CON LA CONDICION ANTERIOR
                      #region SI NO TIENE PAR 1 Y EL MARCAJE NO SE HA PROCESADO Y ES DE TURNO 5
                      wcampo = "HORA_ENT";
                      wcampo2 = "HORA_SAL";
                      wbandera = 1;
                      wtipo_reg = "V";
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && wmar >= 3 && wturno == "2" && wProcesado == false)
                    {

                      #region SI EL DIA ANTERIOR TIENE PAR 1 Y EL DIA ACTUAL NO TIENE PAR 1 Y EL MARCAJE ES MAYOR A LAS 03:00 Y ES DE TURNO 2 Y EL MARCAJE NO SE HA PROCESADO
                      wcampo = "HORA_ENT";
                      wcampo2 = "HORA_SAL";
                      wbandera = 1;
                      wtipo_reg = "V";
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_entAnt.Trim().Length) < 5 && Convert.ToInt32(whora_salAnt.Trim().Length) < 5 && Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && whoras_norAnt == 0 && whoras_nor == 0 && wturno == "3" && wProcesado == false)
                    {

                      #region SI EL DIA ANTERIOR NO TIENE PAR 1 Y EL DIA ACTUAL NO TIENE PAR 1 Y EL DIA ANTERIOR NO ES LABORABLE Y EL DIA ACTUAL NO ES LABORABLE Y ES DE TURNO 3 Y EL MARCAJE NO SE HA PROCESADO
                      wcampo = "HORA_ENT";
                      wcampo2 = "HORA_SAL";
                      wbandera = 1;
                      wtipo_reg = "V";
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else if (Convert.ToInt32(whora_entAnt.Trim().Length) == 5 && Convert.ToInt32(whora_salAnt.Trim().Length) == 5 && Convert.ToInt32(whora_ent.Trim().Length) < 5 && Convert.ToInt32(whora_sal.Trim().Length) < 5 && whoras_norAnt > 0 && whoras_nor == 0 && wturno == "3" && wProcesado == false)
                    {

                      #region SI EL DIA ANTERIOR TIENE PAR 1 Y EL DIA ACTUAL NO TIENE PAR 1 Y EL DIA ANTERIOR ES LABORABLE Y EL DIA ACTUAL NO ES LABORABLE Y ES DE TURNO 3 Y EL MARCAJE NO SE HA PROCESADO
                      wcampo = "HORA_ENT";
                      wcampo2 = "HORA_SAL";
                      wtipo_reg = Diferencia_Marcaje(whora_salAnt, whora_str);
                      wbandera = 1;
                      Ingresa_Marcajes(wclave_emp, wturno, whora_str, wtipo_reg, wcampo, wcampo2, wbandera, wmarcaje, wfechaRevAnt, MARCAJES, wtipo_fest);
                      wProcesado = true;
                      #endregion

                    }
                    else continue;
                    break;
                    #endregion TURNO12345_DESCANSO
                }
                #endregion

              }
              //continue;	//Instrucción de más por estar justo al final del ciclo
            }
          }
        }
        catch (Exception ex)
        {
#if DEBUG
					System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
					System.Diagnostics.StackFrame[] sf = st.GetFrames();
					string metodo = String.Format("Metodo: {0}", sf[sf.Length - 1].GetMethod());
					string archivo = String.Format("Archivo: {0}", sf[sf.Length - 1].GetFileName());
					string linea = String.Format("Linea: {0}", sf[sf.Length - 1].GetFileLineNumber());
					//MessageBox.Show(String.Format("{0}\r\n{1}\r\n{2}\r\n", metodo, archivo, linea));
					FuncionesComunes.GuardarLog(String.Format("{0}\r\n{1}\r\n{2}\r\n", metodo, archivo, linea));
#else
          FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(2333) | {0} | Error general ( {1} ).", wclave_emp, ex.Message, ex.StackTrace));
          System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
          FuncionesComunes.GuardaStackTraceLog(st);
#endif
          throw ex;
          //MessageBox.Show(ex.Message, "SISTEMA", MessageBoxButtons.OK);
          //MessageBox.Show(wclave_emp);
        }
        #endregion recorre_tabla_marcajes

        wfechaRevAnt = wfechaRevAnt.AddDays(1);
      }
      #endregion

      FuncionesComunes.GuardarLog("Termina Pase de Marcajes.");
    }

    private static string Diferencia_Marcaje(string hora1, string hora2)
    {
      string respuesta = "";
      int wdif = 0;
      int horaint1 = 0, horaint2 = 0;
      int minint1 = 0, minint2 = 0;
      horaint1 = Convert.ToInt32(hora1.Trim().Substring(0, 2));
      horaint2 = Convert.ToInt32(hora2.Trim().Substring(0, 2));
      if (horaint2 < horaint1) horaint2 = horaint2 + 24;
      wdif = horaint2 - horaint1;
      if (wdif > 1)
      {
        respuesta = "V";
      }
      else if (wdif <= 1)
      {
        minint1 = Convert.ToInt32(hora1.Trim().Substring(3, 2));
        minint2 = Convert.ToInt32(hora2.Trim().Substring(3, 2));
        if (minint2 < minint1) minint2 = minint2 + 60;
        wdif = minint2 - minint1;
        if (wdif > 10) respuesta = "V";
        else if (wdif <= 10) respuesta = "I";
      }
      return respuesta;
    }

    private void Ingresa_Marcajes(string wclave_emp, string wturno, string whora_str, string wtipo_reg, string wcampo, string wcampo2, Int32 wbandera, DateTime wmarcaje, DateTime wfecha_lista, DataRow MARCAJES, Boolean wtipo_fest)
    {

      #region INGRESA_MARCAJES
      cmdActualiza = new SqlCommand();
      if (wtipo_fest == false)
      {
        if ((wbandera == 1 || wbandera == 3) && wtipo_reg == "V")
        {
          cmdActualiza.CommandText = "UPDATE ASIS_DIA_PERM SET " + wcampo + " = @whora_str , " + wcampo2 + " = '?'  WHERE CLAVE_EMP=@wclave_Emp AND FECHA_MOV=@wfecha_mov AND TURNO=@wturno AND TIPO_MOV <> 'F'";
        }
        else if (wbandera != 1 && wbandera != 3 && wtipo_reg == "V")
        {
          cmdActualiza.CommandText = "UPDATE ASIS_DIA_PERM SET " + wcampo + " = @whora_str  WHERE CLAVE_EMP=@wclave_Emp AND FECHA_MOV=@wfecha_mov AND TURNO=@wturno AND TIPO_MOV <> 'F'";
        }
        if (wtipo_reg == "V")
        {
          cmdActualiza.Connection = ConexionFestivo;
          cmdActualiza.Parameters.Add("@whora_str", SqlDbType.Char).Value = whora_str;
          cmdActualiza.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
          if ((wbandera == 2 || wbandera == 3) && wtipo_reg == "V")
          {
            cmdActualiza.Parameters.Add("@wfecha_mov", SqlDbType.Date).Value = wfechaRevAnt.AddDays(-1);
          }
          else if ((wbandera == 1 || wbandera == 0) && wtipo_reg == "V")
          {
            cmdActualiza.Parameters.Add("@wfecha_mov", SqlDbType.Date).Value = wfechaRevAnt;
          }
          cmdActualiza.Parameters.Add("@wturno", SqlDbType.Char).Value = wturno;
          int i = 0;

          try
          {
            ConexionFestivo.Open();
            i = cmdActualiza.ExecuteNonQuery();
          }
          catch (SqlException ex)
          {
            FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(2406) | {0} | Error al actualizar dato ( {1} ).", wclave_emp, ex.Message));
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
            FuncionesComunes.GuardaStackTraceLog(st);
          }
          finally
          {
            //cmdInsert.Dispose(); //TODO: Activar
            ConexionFestivo.Close();
          }

          if (i == 1)
          {
            cmdActualiza = new SqlCommand("UPDATE MARCAJES SET MARCADO = 1 WHERE CLAVE_EMP=@wclave_Emp AND MARCAJE=@wmarcaje", ConexionFestivo);
            cmdActualiza.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
            cmdActualiza.Parameters.Add("@wmarcaje", SqlDbType.DateTime).Value = wmarcaje;
            daBusca.SelectCommand = cmdActualiza;

            try
            {
              ConexionFestivo.Open();
              cmdActualiza.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
              FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(2424) | {0} | Error al actualizar marcado ( {1} ).", wclave_emp, ex.Message));
              System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
              FuncionesComunes.GuardaStackTraceLog(st);
            }
            finally
            {
              //cmdInsert.Dispose(); //TODO: Activar
              ConexionFestivo.Close();
            }
            MARCAJES["marcado"] = 1;
          }
        }
        else if (wtipo_reg == "I")
        {
          cmdActualiza = new SqlCommand("UPDATE MARCAJES SET MARCADO = 1 WHERE CLAVE_EMP=@wclave_Emp AND MARCAJE=@wmarcaje", ConexionFestivo);
          cmdActualiza.Parameters.Add("@wclave_emp", SqlDbType.Char).Value = wclave_emp;
          cmdActualiza.Parameters.Add("@wmarcaje", SqlDbType.DateTime).Value = wmarcaje;
          daBusca.SelectCommand = cmdActualiza;
          try
          {
            ConexionFestivo.Open();
            cmdActualiza.ExecuteNonQuery();
          }
          catch (SqlException ex)
          {
            FuncionesComunes.GuardarLog(String.Format("PasaMarcajes(2442) | {0} | Error al actualizar marcado ( {1} ).", wclave_emp, ex.Message));
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex, true);
            FuncionesComunes.GuardaStackTraceLog(st);
          }
          finally
          {
            //cmdInsert.Dispose(); //TODO: Activar
            ConexionFestivo.Close();
          }
          MARCAJES["marcado"] = 1;
        }
      }
      #endregion INGRESA_MARCAJES

    }

    #endregion

    public PasaMarcajes(ref SqlConnection cn)
    {
      ConexionFestivo = cn;
      daBusca = new SqlDataAdapter();
      daHorario = new SqlDataAdapter();
      daFestivo = new SqlDataAdapter();
      dtBusca = new DataTable(); dtHorario = new DataTable(); dtMarcaje = new DataTable();
      dtAsistencia = new DataTable(); dtAsistencia_Ant = new DataTable(); dtActualiza = new DataTable();
      dtDiaHorario = new DataTable(); dtDia_Ant = new DataTable(); dt_Festivo = new DataTable();
    }

  }
}
