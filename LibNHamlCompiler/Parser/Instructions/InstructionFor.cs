using System;

using Bau.Libraries.NhamlCompiler.Parser.Tokens;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Instrucción para ejecutar un bucle for
	/// </summary>
	internal class InstructionFor : InstructionBase
	{
		internal InstructionFor(Token objToken) : base(objToken) 
		{ IndexVariable = new ExpressionVariableIdentifier(objToken);
			StartValue = new ExpressionsCollection();
			StartValueRPN = new ExpressionsCollection();
			EndValue = new ExpressionsCollection();
			EndValueRPN = new ExpressionsCollection();
			StepValue = new ExpressionsCollection();
			StepValueRPN = new ExpressionsCollection();
		}

		/// <summary>
		///		Información de depuración
		/// </summary>
		protected override string GetDebugInfo(int intIndent)
		{ return Environment.NewLine + base.GetIndent(intIndent) + 
								" Variable: " + IndexVariable.GetDebugInfo() + 
								" StartValue: " + StartValue.GetDebugInfo() + " StartValueRPN: " + StartValueRPN.GetDebugInfo() +
								" EndValue: " + EndValue.GetDebugInfo() + " EndValueRPN: " + EndValueRPN.GetDebugInfo() +
								" StepValue: " + StepValue.GetDebugInfo() + " StepValueRPN: " + StepValueRPN.GetDebugInfo();
		}

		/// <summary>
		///		Variable índice
		/// </summary>
		internal ExpressionVariableIdentifier IndexVariable { get; set; }

		/// <summary>
		///		Valor inicial
		/// </summary>
		internal ExpressionsCollection StartValue { get; set; }

		/// <summary>
		///		Valor inicial (RPN)
		/// </summary>
		internal ExpressionsCollection StartValueRPN { get; set; }

		/// <summary>
		///		Valor final
		/// </summary>
		internal ExpressionsCollection EndValue { get; set; }

		/// <summary>
		///		Valor final (RPN)
		/// </summary>
		internal ExpressionsCollection EndValueRPN { get; set; }

		/// <summary>
		///		Valor del paso
		/// </summary>
		internal ExpressionsCollection StepValue { get; set; }

		/// <summary>
		///		Valor del paso (RPN)
		/// </summary>
		internal ExpressionsCollection StepValueRPN { get; set; }
	}
}
