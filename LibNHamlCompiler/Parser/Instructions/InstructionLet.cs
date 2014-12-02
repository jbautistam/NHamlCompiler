using System;

using Bau.Libraries.NhamlCompiler.Parser.Tokens;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Instrucción para asignar un valor a una variable
	/// </summary>
	internal class InstructionLet : InstructionBase
	{
		internal InstructionLet(Token objToken) : base(objToken) 
		{ Variable = new ExpressionVariableIdentifier(objToken);
			Expressions = new ExpressionsCollection();
			ExpressionsRPN = new ExpressionsCollection();
		}

		/// <summary>
		///		Obtiene la información de depuración
		/// </summary>
		protected override string GetDebugInfo(int intIndent)
		{ string strDebug = Environment.NewLine + GetIndent(intIndent + 1) + "--> Variable: " + Variable.GetDebugInfo();

				// Añade la información de las expresiones
					strDebug += Environment.NewLine + GetIndent(intIndent + 1) + "--> Expresiones: " + Expressions.GetDebugInfo();
					strDebug += Environment.NewLine + GetIndent(intIndent + 1) + "--> Expresiones RPN: " + ExpressionsRPN.GetDebugInfo();
				// Devuelve la información de depuración
					return strDebug;
		}

		/// <summary>
		///		Variable a la que se asigna el valor
		/// </summary>
		internal ExpressionVariableIdentifier Variable { get; set; }

		/// <summary>
		///		Expresiones a asignar
		/// </summary>
		internal ExpressionsCollection Expressions { get; set; }

		/// <summary>
		///		Expresiones a asignar en formato polaca inversa
		/// </summary>
		internal ExpressionsCollection ExpressionsRPN { get; set; }
	}
}
