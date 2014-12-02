using System;
using System.Diagnostics;
using System.IO;

namespace Bau.Libraries.LibHelper.Files
{
	/// <summary>
	///		Clase de ayuda para manejo de archivos
	/// </summary>
	public class HelperFiles
	{
		/// <summary>
		///		Elimina un archivo (no tiene en cuenta las excepciones)
		/// </summary>
		public static bool KillFile(string strFileName)
		{ try
				{ // Si existe el archivo, lo borra
						if (File.Exists(strFileName))
							{ // Quita el atributo de sólo lectura
									File.SetAttributes(strFileName, FileAttributes.Normal);
								// Elimina el archivo
									File.Delete(strFileName);
							}
					// Indica que se ha borrado correctamente
						return true;
				}
			catch (Exception objException)
				{ // Mensaje
						System.Diagnostics.Debug.WriteLine(objException.Message);
					// Indica que no se ha podido borrar
						return false;
				}
		}	
		
		/// <summary>
		///		Comprueba si existe una unidad
		/// </summary>
		public static bool ExistsDrive(string strPath)
		{ // Comprueba si existe la unidad
				if (!string.IsNullOrEmpty(strPath))
					{ DriveInfo [] arrDrvDrives = DriveInfo.GetDrives();
						  
							// Quita los espacios
								strPath = strPath.Trim();
							// Comprueba si existe la unidad
								foreach (DriveInfo drvDrive in arrDrvDrives)
									if (drvDrive.IsReady && strPath.StartsWith(drvDrive.Name, StringComparison.CurrentCultureIgnoreCase))
										return true;
					}
			// Si ha llegado hasta aquí es porque la unidad no existe
				return false;
		}

		/// <summary>
		///		Crea un directorio sin tener en cuenta las excepciones
		/// </summary>
		public static bool MakePath(string strPath)
		{ bool blnMade = false;

				// Crea el directorio
					try
						{ if (ExistsDrive(strPath))
								{ if (Directory.Exists(strPath)) // ... si ya existía, intentar crearlo devolvería un error
										blnMade = true;
									else // ... intenta crea el directorio
										{ Directory.CreateDirectory(strPath);
											blnMade = true;
										}
								}
						}
					catch {}
				// Devuelve el valor que indica si se ha creado
					return blnMade;
		}

		/// <summary>
		///		Elimina un directorio sin tener en cuenta las excepciones
		/// </summary>
		public static bool KillPath(string strPath)
		{ try
				{ if (Directory.Exists(strPath))
						Directory.Delete(strPath, true);
					return true;
				}
			catch { return false; }
		}
		
		/// <summary>
		/// 	Carga un archivo de texto en una cadena
		/// </summary>
		public static string LoadTextFile(string strFileName)
		{ string strData, strContent = "";

				// Carga el archivo
					using (StreamReader stmFile = new StreamReader(strFileName, System.Text.Encoding.GetEncoding("UTF-8")))
						{ // Lee los datos
								while ((strData = stmFile.ReadLine()) != null)
									{ // Le añade un salto de línea si es necesario
											if (strContent != "")
												strContent += "\n";
										// Añade la línea leída
											strContent += strData;
									}
							// Cierra el stream
								stmFile.Close();
						}
				// Devuelve el contenido
					return strContent;
		}

		/// <summary>
		/// 	Graba una cadena en un archivo de texto
		/// </summary>
		public static void SaveTextFile(string strFileName, string strText)
		{	SaveTextFile(strFileName, strText, System.Text.Encoding.UTF8);
		}

		/// <summary>
		/// 	Graba una cadena en un archivo de texto
		/// </summary>
		public static void SaveTextFile(string strFileName, string strText, string strEncoding)
		{	SaveTextFile(strFileName, strText, System.Text.Encoding.GetEncoding(strEncoding));
		}

		/// <summary>
		/// 	Graba una cadena en un archivo de texto
		/// </summary>
		public static void SaveTextFile(string strFileName, string strText, System.Text.Encoding objEncoding)
		{	// Abre un stream de escritura
				using (StreamWriter stmFile = new StreamWriter(strFileName, false, objEncoding))
					{ // Escribe la cadena
							stmFile.Write(strText);
						// Cierra el stream
							stmFile.Close();
					}
		}		

		/// <summary>
		///		Copia un archivo
		/// </summary>
		public static bool CopyFile(string strFileNameSource, string strFileNameTarget)
		{ try
				{ // Elimina el archivo antiguo
						KillFile(strFileNameTarget);
					// Copia el archivo origen en el destino
						File.Copy(strFileNameSource, strFileNameTarget);
					// Indica que se ha copiado correctamente
						return true;
				}
			catch
				{ return false;
				}
		}

