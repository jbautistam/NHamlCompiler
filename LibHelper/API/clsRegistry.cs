using System;
using Microsoft.Win32;

namespace Bau.Libraries.LibHelper.API
{
	/// <summary>
	///		Clase con funciones útiles de acceso al registro
	/// </summary>
	public class clsRegistry
	{ // Constantes privadas 
			private const string cnstStrWindowsRun = @"Software\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    ///		Asocia una extensión con un ejecutable y una acción
    /// </summary>
    public static void LinkExtension(string strExtension, string strExecutableFileName, string strProgramId)
    {	LinkExtension(strExtension, strExecutableFileName, strProgramId, "open", "");
    }

    /// <summary>
    ///		Asocia una extensión con un ejecutable y una acción
    /// </summary>
    public static void LinkExtension(string strExtension, string strExecutableFileName, string strProgramId,
																		 string strCommand)
    {	LinkExtension(strExtension, strExecutableFileName, strProgramId, strCommand, "");
    }

    /// <summary>
    ///		Asocia una extensión con un ejecutable y una acción
    /// </summary>
    public static void LinkExtension(string strExtension, string strExecutableFileName, string strProgramId, 
																		 string strCommand, string strDescription)
    { string strLinkedProgramID;
      RegistryKey objRegistryKey = null;
      RegistryKey objRegistryKeyShell = null;

				// El comando predeterminado es open
					if (string.IsNullOrEmpty(strCommand))
						strCommand = "open";
				// Obtiene la descripción
					if (string.IsNullOrEmpty(strDescription))
						strDescription = strExtension + " Descripción de " + strProgramId;
				// Normaliza la extensión
					strExtension = NormalizeExtension(strExtension);
				// Obtiene el ID del programa a partir de la extensión
				  strLinkedProgramID = GetProgIdFromExtension(strExtension);
				// Si no hay nada asociado, se crean las claves, si hay algo asociado se modifican
				  if (string.IsNullOrEmpty(strLinkedProgramID) || strLinkedProgramID.Length == 0)
				    {	// Crear la clave con la extensión
				        objRegistryKey = Registry.ClassesRoot.CreateSubKey(strExtension);
				        objRegistryKey.SetValue("", strProgramId);
				      // Crea la calve con el programa
				        objRegistryKey = Registry.ClassesRoot.CreateSubKey(strProgramId);
				        objRegistryKey.SetValue("", strDescription);
				      // Crea la clave con el comando
				        objRegistryKeyShell = objRegistryKey.CreateSubKey("shell\\" + strCommand + "\\command");
				    }
				  else
				    {	// Abrimos la clave clave indicando que vamos a escribir para que nos permita crear nuevas subclaves.
				        objRegistryKey = Registry.ClassesRoot.OpenSubKey(strLinkedProgramID, true);
				        objRegistryKeyShell = objRegistryKey.OpenSubKey("shell\\" + strCommand + "\\command", true);
				      // Si es un comando que se añade, no existirá
				        if (objRegistryKeyShell == null)
				          objRegistryKeyShell = objRegistryKey.CreateSubKey(strProgramId);
				    }
				  // Si tenemos la clave de registro del Shell
				    if (objRegistryKeyShell != null)
				      {	objRegistryKeyShell.SetValue("", "\"" + strExecutableFileName + "\" \"%1\"");
				        objRegistryKeyShell.Close();
				      }
    }

    /// <summary>
    ///		Permite quitar un comando asociado con una extensión
    /// </summary>
    public static void DeleteLinkCommandExtension(string strExtension, string strCommand)
    { if (!string.IsNullOrEmpty(strCommand) && !strCommand.Equals("open", StringComparison.CurrentCultureIgnoreCase))
				{ string strProgramID;
				
						// Normaliza la extensión
							strExtension = NormalizeExtension(strExtension);
						// Obtiene el ID de programa de la extensión
							strProgramID = GetProgIdFromExtension(strExtension);
						// Elimina la clave del registro
            if (!string.IsNullOrEmpty(strProgramID) && strProgramID.Length > 0)
							using(RegistryKey objRegistryKey = Registry.ClassesRoot.OpenSubKey(strProgramID, true))
                {	if (objRegistryKey != null)
                    objRegistryKey.DeleteSubKeyTree("shell\\" + strCommand);
                }
        }
    }

