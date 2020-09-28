namespace Marcajes
{
	partial class frmMarcajes
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnAcomodoMarcajes = new System.Windows.Forms.Button();
			this.btnPaseLista = new System.Windows.Forms.Button();
			this.txtFecha = new System.Windows.Forms.TextBox();
			this.lblFecha = new System.Windows.Forms.Label();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.btnBorrarLog = new System.Windows.Forms.Button();
			this.tbcMarcajes = new System.Windows.Forms.TabControl();
			this.tpAsistencia = new System.Windows.Forms.TabPage();
			this.lblLog = new System.Windows.Forms.Label();
			this.tpConfig = new System.Windows.Forms.TabPage();
			this.btnSalir = new System.Windows.Forms.Button();
			this.txtCadenaConexion = new System.Windows.Forms.TextBox();
			this.lblCadenaConexion = new System.Windows.Forms.Label();
			this.btnProbarConexion = new System.Windows.Forms.Button();
			this.btnBuscarPathTrabajo = new System.Windows.Forms.Button();
			this.lblPathTrabajo = new System.Windows.Forms.Label();
			this.txtPathTrabajo = new System.Windows.Forms.TextBox();
			this.btnCargarConfig = new System.Windows.Forms.Button();
			this.btnGuardarConfig = new System.Windows.Forms.Button();
			this.tbcMarcajes.SuspendLayout();
			this.tpAsistencia.SuspendLayout();
			this.tpConfig.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnAcomodoMarcajes
			// 
			this.btnAcomodoMarcajes.Location = new System.Drawing.Point(205, 26);
			this.btnAcomodoMarcajes.Name = "btnAcomodoMarcajes";
			this.btnAcomodoMarcajes.Size = new System.Drawing.Size(114, 23);
			this.btnAcomodoMarcajes.TabIndex = 6;
			this.btnAcomodoMarcajes.Text = "Acomodar Marcajes";
			this.btnAcomodoMarcajes.UseVisualStyleBackColor = true;
			this.btnAcomodoMarcajes.Click += new System.EventHandler(this.btnAcomodoMarcajes_Click);
			// 
			// btnPaseLista
			// 
			this.btnPaseLista.Location = new System.Drawing.Point(124, 26);
			this.btnPaseLista.Name = "btnPaseLista";
			this.btnPaseLista.Size = new System.Drawing.Size(75, 23);
			this.btnPaseLista.TabIndex = 5;
			this.btnPaseLista.Text = "Pasar Lista";
			this.btnPaseLista.UseVisualStyleBackColor = true;
			this.btnPaseLista.Click += new System.EventHandler(this.btnPaseLista_Click);
			// 
			// txtFecha
			// 
			this.txtFecha.Location = new System.Drawing.Point(18, 28);
			this.txtFecha.Name = "txtFecha";
			this.txtFecha.Size = new System.Drawing.Size(100, 20);
			this.txtFecha.TabIndex = 0;
			// 
			// lblFecha
			// 
			this.lblFecha.AutoSize = true;
			this.lblFecha.Location = new System.Drawing.Point(18, 12);
			this.lblFecha.Name = "lblFecha";
			this.lblFecha.Size = new System.Drawing.Size(37, 13);
			this.lblFecha.TabIndex = 9;
			this.lblFecha.Text = "Fecha";
			// 
			// txtLog
			// 
			this.txtLog.Location = new System.Drawing.Point(18, 92);
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtLog.Size = new System.Drawing.Size(539, 311);
			this.txtLog.TabIndex = 14;
			// 
			// btnBorrarLog
			// 
			this.btnBorrarLog.Location = new System.Drawing.Point(325, 26);
			this.btnBorrarLog.Name = "btnBorrarLog";
			this.btnBorrarLog.Size = new System.Drawing.Size(75, 23);
			this.btnBorrarLog.TabIndex = 15;
			this.btnBorrarLog.Text = "Borrar Log";
			this.btnBorrarLog.UseVisualStyleBackColor = true;
			this.btnBorrarLog.Click += new System.EventHandler(this.btnBorrarLog_Click);
			// 
			// tbcMarcajes
			// 
			this.tbcMarcajes.Controls.Add(this.tpAsistencia);
			this.tbcMarcajes.Controls.Add(this.tpConfig);
			this.tbcMarcajes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tbcMarcajes.Location = new System.Drawing.Point(0, 0);
			this.tbcMarcajes.Name = "tbcMarcajes";
			this.tbcMarcajes.SelectedIndex = 0;
			this.tbcMarcajes.Size = new System.Drawing.Size(585, 461);
			this.tbcMarcajes.TabIndex = 16;
			this.tbcMarcajes.SelectedIndexChanged += new System.EventHandler(this.tbcMarcajes_SelectedIndexChanged);
			// 
			// tpAsistencia
			// 
			this.tpAsistencia.Controls.Add(this.btnSalir);
			this.tpAsistencia.Controls.Add(this.lblFecha);
			this.tpAsistencia.Controls.Add(this.lblLog);
			this.tpAsistencia.Controls.Add(this.btnAcomodoMarcajes);
			this.tpAsistencia.Controls.Add(this.btnBorrarLog);
			this.tpAsistencia.Controls.Add(this.btnPaseLista);
			this.tpAsistencia.Controls.Add(this.txtFecha);
			this.tpAsistencia.Controls.Add(this.txtLog);
			this.tpAsistencia.Location = new System.Drawing.Point(4, 22);
			this.tpAsistencia.Name = "tpAsistencia";
			this.tpAsistencia.Padding = new System.Windows.Forms.Padding(3);
			this.tpAsistencia.Size = new System.Drawing.Size(577, 435);
			this.tpAsistencia.TabIndex = 0;
			this.tpAsistencia.Text = "Asistencia";
			this.tpAsistencia.UseVisualStyleBackColor = true;
			// 
			// lblLog
			// 
			this.lblLog.AutoSize = true;
			this.lblLog.Location = new System.Drawing.Point(15, 71);
			this.lblLog.Name = "lblLog";
			this.lblLog.Size = new System.Drawing.Size(25, 13);
			this.lblLog.TabIndex = 16;
			this.lblLog.Text = "Log";
			// 
			// tpConfig
			// 
			this.tpConfig.Controls.Add(this.btnGuardarConfig);
			this.tpConfig.Controls.Add(this.btnCargarConfig);
			this.tpConfig.Controls.Add(this.btnBuscarPathTrabajo);
			this.tpConfig.Controls.Add(this.lblPathTrabajo);
			this.tpConfig.Controls.Add(this.txtPathTrabajo);
			this.tpConfig.Controls.Add(this.btnProbarConexion);
			this.tpConfig.Controls.Add(this.lblCadenaConexion);
			this.tpConfig.Controls.Add(this.txtCadenaConexion);
			this.tpConfig.Location = new System.Drawing.Point(4, 22);
			this.tpConfig.Name = "tpConfig";
			this.tpConfig.Padding = new System.Windows.Forms.Padding(3);
			this.tpConfig.Size = new System.Drawing.Size(577, 435);
			this.tpConfig.TabIndex = 1;
			this.tpConfig.Text = "Config";
			this.tpConfig.UseVisualStyleBackColor = true;
			// 
			// btnSalir
			// 
			this.btnSalir.Location = new System.Drawing.Point(482, 26);
			this.btnSalir.Name = "btnSalir";
			this.btnSalir.Size = new System.Drawing.Size(75, 23);
			this.btnSalir.TabIndex = 17;
			this.btnSalir.Text = "Salir";
			this.btnSalir.UseVisualStyleBackColor = true;
			this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
			// 
			// txtCadenaConexion
			// 
			this.txtCadenaConexion.Location = new System.Drawing.Point(120, 35);
			this.txtCadenaConexion.Name = "txtCadenaConexion";
			this.txtCadenaConexion.Size = new System.Drawing.Size(347, 20);
			this.txtCadenaConexion.TabIndex = 0;
			// 
			// lblCadenaConexion
			// 
			this.lblCadenaConexion.AutoSize = true;
			this.lblCadenaConexion.Location = new System.Drawing.Point(8, 38);
			this.lblCadenaConexion.Name = "lblCadenaConexion";
			this.lblCadenaConexion.Size = new System.Drawing.Size(106, 13);
			this.lblCadenaConexion.TabIndex = 10;
			this.lblCadenaConexion.Text = "Cadena de Conexion";
			// 
			// btnProbarConexion
			// 
			this.btnProbarConexion.Location = new System.Drawing.Point(482, 33);
			this.btnProbarConexion.Name = "btnProbarConexion";
			this.btnProbarConexion.Size = new System.Drawing.Size(75, 23);
			this.btnProbarConexion.TabIndex = 18;
			this.btnProbarConexion.Text = "Probar";
			this.btnProbarConexion.UseVisualStyleBackColor = true;
			this.btnProbarConexion.Click += new System.EventHandler(this.btnProbarConexion_Click);
			// 
			// btnBuscarPathTrabajo
			// 
			this.btnBuscarPathTrabajo.Location = new System.Drawing.Point(482, 59);
			this.btnBuscarPathTrabajo.Name = "btnBuscarPathTrabajo";
			this.btnBuscarPathTrabajo.Size = new System.Drawing.Size(75, 23);
			this.btnBuscarPathTrabajo.TabIndex = 21;
			this.btnBuscarPathTrabajo.Text = "Buscar";
			this.btnBuscarPathTrabajo.UseVisualStyleBackColor = true;
			this.btnBuscarPathTrabajo.Click += new System.EventHandler(this.btnBuscarPathTrabajo_Click);
			// 
			// lblPathTrabajo
			// 
			this.lblPathTrabajo.AutoSize = true;
			this.lblPathTrabajo.Location = new System.Drawing.Point(8, 64);
			this.lblPathTrabajo.Name = "lblPathTrabajo";
			this.lblPathTrabajo.Size = new System.Drawing.Size(106, 13);
			this.lblPathTrabajo.TabIndex = 20;
			this.lblPathTrabajo.Text = "Directorio de Trabajo";
			// 
			// txtPathTrabajo
			// 
			this.txtPathTrabajo.Location = new System.Drawing.Point(120, 61);
			this.txtPathTrabajo.Name = "txtPathTrabajo";
			this.txtPathTrabajo.Size = new System.Drawing.Size(347, 20);
			this.txtPathTrabajo.TabIndex = 19;
			// 
			// btnCargarConfig
			// 
			this.btnCargarConfig.Location = new System.Drawing.Point(6, 6);
			this.btnCargarConfig.Name = "btnCargarConfig";
			this.btnCargarConfig.Size = new System.Drawing.Size(75, 23);
			this.btnCargarConfig.TabIndex = 22;
			this.btnCargarConfig.Text = "Cargar";
			this.btnCargarConfig.UseVisualStyleBackColor = true;
			this.btnCargarConfig.Click += new System.EventHandler(this.btnCargarConfig_Click);
			// 
			// btnGuardarConfig
			// 
			this.btnGuardarConfig.Location = new System.Drawing.Point(87, 6);
			this.btnGuardarConfig.Name = "btnGuardarConfig";
			this.btnGuardarConfig.Size = new System.Drawing.Size(75, 23);
			this.btnGuardarConfig.TabIndex = 23;
			this.btnGuardarConfig.Text = "Guardar";
			this.btnGuardarConfig.UseVisualStyleBackColor = true;
			this.btnGuardarConfig.Click += new System.EventHandler(this.btnGuardarConfig_Click);
			// 
			// frmMarcajes
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(585, 461);
			this.Controls.Add(this.tbcMarcajes);
			this.Name = "frmMarcajes";
			this.Text = "Manejo de marcajes";
			this.Load += new System.EventHandler(this.frmMarcajes_Load);
			this.tbcMarcajes.ResumeLayout(false);
			this.tpAsistencia.ResumeLayout(false);
			this.tpAsistencia.PerformLayout();
			this.tpConfig.ResumeLayout(false);
			this.tpConfig.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Button btnAcomodoMarcajes;
		private System.Windows.Forms.Button btnPaseLista;
		private System.Windows.Forms.TextBox txtFecha;
		private System.Windows.Forms.Label lblFecha;
		private System.Windows.Forms.TextBox txtLog;
		private System.Windows.Forms.Button btnBorrarLog;
		private System.Windows.Forms.TabControl tbcMarcajes;
		private System.Windows.Forms.TabPage tpAsistencia;
		private System.Windows.Forms.Label lblLog;
		private System.Windows.Forms.TabPage tpConfig;
		private System.Windows.Forms.Button btnSalir;
		private System.Windows.Forms.Button btnProbarConexion;
		private System.Windows.Forms.Label lblCadenaConexion;
		private System.Windows.Forms.TextBox txtCadenaConexion;
		private System.Windows.Forms.Button btnBuscarPathTrabajo;
		private System.Windows.Forms.Label lblPathTrabajo;
		private System.Windows.Forms.TextBox txtPathTrabajo;
		private System.Windows.Forms.Button btnGuardarConfig;
		private System.Windows.Forms.Button btnCargarConfig;
	}
}

