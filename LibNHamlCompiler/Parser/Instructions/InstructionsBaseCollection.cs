using System;
using System.Collections.Generic;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Colección de <see cref="InstructionBase"/>
	/// </summary>
	internal class InstructionsBaseCollection : List<InstructionBase>
	{
		/// <summary>
		///		Cadena con el contenido de la colección
		/// </summary>
		internal string GetDebugString(int intIndent = 0)
		{ string strMessage = "";

				// Obtiene la cadena de depuración
					foreach (InstructionBase objInstruction in this)
						strMessage += Environment.NewLine + objInstruction.GetDebugString(intIndent);
				// Devuelve el mensaje
					return strMessage;
		}

		/// <summary>
		///		Selecciona las primeras n instrucciones
		/// </summary>
		internal InstructionsBaseCollection Select(int intMaxInstructions)
		{ InstructionsBaseCollection objColInstructions = new InstructionsBaseCollection();

				// Obtiene las primeras instrucciones
					for (int intIndex = 0; intIndex < intMaxInstructions && intIndex < Count; intIndex++)
						objColInstructions.Add(this[intIndex]);
				// Devuelve las instrucciones
					return objColInstructions;
		}
	}
}
