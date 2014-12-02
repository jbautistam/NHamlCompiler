using System;
using System.Collections.Generic;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Colección de <see cref="ExpressionBase"/>
	/// </summary>
	internal class ExpressionsCollection : List<ExpressionBase>
	{
		/// <summary>
		///		Obtiene una cadena de depuración
		/// </summary>
		internal string GetDebugInfo()
		{ string strDebug = "";

				// Añade los datos a la cadena de depuración
					foreach (ExpressionBase objExpression in this)
						{ if (!string.IsNullOrEmpty(strDebug))
								strDebug += " # ";
							strDebug += objExpression.GetDebugInfo();
						}
				// Devuelve la cadena de depuración
					return strDebug;
		}

		/// <summary>
		///		Clona la colección de expresiones
		/// </summary>
		internal ExpressionsCollection Clone()
		{ ExpressionsCollection objColExpressions = new ExpressionsCollection();

				// Clona las expresiones
					foreach (ExpressionBase objExpression in this)
						objColExpressions.Add(objExpression.Clone());
				// Devuelve la colección
					return objColExpressions;
		}
	}
}
