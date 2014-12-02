using System;

using Bau.Libraries.NhamlCompiler.Parser.Tokens;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Sentencia if
	/// </summary>
	internal class InstructionIf : InstructionBase
	{
		internal InstructionIf(Token objToken) : base(objToken) 
		{ Condition = new ExpressionsCollection();
			ConditionRPN = new ExpressionsCollection();
			InstructionsElse = new InstructionsBaseCollection();
		}

		/// <summary>
		///		Obtiene la información de depuración
		/// </summary>
		protected override string GetDebugInfo(int intIndent)
		{ string strDebug = Environment.NewLine + GetIndent(intIndent + 1) + " --> Condición "  + Condition.GetDebugInfo();

				// Condiciones
					strDebug += Environment.NewLine + GetIndent(intIndent + 1) + " --> Condición RPN " + ConditionRPN.GetDebugInfo();
				// Else
					strDebug += Environment.NewLine + GetIndent(intIndent);
					if (InstructionsElse == null || InstructionsElse.Count == 0)
						strDebug += "Sin sentencia else";
					else
						strDebug += "Else " + Environment.NewLine + InstructionsElse.GetDebugString(intIndent + 1);
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

		/// <summary>
		///		Instrucciones de la parte else
		/// </summary>
		internal InstructionsBaseCollection InstructionsElse { get; set; }
	}
}