		/// <summary>
		///		Copia un directorio en otro
		/// </summary>
		public static void CopyPath(string strSourcePath, string strTargetPath)
		{ string [] arrStrFiles = Directory.GetFiles(strSourcePath);
			string [] arrStrPath = Directory.GetDirectories(strSourcePath);
		
				// Crea el directorio destino
					MakePath(strTargetPath);
				// Copia los archivos del directorio origen en el destino
					foreach (string strFileName in arrStrFiles)
						CopyFile(strFileName, Path.Combine(strTargetPath, Path.GetFileName(strFileName)));
				// Copia los directorios
					foreach (string strPath in arrStrPath)
						CopyPath(strPath, Path.Combine(strTargetPath, Path.GetFileName(strPath)));
		}

		/// <summary>
		///		Crea un directorio consecutivo, es decir, si existe ya el nombre del directorio crea
		///	uno con el mismo nombre y un índice: "Nueva carpeta", "Nueva carpeta 1", "Nueva carpeta 2" ...
		/// </summary>
		public static bool MakeConsecutivePath(string strPathBase, string strPath)
		{ return MakePath(Path.Combine(strPathBase, GetConsecutivePath(strPathBase, strPath)));						
		}

		/// <summary>
		///		Cambia el nombre de un archivo o directorio
		/// </summary>
		public static bool Rename(string strOldName, string strNewName)
		{ bool blnChanged = false;
		
				// Cambia el nombre
					if (!strOldName.Equals(strNewName, StringComparison.CurrentCulture))
						{ try
								{ // Cambia el nombre
										if (Directory.Exists(strOldName))
											Directory.Move(strOldName, strNewName);
										else if (File.Exists(strOldName))
											File.Move(strOldName, strNewName);
									// Indica que se ha modificado el nombre
										blnChanged = true;
								}
							catch {}
						}
				// Devuelve el valor que indica si se ha cambiado
					return blnChanged;
		}

		/// <summary>
		///		Obtiene un nombre consecutivo de archivo del tipo NombreArchivo, Copia de nombreArchivo, Copia de nombreArchivo 2
		/// </summary>
		public static string GetConsecutiveFileName(string strPath, string strFileName)
		{ string strNewFile = Path.GetFileName(strFileName);
			int intIndex = 0;
		
				// Si existe el archivo destino, lo cambia
					while (File.Exists(Path.Combine(strPath, strNewFile)))
						{ // Obtiene el nombre nuevo
								strNewFile = Path.GetFileName(strFileName);
								if (intIndex > 0)
									strNewFile = Path.GetFileNameWithoutExtension(strNewFile) + " " + intIndex.ToString() + Path.GetExtension(strNewFile);
							// Incrementa el índice
								intIndex++;
						}
				// Devuelve el nombre destino
					return Path.Combine(strPath, strNewFile);
		}

		/// <summary>
		///		Obtiene un nombre consecutivo de archivo del tipo 00000001.Ext
		/// </summary>
		public static string GetConsecutiveFileNameByExtension(string strPath, string strExtension)
		{ string strNewFile = "00000001." + strExtension;
			int intIndex = 2;
		
				// Si existe el archivo destino, lo cambia
					while (File.Exists(Path.Combine(strPath, strNewFile)))
						{ // Obtiene el nombre nuevo
								strNewFile = string.Format("{0:0000000}.", intIndex) + strExtension;
							// Incrementa el índice
								intIndex++;
						}
				// Devuelve el nombre destino
					return Path.Combine(strPath, strNewFile);
		}
		
		/// <summary>
		///		Obtiene un nombre consecutivo de directorio del tipo Directorio, Directorio 1, Directorio 2 ...
		/// </summary>
		public static string GetConsecutivePath(string strPathBase, string strPath)
		{	string strNewPath = strPath;
			int intIndex = 1;
					
				// Crea el nombre del directorio
					while (Directory.Exists(Path.Combine(strPathBase, strNewPath)))
						{ // Crea el nuevo nombre
								strNewPath = strPath + " " + intIndex.ToString();
							// Incrementa el índice
								intIndex++;
						}
				// Crea el nombre de directorio
					strNewPath = Path.Combine(strPathBase, strNewPath);
				// Devuelve el nombre del archivo
					return strNewPath;
		}

		/// <summary>
		///		Obtiene el último directorio de un directorio
		/// </summary>
		public static string GetLastPath(string strPath)
		{ string strLastPath = "";
		
				// Busca la última parte del directorio
					if (!string.IsNullOrEmpty(strPath))
						{ string [] arrStrPath = strPath.Split(Path.DirectorySeparatorChar);
						
								if (arrStrPath.Length > 0)
									strLastPath = arrStrPath[arrStrPath.Length - 1];
						}
				// Devuelve la última parte del directorio
					return strLastPath;
		}

