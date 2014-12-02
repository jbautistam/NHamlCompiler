using System;

using Bau.Libraries.NhamlCompiler.Parser.Tokens;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Base para las clases de instrucción
	/// </summary>
	internal class InstructionBase
	{ // Variables privadas
			private string strError;

		internal InstructionBase(Token objToken)
		{ Token = objToken;
			Instructions = new InstructionsBaseCollection();
			IsError = false; 
		}

		/// <summary>
		///		Obtiene una cadena con el tipo
		/// </summary>
		internal string GetDebugString(int intIndent = 0)
		{ string strMessage = GetIndent(intIndent); 
		
				// Añade los datos básicos
					strMessage += Token.ToString() + " (" + ToString() + ")";
				// Añade el error
					if (IsError)
						strMessage += " Error: " + Error;
				// Añade la depuración de las instrucciones
					strMessage += Instructions.GetDebugString(intIndent + 1);
				// Añade la depuración interna
					strMessage += GetDebugInfo(intIndent);
				// Devuelve la información de depuración
					return strMessage;
		}

		/// <summary>
		///		Obtiene una cadena para indentar
		/// </summary>
		protected string GetIndent(int intIndent)
		{ string strMessage = "";

				// Obtiene la cadena de indentación
					for (int intIndex = 0; intIndex < intIndent; intIndex++)
						strMessage += "    ";
				// Devuelve la cadena de indentación
					return strMessage;
		}

		/// <summary>
		///		Obtiene la información de depuración
		/// </summary>
		protected virtual string GetDebugInfo(int intIndent)
		{ return "";
		}

		/// <summary>
		///		Token al que se asocia la instrucción
		/// </summary>
		internal Token Token { get; private set; }

		/// <summary>
		///		Indica si la instrucción tiene un error
		/// </summary>
		internal bool IsError { get; set; }

		/// <summary>
		///		Indica el error de la instrucción
		/// </summary>
		internal string Error 
		{ get 
				{ if (string.IsNullOrWhiteSpace(strError))
						return "Error no definido";
					else
						return strError;
				}
			set 
				{ strError = value; 
					IsError = true;
				}
		}

		/// <summary>
		///		Instrucciones
		/// </summary>
		internal InstructionsBaseCollection Instructions { get; set; }
	}
}
