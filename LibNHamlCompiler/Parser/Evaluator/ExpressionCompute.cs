using System;
using System.Collections.Generic;

using Bau.Libraries.NhamlCompiler.Parser.Instructions;
using Bau.Libraries.NhamlCompiler.Variables;

namespace Bau.Libraries.NhamlCompiler.Parser.Evaluator
{
	/// <summary>
	///		Clase para el cálculo de expresiones
	/// </summary>
	internal class ExpressionCompute
	{
		internal ExpressionCompute(VariablesCollection objColVariables)
		{ if (objColVariables == null)
				Variables = new VariablesCollection();
			else
				Variables = objColVariables.Clone();
		}

		/// <summary>
		///		Evalúa una serie de expresiones
		/// </summary>
		internal ValueBase Evaluate(ExpressionsCollection objStackExpressions)
		{ return Compute(objStackExpressions.Clone());
		}

		/// <summary>
		///		Busca una variable en la colección
		/// </summary>
		internal Variable Search(ExpressionVariableIdentifier objExpressionVariable, out string strError)
		{ return Search(objExpressionVariable, Variables, out strError);
		}

		/// <summary>
		///		Busca recursivamente una variable y su miembro
		/// </summary>
		private Variable Search(ExpressionVariableIdentifier objExpressionVariable, VariablesCollection objColVariables, out string strError)
		{ Variable objVariable = null;
			int intIndex = 0;

				// Inicializa las variables de salida
					strError = null;
				// Obtiene el índice asociado a la variable
					if (objExpressionVariable.IndexExpressionsRPN != null && objExpressionVariable.IndexExpressionsRPN.Count > 0)
						{ ValueBase objIndex = Compute(objExpressionVariable.IndexExpressionsRPN);

								if (objIndex.HasError)
									strError = objIndex.Error;
								else if (!(objIndex is ValueNumeric))
									strError = "La expresión del índice no tiene un valor numérico";
								else
									intIndex = (int) (objIndex as ValueNumeric).Value;
						}
				// Si no hay ningún error, obtiene la variable
					if (string.IsNullOrWhiteSpace(strError))
						{ // Obtiene la variable
								objVariable = objColVariables.Search(objExpressionVariable.Name, intIndex);
							// Si tiene algún miembro, busca ese miembro
								if (objExpressionVariable.Member != null && !string.IsNullOrWhiteSpace(objExpressionVariable.Member.Name))
									objVariable = Search(objExpressionVariable.Member, objVariable.Members, out strError);
						}
				// Devuelve la variable
					return objVariable;
		}

		/// <summary>
		///		Calcula una expresión
		/// </summary>
		private ValueBase Compute(ExpressionsCollection objStackExpressions)
		{ Stack<ValueBase> objStackOperators = new Stack<ValueBase>();
			bool blnError = false;

				// Calcula el resultado
					foreach (ExpressionBase objExpression in objStackExpressions)
						if (!blnError)
							{ if (objExpression.Token.Type == Tokens.Token.TokenType.String || objExpression.Token.Type == Tokens.Token.TokenType.Number)
									objStackOperators.Push(ValueBase.GetInstance(objExpression.Token.Content));
								else if (objExpression is ExpressionVariableIdentifier)
									{ ValueBase objVariableValue = GetValueVariable(objExpression as ExpressionVariableIdentifier);

											// Añade el resultado a la pila (aunque haya un error, para que así este sea el último operando en la pila)
												if (objVariableValue != null)
													{ blnError = objVariableValue.HasError;
														objStackOperators.Push(objVariableValue);
													}
												else
													{ blnError = true;
														objStackOperators.Push(ValueBase.GetError("No se encuentra el valor de la variable"));
													}
									}
								else
									{ ValueBase objResult = null;

											// Realiza la operación
												switch (objExpression.Token.Content)
													{ case "+":
														case "-":
														case "*":
														case "/":
														case ">=":
														case "<=":
														case "==":
														case "!=":
														case ">":
														case "<":
														case "||":
														case "&&":
																objResult = ComputeBinary(objStackOperators, objExpression.Token.Content);
															break;
														default:
																objResult = ValueBase.GetError("Operador desconocido: " + objExpression.Token.Content);
															break;
													}
											// Añade el resultado a la pila (aunque haya error, para que así sea el último operador de la pila)
												blnError = objResult.HasError;
												objStackOperators.Push(objResult);
									}
							}
				// Obtiene el resultado
					if (blnError || objStackOperators.Count == 1)
						return objStackOperators.Pop();
					else if (objStackOperators.Count == 0)
						return ValueBase.GetError("No hay ningún operador en la pila de operaciones");
					else 
						return ValueBase.GetError("Hay más de un operador en la pila de instrucciones");
		}

		/// <summary>
		///		Obtiene el valor contenido en una variable
		/// </summary>
		private ValueBase GetValueVariable(ExpressionVariableIdentifier objVariableIdentifier)
		{ string strError;
			Variable objVariable = Search(objVariableIdentifier, out strError);

				if (!string.IsNullOrWhiteSpace(strError))
					return ValueBase.GetError(strError);
				else
					return objVariable.Value;
		}

		/// <summary>
		///		Calcula una operación con dos valores
		/// </summary>
		private ValueBase ComputeBinary(Stack<ValueBase> objStackOperators, string strOperator)
		{ if (objStackOperators.Count < 2)
				return ValueBase.GetError("No existen suficientes operandos en la pila para ejecutar el operador '" + strOperator + "'");
			else
				return objStackOperators.Pop().Execute(objStackOperators.Pop(), strOperator);
		}

		/// <summary>
		///		Variables
		/// </summary>
		internal VariablesCollection Variables { get; set; }
	}
}
