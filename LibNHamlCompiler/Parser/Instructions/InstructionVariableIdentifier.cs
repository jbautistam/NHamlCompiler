using System;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Instrucción con un identificador de variable
	/// </summary>
	internal class InstructionVariableIdentifier : InstructionBase
	{
		internal InstructionVariableIdentifier(Tokens.Token objToken) : base(objToken) 
		{ Variable = new ExpressionVariableIdentifier(objToken);
		}

		/// <summary>
		///		Obtiene la información de depuración
		/// </summary>
		protected override string GetDebugInfo(int intIndent)
		{ return Environment.NewLine + GetIndent(intIndent + 1) + " --> Variable " + Variable.GetDebugInfo();
		}

		/// <summary>
		///		Expresión que identifica la variable
		/// </summary>
		internal ExpressionVariableIdentifier Variable { get; set; }
	}
}
