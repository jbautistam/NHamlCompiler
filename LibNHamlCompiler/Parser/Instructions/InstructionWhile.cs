using System;

using Bau.Libraries.NhamlCompiler.Parser.Tokens;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Sentencia While
	/// </summary>
	internal class InstructionWhile : InstructionBase
	{
		internal InstructionWhile(Token objToken) : base(objToken) 
		{ Condition = new ExpressionsCollection();
			ConditionRPN = new ExpressionsCollection();
		}

		/// <summary>
		///		Obtiene la información de depuración
		/// </summary>
		protected override string GetDebugInfo(int intIndent)
		{ string strDebug = Environment.NewLine + GetIndent(intIndent + 1) + " --> Condición "  + Condition.GetDebugInfo();

				// Condiciones
					strDebug += Environment.NewLine + GetIndent(intIndent + 1) + " --> Condición RPN " + ConditionRPN.GetDebugInfo();
				// Devuelve la cadena de depuración
					return strDebug;
		}

		/// <summary>
		///		Expresiones que forman la condición
		/// </summary>
		internal ExpressionsCollection Condition { get; set; }

		/// <summary>
		///		Condiciones en formato polaca inversa
		/// </summary>
		internal ExpressionsCollection ConditionRPN { get; set; }
	}
}
