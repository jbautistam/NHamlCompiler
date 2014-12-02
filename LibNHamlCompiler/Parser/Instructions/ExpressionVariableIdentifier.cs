using System;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Expresión de tipo variable
	/// </summary>
	internal class ExpressionVariableIdentifier : ExpressionBase
	{
		internal ExpressionVariableIdentifier(Tokens.Token objToken) : base(objToken) 
		{ Name = objToken.Content;
			IndexExpressions = new ExpressionsCollection();
			IndexExpressionsRPN = new ExpressionsCollection();
		}

		/// <summary>
		///		Clona el identificador de variable
		/// </summary>
		internal override ExpressionBase Clone()
		{ ExpressionVariableIdentifier objVariable = new ExpressionVariableIdentifier(base.Token);

				// Clona las expresiones
					objVariable.IndexExpressions = IndexExpressions.Clone();
					objVariable.IndexExpressionsRPN = IndexExpressionsRPN.Clone();
					if (Member != null)
						objVariable.Member = Member.Clone() as ExpressionVariableIdentifier;
				// Devuelve el objeto clonado
					return objVariable;
		}

		/// <summary>
		///		Obtiene la información de depuración
		/// </summary>
		internal override string GetDebugInfo()
		{ string strDebug = Name;
			
				// Añade el índice
					if (IndexExpressions != null && IndexExpressions.Count > 0)
						strDebug += "[" + IndexExpressions.GetDebugInfo() + "]";
				// Añade el índice en formato RPN
					if (IndexExpressionsRPN != null && IndexExpressionsRPN.Count > 0)
						strDebug += " (RPN: " + IndexExpressionsRPN.GetDebugInfo() + ")";
				// Añade el miembro
					if (Member != null)
						strDebug += "->" + Member.GetDebugInfo();
				// Devuelve la información de depuración
					return strDebug;
		}

		/// <summary>
		///		Nombre de la variable
		/// </summary>
		internal string Name { get; set; }

		/// <summary>
		///		Expresiones de índice
		/// </summary>
		internal ExpressionsCollection IndexExpressions { get; set; }

		/// <summary>
		///		Expresiones del índice en formato RPN
		/// </summary>
		internal ExpressionsCollection IndexExpressionsRPN { get; set; }

		/// <summary>
		///		Identificador de variable
		/// </summary>
		internal ExpressionVariableIdentifier Member { get; set; }
	}
}
