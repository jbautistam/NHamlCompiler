using System;

using Bau.Libraries.LibHelper.Extensors;
using Bau.Libraries.NhamlCompiler.Parser.Evaluator;
using Bau.Libraries.NhamlCompiler.Parser.Instructions;
using Bau.Libraries.NhamlCompiler.Parser.Tokens;
using Bau.Libraries.NhamlCompiler.Variables;

namespace Bau.Libraries.NhamlCompiler.Parser.Translator
{
	/// <summary>
	///		Intérprete
	/// </summary>
	internal class Interpreter
	{ // Constantes privadas
			private const string cnstStrCharNoSpace = ".,;:)]}"; // ... caracteres que no precisan que se añada un espacio anterior en la salida

		internal Interpreter(Compiler objCompiler, VariablesCollection objColVariables, int intMaxInstructions = 0, bool blnIsCompressed = false)
		{ Compiler = objCompiler;
			MaxInstructions = intMaxInstructions;
			Builder = new Writer.StringBuilderHtml(blnIsCompressed);
			ExpressionComputer = new ExpressionCompute(objColVariables);
		}

		/// <summary>
		///		Interpreta un programa
		/// </summary>
		internal string Parse(string strSource)
		{ Parser.Lexical.ParserManager objParser = new Parser.Lexical.ParserManager(Compiler);

				// Interpreta las líneas de programa
					objParser.Parse(strSource);
				// Ejecuta las instrucciones
					if (Compiler.LocalErrors.Count > 0)
						Builder.Add("Error en la interpretación");
					else if (MaxInstructions > 0)
						Execute(objParser.Instructions.Select(MaxInstructions));
					else
						Execute(objParser.Instructions);
				// Devuelve la cadena resultante
					return Builder.Builder.ToString();
		}

		/// <summary>
		///		Ejecuta el programa
		/// </summary>
		private void Execute(InstructionsBaseCollection objColInstructions)
		{ if (objColInstructions != null)
				foreach (Instructions.InstructionBase objInstruction in objColInstructions)
					Execute(objInstruction);
		}

		/// <summary>
		///		Ejecuta una instrucción
		/// </summary>
		private void Execute(InstructionBase objInstruction)
		{	if (objInstruction is InstructionNhaml)
				Execute(objInstruction as InstructionNhaml);
			else if (objInstruction is InstructionComment)
				Execute(objInstruction as InstructionComment);
			else if (objInstruction is InstructionCode)
				Execute(objInstruction as InstructionCode);
			else if (objInstruction is InstructionForEach)
				Execute(objInstruction as InstructionForEach);
			else if (objInstruction is InstructionFor)
				Execute(objInstruction as InstructionFor);
			else if (objInstruction is InstructionIf)
				Execute(objInstruction as InstructionIf);
			else if (objInstruction is InstructionWhile)
				Execute(objInstruction as InstructionWhile);
			else if (objInstruction is InstructionLet)
				Execute(objInstruction as InstructionLet);
			else if (objInstruction.Token.Type != Token.TokenType.EOF)
				Compiler.LocalErrors.Add(objInstruction.Token, "Error en la ejecución. Instrucción desconocida");
		}

		/// <summary>
		///		Ejecuta una instrucción Nhaml
		/// </summary>
		private void Execute(InstructionNhaml objInstruction)
		{ int intIndex = 0;

				// Asigna la indentación
					Builder.Indent = objInstruction.Token.Indent;
				// Añade la etiqueta de apertura
					Builder.AddTag(GetTagHtml(objInstruction.Token, true) + " " + GetAttributes(objInstruction), false, objInstruction.IsInner);
				// Añade los literales
					foreach (InstructionBase objInnerInstruction in objInstruction.Instructions)
						if (objInnerInstruction is InstructionComment)
							Execute(objInnerInstruction as InstructionComment);
						else if (objInnerInstruction is InstructionNhaml || objInnerInstruction.Token.Type == Token.TokenType.Sentence) // ... tiene que estar antes de comprobar si es una instrucción base
							Execute(objInnerInstruction);
						else if (objInnerInstruction is InstructionVariableIdentifier)
							Builder.Add(" " + GetVariableValue(objInnerInstruction as InstructionVariableIdentifier));
						else if (objInnerInstruction is InstructionBase || objInnerInstruction is InstructionLiteral)
							{ bool blnAddSpace = MustAddSpace(intIndex, objInnerInstruction.Token.Content);
								string strContent = objInnerInstruction.Token.Content;

									// Añade el contenido
										Builder.Add((blnAddSpace ? " " : "") + strContent);
									// Incrementa el índice
										intIndex++;
							}
				// Añade la etiqueta de cierre
					Builder.Indent = objInstruction.Token.Indent;
					Builder.AddTag(GetTagHtml(objInstruction.Token, false), true, objInstruction.IsInner);
		}

