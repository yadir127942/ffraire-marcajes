using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.IO;
using Marcajes.Properties;
using Marcajes.Clases;
using static Marcajes.Clases.EnumsComunes;

namespace Marcajes
{
	//LIGA PARA PROYECTO SQL https://www.c-sharpcorner.com/article/create-sql-server-database-project-with-visual-studio/
	public partial class frmMarcajes : Form
	{

		#region Variables
		SqlConnection conexion;
		#endregion

		#region Metodos Personales
		
		private void CargarConfiguracion()
		{
			string cadenaConexion= Settings.Default.instanciaSQL;
			string directorioTrabajo = Settings.Default.pathTrabajo;
			conexion.ConnectionString = cadenaConexion;
			txtCadenaConexion.Text = cadenaConexion;
			txtPathTrabajo.Text = directorioTrabajo;
		}

		private void GuardarConfiguracion()
		{
			Settings.Default.instanciaSQL = txtCadenaConexion.Text.Trim();
			Settings.Default.pathTrabajo = txtPathTrabajo.Text.Trim();
		}

		private void GuardarLog(string mensaje, TipoLog tipo = TipoLog.Proceso)
		{
			switch (tipo)
			{
				case TipoLog.Proceso:
					txtLog.Text = $"{mensaje}\r\n{txtLog.Text}";
					break;
				case TipoLog.Error:
					break;
			}
		}
		#endregion

		public frmMarcajes()
		{
			InitializeComponent();
		}

		private void frmMarcajes_Load(object sender, EventArgs e)
		{
			conexion = new SqlConnection();
			CargarConfiguracion();
		}

		private void btnPaseLista_Click(object sender, EventArgs e)
		{
			PasaListaNuevo lista = new PasaListaNuevo(txtFecha.Text.Trim(), ref conexion);
			Cursor.Current = Cursors.WaitCursor;
			System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
			timer.Start();
			lista.PasarLista();
			lista.FestivosTrabajados();
			timer.Stop();
			Cursor.Current = Cursors.Default;
			GuardarLog($"Terminado pase de lista ({timer.Elapsed.TotalSeconds} segundos)");
		}

		private void btnAcomodoMarcajes_Click(object sender, EventArgs e)
		{
			PasaMarcajes marcajes = new PasaMarcajes(ref conexion);
			Cursor.Current = Cursors.WaitCursor;
			System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
			timer.Start();
			marcajes.IniciarProceso(txtFecha.Text.Trim());
			timer.Stop();
			Cursor.Current = Cursors.Default;
			GuardarLog($"Terminado acomodo de marcajes ({timer.Elapsed.TotalSeconds} segundos)");
		}

		private void btnBorrarLog_Click(object sender, EventArgs e)
		{
			txtLog.Clear();
		}

		private void btnSalir_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void tbcMarcajes_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (tbcMarcajes.SelectedTab.Name.Equals(tpConfig.Name))
			{

			}
		}

		private void btnProbarConexion_Click(object sender, EventArgs e)
		{
			string cadenaConexion = txtCadenaConexion.Text.Trim();
			if (String.IsNullOrWhiteSpace(cadenaConexion))
			{
				MessageBox.Show("La cadena de conexión no puede estar vacía");
				return;
			}
			conexion.ConnectionString = cadenaConexion;
			try
			{
				conexion.Open();
				MessageBox.Show(conexion.State.ToString());
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				conexion.Close();
			}
		}

		private void btnBuscarPathTrabajo_Click(object sender, EventArgs e)
		{
			string pathTrabajo = txtPathTrabajo.Text.Trim();
			if (String.IsNullOrWhiteSpace(pathTrabajo))
			{
				pathTrabajo = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
			}
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			fbd.Description = "Carpeta de trabajo para archivos auxiliares...";
			fbd.SelectedPath = Path.GetFullPath(pathTrabajo);
			if (fbd.ShowDialog() == DialogResult.OK)
			{
				pathTrabajo = fbd.SelectedPath;
			}
			txtPathTrabajo.Text = pathTrabajo;
		}

		private void btnCargarConfig_Click(object sender, EventArgs e)
		{
			CargarConfiguracion();
		}

		private void btnGuardarConfig_Click(object sender, EventArgs e)
		{
			GuardarConfiguracion();
		}
	}
}
