using System;
using Microsoft.Win32;

namespace Bau.Libraries.LibHelper.API
{
	/// <summary>
	///		Clase con funciones �tiles de acceso al registro
	/// </summary>
	public class clsRegistry
	{ // Constantes privadas 
			private const string cnstStrWindowsRun = @"Software\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    ///		Asocia una extensi�n con un ejecutable y una acci�n
    /// </summary>
    public static void LinkExtension(string strExtension, string strExecutableFileName, string strProgramId)
    {	LinkExtension(strExtension, strExecutableFileName, strProgramId, "open", "");
    }

    /// <summary>
    ///		Asocia una extensi�n con un ejecutable y una acci�n
    /// </summary>
    public static void LinkExtension(string strExtension, string strExecutableFileName, string strProgramId,
																		 string strCommand)
    {	LinkExtension(strExtension, strExecutableFileName, strProgramId, strCommand, "");
    }

    /// <summary>
    ///		Asocia una extensi�n con un ejecutable y una acci�n
    /// </summary>
    public static void LinkExtension(string strExtension, string strExecutableFileName, string strProgramId, 
																		 string strCommand, string strDescription)
    { string strLinkedProgramID;
      RegistryKey objRegistryKey = null;
      RegistryKey objRegistryKeyShell = null;

				// El comando predeterminado es open
					if (string.IsNullOrEmpty(strCommand))
						strCommand = "open";
				// Obtiene la descripci�n
					if (string.IsNullOrEmpty(strDescription))
						strDescription = strExtension + " Descripci�n de " + strProgramId;
				// Normaliza la extensi�n
					strExtension = NormalizeExtension(strExtension);
				// Obtiene el ID del programa a partir de la extensi�n
				  strLinkedProgramID = GetProgIdFromExtension(strExtension);
				// Si no hay nada asociado, se crean las claves, si hay algo asociado se modifican
				  if (string.IsNullOrEmpty(strLinkedProgramID) || strLinkedProgramID.Length == 0)
				    {	// Crear la clave con la extensi�n
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
				      // Si es un comando que se a�ade, no existir�
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
    ///		Permite quitar un comando asociado con una extensi�n
    /// </summary>
    public static void DeleteLinkCommandExtension(string strExtension, string strCommand)
    { if (!string.IsNullOrEmpty(strCommand) && !strCommand.Equals("open", StringComparison.CurrentCultureIgnoreCase))
				{ string strProgramID;
				
						// Normaliza la extensi�n
							strExtension = NormalizeExtension(strExtension);
						// Obtiene el ID de programa de la extensi�n
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
    /// Quita la extensi�n indicada de los tipos de archivos registrados
    /// </summary>
    public static void DeleteLinkProgramExtension(string strExtension)
    { string strProgramID;
    
				// Obtiene la extensi�n
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
    ///		Comprueba si la extensi�n indicada est� registrada.
    /// </summary>
    public static bool ExistsLink(string strExtension)
    { string strProgramID = GetProgIdFromExtension(NormalizeExtension(strExtension));

        return !string.IsNullOrEmpty(strProgramID) && strProgramID.Length > 0;
    }

		/// <summary>
		///		A�ade un punto al inicio de una extensi�n
		/// </summary>
		private static string NormalizeExtension(string strExtension)
		{ if (!strExtension.StartsWith("."))
				return "." + strExtension;
			else
				return strExtension;
		}
		
    /// <summary>
    ///		M�todo para obtener el ID de programa de una extensi�n
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
    /// A�ade al registro de Windows la aplicaci�n usando el t�tulo indicado
    /// </summary>
    public static void AddToWindowsStart(string strTitle, string strExeFileName)
    {	using(RegistryKey objRegistryKey = Registry.LocalMachine.OpenSubKey(cnstStrWindowsRun, true))
				{	if(objRegistryKey != null)
						objRegistryKey.SetValue(strTitle, "\"" + strExeFileName + "\"");
        }
    }

    /// <summary>
    ///		Quita del registro de Windows la aplicaci�n relacionada con el t�tulo indicado
    /// </summary>
    public static void RemoveFromWindowsStart(string strTitle)
    {	using(RegistryKey objRegistryKey = Registry.LocalMachine.OpenSubKey(cnstStrWindowsRun, true))
				{	if(objRegistryKey != null)
						objRegistryKey.DeleteValue(strTitle, false);
        }
    }

    /// <summary>
    ///		Comprueba si una aplicaci�n con el t�tulo indicado est� en el inicio de Windows
    /// </summary>
    public static bool IsAtWindowsStart(string strTitle, out string strFileName)
    {	// Inicializa el valor de salida
				strFileName = null;
			// Obtiene el valor del registro
          using(RegistryKey objRegistryKey = Registry.LocalMachine.OpenSubKey(cnstStrWindowsRun, true))
						{	if(objRegistryKey != null)
								strFileName = objRegistryKey.GetValue(strTitle).ToString();
						}
			// Devuelve el valor que indica si est� en el inicio de Windows
				return !string.IsNullOrEmpty(strFileName);
    }
	}
}
