using System;

using Bau.Libraries.NhamlCompiler.Parser.Tokens;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Instrucción para ejecutar un bucle foreach
	/// </summary>
	internal class InstructionForEach : InstructionBase
	{
		internal InstructionForEach(Token objToken) : base(objToken) 
		{ IndexVariable = new ExpressionVariableIdentifier(objToken);
			ListVariable = new ExpressionVariableIdentifier(objToken);
		}

		/// <summary>
		///		Información de depuración
		/// </summary>
		protected override string GetDebugInfo(int intIndent)
		{ return Environment.NewLine + base.GetIndent(intIndent) + 
								" Variable: " + IndexVariable.GetDebugInfo() + " List: " + ListVariable.GetDebugInfo();
		}

		/// <summary>
		///		Variable índice
		/// </summary>
		internal ExpressionVariableIdentifier IndexVariable { get; set; }

		/// <summary>
		///		Lista de variables
		/// </summary>
		internal ExpressionVariableIdentifier ListVariable { get; set; }
	}
}
