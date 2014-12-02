using System;

using Bau.Libraries.NhamlCompiler.Parser.Tokens;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Clase con los datos de un atributo
	/// </summary>
	internal class Parameter
	{	
		internal Parameter()
		{ Variable = new ExpressionsCollection();
			VariableRPN = new ExpressionsCollection();
		}

		/// <summary>
		///		Nombre del atributo
		/// </summary>
		internal string Name { get; set; }

		/// <summary>
		///		Variable
		/// </summary>
		internal ExpressionsCollection Variable { get; set; }

		/// <summary>
		///		Variable (RPN)
		/// </summary>
		internal ExpressionsCollection VariableRPN { get; set; }
	}
}