    /// <summary>
    /// Quita la extensión indicada de los tipos de archivos registrados
    /// </summary>
    public static void DeleteLinkProgramExtension(string strExtension)
    { string strProgramID;
    
				// Obtiene la extensión
					strExtension = NormalizeExtension(strExtension);
				// Obtiene el ID del programa
					strProgramID = GetProgIdFromExtension(strExtension);
				// Elimina las claves
					if (!string.IsNullOrEmpty(strProgramID) && strProgramID.Length > 0)
            {	// Elimina la subclave
                Registry.ClassesRoot.DeleteSubKeyTree(strExtension);
							// Elimina la clave
                Registry.ClassesRoot.DeleteSubKeyTree(strProgramID);
            }
    }

    /// <summary>
    ///		Comprueba si la extensión indicada está registrada.
    /// </summary>
    public static bool ExistsLink(string strExtension)
    { string strProgramID = GetProgIdFromExtension(NormalizeExtension(strExtension));

        return !string.IsNullOrEmpty(strProgramID) && strProgramID.Length > 0;
    }

		/// <summary>
		///		Añade un punto al inicio de una extensión
		/// </summary>
		private static string NormalizeExtension(string strExtension)
		{ if (!strExtension.StartsWith("."))
				return "." + strExtension;
			else
				return strExtension;
		}
		
    /// <summary>
    ///		Método para obtener el ID de programa de una extensión
    /// </summary>
    public static string GetProgIdFromExtension(string strExtension)
    { string strProgramID = "";

				// Obtiene el ID del prgorama
          using (RegistryKey objRegistryKey = Registry.ClassesRoot.OpenSubKey(strExtension))
						{	if (objRegistryKey != null && objRegistryKey.GetValue("") != null)
								{	// Obtiene el ID
										strProgramID = objRegistryKey.GetValue("").ToString();
									// Cierra la clave
										objRegistryKey.Close();
								}
						}
				// Devuelve el ID del programa
					return strProgramID;
    }

    /// <summary>
    /// Añade al registro de Windows la aplicación usando el título indicado
    /// </summary>
    public static void AddToWindowsStart(string strTitle, string strExeFileName)
    {	using(RegistryKey objRegistryKey = Registry.LocalMachine.OpenSubKey(cnstStrWindowsRun, true))
				{	if(objRegistryKey != null)
						objRegistryKey.SetValue(strTitle, "\"" + strExeFileName + "\"");
        }
    }

    /// <summary>
    ///		Quita del registro de Windows la aplicación relacionada con el título indicado
    /// </summary>
    public static void RemoveFromWindowsStart(string strTitle)
    {	using(RegistryKey objRegistryKey = Registry.LocalMachine.OpenSubKey(cnstStrWindowsRun, true))
				{	if(objRegistryKey != null)
						objRegistryKey.DeleteValue(strTitle, false);
        }
    }

    /// <summary>
    ///		Comprueba si una aplicación con el título indicado está en el inicio de Windows
    /// </summary>
    public static bool IsAtWindowsStart(string strTitle, out string strFileName)
    {	// Inicializa el valor de salida
				strFileName = null;
			// Obtiene el valor del registro
          using(RegistryKey objRegistryKey = Registry.LocalMachine.OpenSubKey(cnstStrWindowsRun, true))
						{	if(objRegistryKey != null)
								strFileName = objRegistryKey.GetValue(strTitle).ToString();
						}
			// Devuelve el valor que indica si está en el inicio de Windows
				return !string.IsNullOrEmpty(strFileName);
    }
	}
}