		/// <summary>
		///		Obtiene la etiqueta de Html
		/// </summary>
		private string GetTagHtml(Token objToken, bool blnStart)
		{ string strHtml;

				// Obtiene la etiqueta
					if (objToken.Content.StartsWith("&"))
						{	if (blnStart)
								strHtml = "div id=\"" + objToken.Content.Substring(1) + "\"";
							else
								strHtml = "div";
						}
					else
						strHtml = objToken.Content.Substring(1);
				// Devuelve la etiqueta
					return strHtml;
		}

		/// <summary>
		///		Obtiene el valor de una variable
		/// </summary>
		private string GetVariableValue(InstructionVariableIdentifier objInstruction)
		{ ValueBase objValue = GetVariable(objInstruction);

				if (objValue == null)
					return "##Error al obtener la variable " + objInstruction.Variable.Name;
				else if (objValue.Content != null)
					return objValue.Content;
				else
					return "";
		}

		/// <summary>
		///		Obtiene una variable
		/// </summary>
		private ValueBase GetVariable(InstructionVariableIdentifier objInstruction)
		{ string strError;
			Variable objVariable = ExpressionComputer.Search(objInstruction.Variable, out strError);

				if (!string.IsNullOrWhiteSpace(strError))
					{ Compiler.LocalErrors.Add(objInstruction.Token, strError);
						return ValueBase.GetError("## Error al obtener la variable: " + strError + " ##");
					}
				else if (objVariable == null) // ... nunca se debería dar
					{ Compiler.LocalErrors.Add(objInstruction.Token, "No se encuentra el valor de la variable " + objInstruction.Variable.Name);
						return ValueBase.GetError("## No se encuentra la variable: " + objInstruction.Variable.Name + " ##");
					}
				else
					return objVariable.Value;
		}

		/// <summary>
		///		Obtiene los atributos de una instrucción
		/// </summary>
		private string GetAttributes(InstructionNhaml objInstruction)
		{ string strAttributes = "";

				// Añade los parámetros
					foreach (Parameter objParameter in objInstruction.Attributes)
						{ ValueBase objResult = ExpressionComputer.Evaluate(objParameter.VariableRPN);

								// Añade el nombre
									strAttributes += " " + objParameter.Name + "=";
								// Añade el valor
									if (objResult.HasError)
										Compiler.LocalErrors.Add(objInstruction.Token, objResult.Error);
									else
										strAttributes += "\"" + objResult.Content + "\"";
						}
				// Devuelve los atributos
					return strAttributes;
		}

		/// <summary>
		///		Ejecuta una instrucción de código
		/// </summary>
		private void Execute(InstructionCode objInstruction)
		{ // Añade el contenido	
				Builder.Add(Environment.NewLine + objInstruction.Content);
		}

		/// <summary>
		///		Ejecuta un comentario
		/// </summary>
		private void Execute(InstructionComment objInstruction)
		{ if (!Builder.IsCompressed)
				{ // Añade el inicio de comentario
						Builder.Indent = objInstruction.Token.Indent;
						Builder.AddIndent();
						Builder.Add("<!--");
					// Añade el texto
						Builder.Add(objInstruction.Content);
					// Añade el fin de comentario
						Builder.Add("-->");
				}
		}