		/// <summary>
		///		Normaliza un nombre de archivo
		/// </summary>
		public static string Normalize(string strFileName, bool blnWithAccents = true)
		{ return Normalize(strFileName, 0, blnWithAccents);
		}
		
		/// <summary>
		///		Normaliza un nombre de archivo
		/// </summary>
		public static string Normalize(string strFileName, int intLength, bool blnWithAccents = true)
		{ const string cnstStrValid = "01234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ ._-";
			const string cnstStrWithAccent = "áéíóúàèìòùÁÉÍÓÚÀÈÌÒÙñÑçÇ";
			const string cnstStrWithOutAccent = "aeiouaeiouAEIOUAEIOUnNcC";
			string strTarget = "";

				// Normaliza el nombre de archivo
					foreach (char chrChar in strFileName)
						if (cnstStrWithAccent.IndexOf(chrChar) >= 0)
							{ if (!blnWithAccents)
									strTarget += cnstStrWithOutAccent[cnstStrWithAccent.IndexOf(chrChar)];
								else
									strTarget += chrChar;
							}
						else if (cnstStrValid.IndexOf(chrChar) >= 0)
							strTarget += chrChar;
				// Limpia los espacios
					strTarget = strTarget.Trim();
				// Quita los puntos iniciales
					while (strTarget.Length > 0 && strTarget[0] == '.')
						strTarget = strTarget.Substring(1);
				// Obtiene los n primeros caracteres del nombre de archivo
					if (intLength > 0 && strTarget.Length >	intLength)
						strTarget = strTarget.Substring(0, intLength);
				// Devuelve la cadena de salida
					return strTarget;
		}

		/// <summary>
		///		Abre un documento utilizando el shell
		/// </summary>
		public static void OpenDocumentShell(string strExecutable, string strFileName)
		{ Process objProcess = new Process();

				// Inicializa las propiedades del proceso
					objProcess.StartInfo.UseShellExecute = true;
					objProcess.StartInfo.RedirectStandardOutput = false;
					if (string.IsNullOrEmpty(strExecutable))
						{ objProcess.StartInfo.FileName = strFileName;
							objProcess.StartInfo.Arguments = "";
						}
					else
						{ objProcess.StartInfo.FileName = strExecutable;
							objProcess.StartInfo.Arguments = strFileName;
						}
					objProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
					objProcess.StartInfo.CreateNoWindow = true;
				// Ejecuta el proceso
					objProcess.Start();
		}
		
		/// <summary>
		///		Abre el documento utilizando el shell
		/// </summary>
		public static void OpenDocumentShell(string strFileName)
		{ OpenDocumentShell(null, strFileName);
		}

		/// <summary>
		///		Abre el explorador con un archivo
		/// </summary>
		public static void OpenBrowser(string strURL)
		{ OpenDocumentShell("iexplore.exe", strURL);
		}

		/// <summary>
		///		Carga recursivamente una colección de archivos de un directorio
		/// </summary>
		public static System.Collections.Generic.List<string> LoadRecursive(string strPathSource)
		{ System.Collections.Generic.List<string> objColFiles = new System.Collections.Generic.List<string>();
			string [] arrStrFiles;

				// Obtiene los archivos
					arrStrFiles = Directory.GetFiles(strPathSource);
					foreach (string strFile in arrStrFiles)
						objColFiles.Add(strFile);
				// Obtiene los directorios
					arrStrFiles = Directory.GetDirectories(strPathSource);
					foreach (string strPath in arrStrFiles)
						objColFiles.AddRange(LoadRecursive(Path.Combine(strPathSource, Path.GetFileName(strPath))));
				// Devuelve la colección de archivos
					return objColFiles;
		}

		/// <summary>
		///		Comprueba si es un archivo de imagen
		/// </summary>
		public static bool IsImage(string strFileName)
		{ string strExtension = Path.GetExtension(strFileName);

				// Comprueba la extensión
					return strExtension.StartsWith(".png", StringComparison.CurrentCultureIgnoreCase) ||
								 strExtension.StartsWith(".gif", StringComparison.CurrentCultureIgnoreCase) ||
								 strExtension.StartsWith(".jpg", StringComparison.CurrentCultureIgnoreCase) ||
								 strExtension.StartsWith(".bmp", StringComparison.CurrentCultureIgnoreCase) ||
								 strExtension.StartsWith(".tiff", StringComparison.CurrentCultureIgnoreCase) ||
								 strExtension.StartsWith(".tif", StringComparison.CurrentCultureIgnoreCase);
		}
	}
}