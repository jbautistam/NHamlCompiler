using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Bau.Libraries.NhamlCompiler;

namespace TestNhamlCompiler
{
	/// <summary>
	///		Formulario principal
	/// </summary>
	public partial class frmMain : Form
	{
		public frmMain()
		{ InitializeComponent();
		}

		/// <summary>
		///		Inicializa el formulario
		/// </summary>
		private void InitForm()
		{ if (!string.IsNullOrEmpty(Properties.Settings.Default.LastFileName))
				LoadFile(Properties.Settings.Default.LastFileName);
		}

		/// <summary>
		///		Interpreta un código fuente
		/// </summary>
		private void Parse(string strSource)
		{ Compiler objCompiler = new Compiler();
		
				// Vacía los cuadros de texto de resultado
					txtTarget.Text = "";
					txtEvents.Text = "";
				// Asigna el manejador de eventos
					objCompiler.Debug += (objSender, objEvntArgs) =>
																		{ txtEvents.Text += objEvntArgs.DebugMode.ToString() + " --> " + objEvntArgs.Title + Environment.NewLine;
																			txtEvents.Text += objEvntArgs.Message + Environment.NewLine;
																		};
				// Interpreta una cadena
					txtTarget.Text = objCompiler.Parse(strSource, null, 0, chkCompress.Checked);
				// Muestra los errores
					txtEvents.Text += Environment.NewLine + "--> Errores:";
					foreach (Bau.Libraries.NhamlCompiler.Errors.CompilerError objError in objCompiler.LocalErrors)
						{ txtEvents.Text += Environment.NewLine;
							txtEvents.Text += objError.Description + Environment.NewLine;
							txtEvents.Text += objError.Token + " Fila: " + objError.Row + " Columna: " + objError.Column;
						}
		}

		/// <summary>
		///		Carga el archivo
		/// </summary>
		private void LoadFile(string strFileName)
		{ // Carga el archivo (si existe)
				if (!System.IO.File.Exists(strFileName))
					{ Bau.Controls.Forms.Helper.ShowMessage(this, "No existe el archivo");
						strFileName = "";
					}
				else
					{ txtSource.Text = Bau.Libraries.LibHelper.Files.HelperFiles.LoadTextFile(strFileName).Replace("\n", "\r\n");
						fnFile.FileName = strFileName;
					}
			// Apunta el archivo en la configuración
				Properties.Settings.Default.LastFileName = strFileName;
				Properties.Settings.Default.Save();
		}

		private void cmdParse_Click(object sender, EventArgs e)
		{ Parse(txtSource.Text);
		}

		private void frmMain_Load(object sender, EventArgs e)
		{ InitForm();
		}

		private void cmdSave_Click(object sender, EventArgs e)
		{ string strFileName = Bau.Controls.Forms.Helper.GetFileNameSave("Archivos de texto|*.txt");

				if (!string.IsNullOrEmpty(strFileName))
					{ // Graba el archivo
							Bau.Libraries.LibHelper.Files.HelperFiles.SaveTextFile(strFileName, txtSource.Text);
						// Carga el archivo para abrirlo la próxima vez
							LoadFile(strFileName);
					}
		}

		private void fnFile_Changed(object objSender, EventArgs evnArgs)
		{ LoadFile(fnFile.FileName);
		}
	}
}