		/// <summary>
		///		Ejecuta una instrucción if
		/// </summary>
		private void Execute(InstructionIf objInstruction)
		{ ValueBase objResult = ExpressionComputer.Evaluate(objInstruction.ConditionRPN);

				if (objResult.HasError)
					Compiler.LocalErrors.Add(objInstruction.Token, objResult.Error);
				else if (!(objResult is ValueBool))
					Compiler.LocalErrors.Add(objInstruction.Token, "El resultado de calcular la expresión no es un valor lógico");
				else if ((objResult as ValueBool).Value)
					Execute(objInstruction.Instructions);
				else
					Execute(objInstruction.InstructionsElse);
		}

		/// <summary>
		///		Ejecuta una instrucción while
		/// </summary>
		private void Execute(InstructionWhile objInstruction)
		{ bool blnEnd = false;
			int intLoopIndex = 0;

				// Ejecuta las instrucciones en un bucle
					do
						{ ValueBase objResult = ExpressionComputer.Evaluate(objInstruction.ConditionRPN);

								if (objResult.HasError)
									{ Compiler.LocalErrors.Add(objInstruction.Token, objResult.Error);
										blnEnd = true;
									}
								else if (!(objResult is ValueBool))
									{ Compiler.LocalErrors.Add(objInstruction.Token, "El resultado de calcular la expresión no es un valor lógico");
										blnEnd = true;
									}
								else if (!(objResult as ValueBool).Value)
									blnEnd = true;
								else
									Execute(objInstruction.Instructions);
						}
					while (!blnEnd && ++intLoopIndex < Compiler.MaximumRepetitionsLoop);
		}

		/// <summary>
		///		Ejecuta una instrucción for
		/// </summary>
		private void Execute(InstructionFor objInstruction)
		{ string strError;
			Variable objVariableIndex = ExpressionComputer.Search(objInstruction.IndexVariable, out strError);

				if (!strError.IsEmpty())
					Compiler.LocalErrors.Add(objInstruction.Token, "Error al obtener la variable índice: " + strError);
				else
					{ ValueBase objValueStart = ExpressionComputer.Evaluate(objInstruction.StartValueRPN);
						
							if (objValueStart.HasError)
								Compiler.LocalErrors.Add(objInstruction.Token, "Error al obtener el valor de inicio del bucle for " + objValueStart.Error);
							else if (!(objValueStart is ValueNumeric))
								Compiler.LocalErrors.Add(objInstruction.Token, "El valor de inicio del bucle for no es un valor numérico");
							else
								{ ValueBase objValueEnd = ExpressionComputer.Evaluate(objInstruction.EndValueRPN);

										if (objValueEnd.HasError)
											Compiler.LocalErrors.Add(objInstruction.Token, "Error al obtener el valor de fin del bucle for " + objValueEnd.Error);
										else if (!(objValueEnd is ValueNumeric))
											Compiler.LocalErrors.Add(objInstruction.Token, "El valor de fin del bucle for no es un valor numérico");
										else
											{ ValueBase objValueStep;
											
													// Obtiene el valor del paso
														if (objInstruction.StepValueRPN == null || objInstruction.StepValueRPN.Count == 0)
															objValueStep = ValueBase.GetInstance("1");
														else
															objValueStep = ExpressionComputer.Evaluate(objInstruction.StepValueRPN);
													// Comprueba los errores antes de entrar en el bucle
														if (objValueStep.HasError)
															Compiler.LocalErrors.Add(objInstruction.Token, "Error al obtener el valor de paso del bucle for " + objValueEnd.Error);
														else if (!(objValueEnd is ValueNumeric))
															Compiler.LocalErrors.Add(objInstruction.Token, "El valor de paso del bucle for no es un valor numérico");
														else
															{ int intIndexLoop = 0;
																int intStart = (int) (objValueStart as ValueNumeric).Value;
																int intEnd = (int) (objValueEnd as ValueNumeric).Value;
																int intStep = (int) (objValueStep as ValueNumeric).Value;
																int intIndex = intStart;

																	// Cambia el valor de la variable de índice
																		objVariableIndex.Value = ValueBase.GetInstance(intIndex.ToString());
																	// Ejecuta las instrucciones del bucle
																		while (intIndex <= intEnd && intIndexLoop < Compiler.MaximumRepetitionsLoop)
																			{ // Ejecuta las instrucciones del bucle
																					Execute(objInstruction.Instructions);
																				// Incrementa la variable índice y cambia el valor de la variable
																					intIndex += intStep;
																					objVariableIndex.Value = ValueBase.GetInstance(intIndex.ToString());
																				// Incrementa el número de iteraciones
																					intIndexLoop++;
																			}
																	// Comprueba el número de iteraciones
																		if (intIndexLoop >= Compiler.MaximumRepetitionsLoop)
																			Compiler.LocalErrors.Add(objInstruction.Token, "Se ha sobrepasado el número máximo de iteraciones del bucle for");
															}
											}
								}
					}
		}

		/// <summary>
		///		Ejecuta las instrucciones de un bucle foreach
		/// </summary>
		private void Execute(InstructionForEach objInstruction)
		{ string strError;
			Variable objVariable = ExpressionComputer.Search(objInstruction.IndexVariable, out strError);

				if (!string.IsNullOrEmpty(strError))
					Compiler.LocalErrors.Add(objInstruction.Token, strError);
				else
					{ VariablesCollection objColVariables = ExpressionComputer.Search(objInstruction.ListVariable, out strError).Members;

							if (!string.IsNullOrEmpty(strError))
								Compiler.LocalErrors.Add(objInstruction.Token, strError);
							else
								{ // Ordena las variables por su índice
										objColVariables.SortByIndex();
									// Recorre las variables ejecutando el código (en realidad puede que no fuera necesario comprobar el número de iteraciones porque
									// la colección de variables no se va a modificar por mucho que lo intente el código Nhaml)
										for (int intIndex = 0; intIndex < objColVariables.Count && intIndex < Compiler.MaximumRepetitionsLoop; intIndex++)
											{ // Asigna el contenido a la variable
													objVariable.Value = objColVariables[intIndex].Value;
												// Ejecuta las instrucciones
													Execute(objInstruction.Instructions);
											}
								}
					}
		}

		/// <summary>
		///		Ejecuta una instrucción de asignación
		/// </summary>
		private void Execute(InstructionLet objInstruction)
		{ string strError;
			Variable objVariable = ExpressionComputer.Search(objInstruction.Variable, out strError);

				if (!string.IsNullOrWhiteSpace(strError))
					Compiler.LocalErrors.Add(objInstruction.Token, strError);
				else
					{ ValueBase objValue = ExpressionComputer.Evaluate(objInstruction.ExpressionsRPN);

							if (objValue.HasError)
								Compiler.LocalErrors.Add(objInstruction.Token, objValue.Error);
							else
								objVariable.Value = objValue;
					}
		}

		/// <summary>
		///		Comprueba si se debe añadir un espacio
		/// </summary>
		private bool MustAddSpace(int intIndex, string strText)
		{ bool blnAddSpace = intIndex != 0;

				// Comprueba si debe añadir el espacio
					if (blnAddSpace)
						foreach (char chrChar in cnstStrCharNoSpace)
							if (strText.StartsWith(chrChar.ToString()))
								blnAddSpace = false;
				// Devuelve si se debe añadir un espacio
					return blnAddSpace;
		}

		/// <summary>
		///		Generador de HTML
		/// </summary>
		private Writer.StringBuilderHtml Builder { get; set; }

		/// <summary>
		///		Compilado
		/// </summary>
		private Compiler Compiler { get; set; }

		/// <summary>
		///		Número máximo de instrucciones a compilar
		/// </summary>
		private int MaxInstructions { get; set; }

		/// <summary>
		///		Objeto de ejecución de expresiones
		/// </summary>
		private ExpressionCompute ExpressionComputer { get; set; }
	}
}
