using System;

using Bau.Libraries.LibHelper.Extensors;
using Bau.Libraries.NhamlCompiler.Parser.Instructions;
using Bau.Libraries.NhamlCompiler.Parser.Tokens;

namespace Bau.Libraries.NhamlCompiler.Parser.Lexical
{
	/// <summary>
	///		Intérprete
	/// </summary>
	internal class ParserManager
	{ // Enumerados privados
			private enum ExpressionReaderMode
				{ AtAttributes,
					AtVariable,
					Normal
				}
		// Variables privadas
			private Evaluator.ExpressionConversorRpn objExpressionEvaluator;

		internal ParserManager(Compiler objCompiler)
		{ Compiler = objCompiler;
			Tokens = new TokensCollection();
			Instructions = new InstructionsBaseCollection();
			objExpressionEvaluator = new Evaluator.ExpressionConversorRpn();
		}

		/// <summary>
		///		Interpreta una cadena
		/// </summary>
		internal void Parse(string strSource)
		{ Token objLastToken;

				// Crea la colección de tokens
					Tokens = new StringTokenizer(strSource.TrimEnd()).GetAllTokens();
				// Depura la colección de tokens
					DebugTokens();
				// Lee las instrucciones
					objLastToken = ReadInstructions(Instructions);
					//if (objLastToken.Content == "<%else%>")
					//  Instructions.Add(new InstructionBase(objLastToken) { Error = "Fin de archivo inesperado" });
				// Depura las instrucciones
					DebugInstructions();
		}

		/// <summary>
		///		Lee las instrucciones
		/// </summary>
		private Token ReadInstructions(InstructionsBaseCollection objColInstructions)
		{ Token objNextToken = GetToken(true);
			int intFirstIndent;
			bool blnEnd = false;

				// Quita las instrucciones de fin y de inicio de comando
					while (objNextToken.Type == Token.TokenType.EndSentenceBlock)
						{ GetToken();
							objNextToken = GetToken(true);
						}
				// Obtiene la indentación inicial
					intFirstIndent = objNextToken.Indent;
				// Lee las instrucciones
					while (objNextToken.Type != Token.TokenType.EOF && objNextToken.Indent >= intFirstIndent && !blnEnd)
						{ InstructionsBaseCollection objColInnerInstructions = new InstructionsBaseCollection();

								// Obtiene el token real
									objNextToken = GetToken();
								// Trata las instrucciones
									switch (objNextToken.Type)
										{ case Token.TokenType.StartComment:
													objColInnerInstructions.Add(ReadComment(objNextToken));
												break;
											case Token.TokenType.StartSentenceBlock:
											case Token.TokenType.EndSentenceBlock:
														// ... no hace nada, simplemente se las salta
													break;
											case Token.TokenType.TagHTML:
													objColInnerInstructions.Add(ReadHtml(objNextToken));
												break;
											case Token.TokenType.Sentence:
													objColInnerInstructions.Add(ReadSentence(objNextToken));
												break;
											default:
													objColInnerInstructions.Add(new InstructionBase(objNextToken));
												break;
										}
								// Trata las instrucciones
									if (objColInnerInstructions.Count > 0)
										{ bool blnError = false;

												// Si hay algún error lo añade
													foreach (InstructionBase objInstruction in objColInnerInstructions)
														if (objInstruction.IsError)
															{ Compiler.LocalErrors.Add(objInstruction.Token, objInstruction.Error);
																blnError = true;
															}
												// Se recupera de los errores
													if (blnError)
														RecoverError();
												// En cualquier caso, mete las instrucciones en el buffer
													objColInstructions.AddRange(objColInnerInstructions);
										}
								// Obtiene el siguiente token
									objNextToken = GetToken(true);
								// Termina si es una sentencia else
									if (IsElseCommand(objNextToken))
										blnEnd = true;
						}
				// Devuelve el último token leído
					return objNextToken;
		}

		/// <summary>
		///		Lee un comentario
		/// </summary>
		private InstructionComment ReadComment(Token objToken)
		{ InstructionComment objInstruction = new InstructionComment(objToken);

				// Supone que hay algún error
					objInstruction.IsError = true;
				// Lee el siguiente token (cuerpo del comentario)
					objToken = GetToken();
					if (objToken.Type != Token.TokenType.EndComment)
						{ // Lee el contenido del comentario
								objInstruction.Content = objToken.Content;
							// Lee el siguiente token (fin del comentario)
								objToken = GetToken();
						}
				// Comprueba si es un fin de comentario
					if (objToken.Type == Token.TokenType.EndComment)
						objInstruction.IsError = false;
					else
						objInstruction.Error = "No se ha encontrado la etiqueta de fin de comentario";
				// Devuelve la instrucción
					return objInstruction;
		}

		/// <summary>
		///		Lee el Html de una página
		/// </summary>
		private InstructionNhaml ReadHtml(Token objToken)
		{ InstructionNhaml objInstruction = new InstructionNhaml(objToken);
			Token objNextToken = GetToken(true); // ... lee el siguiente token sin sacarlo del buffer
			bool blnEnd = false;
			
				// Si es una llave de apertura, lee los argumentos
					if (objNextToken.Type == Token.TokenType.LeftLlave)
						{ // Lee los atributos 
								ReadAttributes(objInstruction);
							// Lee el siguiente token
								objNextToken = GetToken(true);
						}
				// Recorre los tokens de la instrucción
					while (!IsEof(objNextToken) && !blnEnd && !objInstruction.IsError)
						{ // Comprueba si lo siguiente es una instrucción interna o si ya se ha terminado
								if (IsNextInstructionInternal(objInstruction, objNextToken))
									{ if (objInstruction.IsInner)
											objInstruction.Error = "Se ha detectado una instrucción dentro de una etiqueta HTML";
										else
											objNextToken = ReadInstructions(objInstruction.Instructions);
									}
								else if (IsNextInstruction(objNextToken))
									{ if (objInstruction.IsInner)
											objInstruction.Error = "Se ha detectado una instrucción dentro de una etiqueta HTML";
										else
											blnEnd = true;
									}
								else
									{ // Lee el token
											objToken = GetToken();
										// Trata el token
											switch (objToken.Type)
												{ case Token.TokenType.LeftTagHTMLInner:
															objInstruction.Instructions.Add(ReadHtml(objToken));
														break;
													case Token.TokenType.RightTagHTMLInner:
															if (objInstruction.IsInner)
																blnEnd = true;
															else
																objInstruction.Instructions.Add(new InstructionLiteral(objToken));
														break;
													case Token.TokenType.Variable:
															objInstruction.Instructions.Add(ReadVariableIdentifier(objToken));
														break;
													default:
															objInstruction.Instructions.Add(new InstructionLiteral(objToken));
														break;
												}
										// Lee el siguiente token sin sacarlo del buffer
											objNextToken = GetToken(true);
									}
						}
				// Devuelve la instrucción
					return objInstruction;
		}

		/// <summary>
		///		Lee una instrucción de código
		/// </summary>
		private InstructionBase ReadCommand(Token objToken)
		{ InstructionBase objInstruction = null;

				// Lee el siguiente token
					objToken = GetToken();
				// Interpreta la sentencia
					if (objToken.Type != Token.TokenType.Sentence)
						objInstruction = GetInstructionError(objToken, "Sentencia desconocida");
					else
						objInstruction = ReadSentence(objToken);
				// Devuelve la instrucción
					return objInstruction;
		}

		/// <summary>
		///		Lee una sentencia
		/// </summary>
		private InstructionBase ReadSentence(Token objToken)
		{ switch ((objToken.Content ?? "").ToUpper())
				{ case "CODE":
					case "<%CODE%>":
						return ReadCommandCode(objToken);
					case "END":
						return GetInstructionError(objToken, "Sentencia 'end' sin inicio de bloque");
					case "ELSE":
						return GetInstructionError(objToken, "Sentencia 'else' sin 'if'");
					case "IF":
						return ReadCommandIf(objToken);
					case "LET":
						return ReadCommandLet(objToken);
					case "FOREACH":
						return ReadCommandForEach(objToken);
					case "FOR":
						return ReadCommandFor(objToken);
					case "WHILE":
						return ReadCommandWhile(objToken);
					default:
						return GetInstructionError(objToken, "Sentencia desconocida");
				}
		}

		/// <summary>
		///		Lee una sentencia if
		/// </summary>
		private InstructionIf ReadCommandIf(Token objToken)
		{ InstructionIf objInstruction = new InstructionIf(objToken);
			string strError;

				// Lee la expresión
					objInstruction.Condition = ReadExpression(ExpressionReaderMode.Normal, out strError);
					objInstruction.ConditionRPN = objExpressionEvaluator.ConvertToRPN(objInstruction.Condition);
					if (!string.IsNullOrWhiteSpace(strError))
						objInstruction.Error = strError;
				// Lee el resto de datos
					if (!objInstruction.IsError)
						{ Token objNextToken = GetToken(true);

								// Comprueba si es un error
									if (objNextToken.Type != Token.TokenType.EndSentenceBlock && objNextToken.Type != Token.TokenType.Sentence)
										objInstruction.Error = "No se ha encontrado el final de la instrucción";
									else if (!IsEndCommand(objNextToken)) // ... no es un if vacío
										{ bool blnElse = false;

												// Lee las instrucciones de la parte
													objNextToken = ReadInstructions(objInstruction.Instructions);
												// Comprueba si la siguiente es una sentencia else
													if (IsElseCommand(objNextToken))
														{ // Indica que es un else
																blnElse = true;
															// Quita los tokens del else
																if (objNextToken.Type == Token.TokenType.StartSentenceBlock)
																	{ GetToken();
																		objNextToken = GetToken(true);
																	}
																if (objNextToken.Type == Token.TokenType.Sentence && objNextToken.Content.Equals("else", StringComparison.CurrentCultureIgnoreCase))
																	{ GetToken();
																		objNextToken = GetToken(true);
																	}
																if (objNextToken.Type == Token.TokenType.EndSentenceBlock)
																	{ GetToken();
																		objNextToken = GetToken(true);
																	}
														}
												// Lee la parte else
													if (blnElse)
														ReadInstructions(objInstruction.InstructionsElse);
										}
						}
				// Devuelve la instrucción
					return objInstruction;
		}

		/// <summary>
		///		Lee una sentencia while
		/// </summary>
		private InstructionWhile ReadCommandWhile(Token objToken)
		{ InstructionWhile objInstruction = new InstructionWhile(objToken);
			string strError;

				// Lee la expresión
					objInstruction.Condition = ReadExpression(ExpressionReaderMode.Normal, out strError);
					objInstruction.ConditionRPN = objExpressionEvaluator.ConvertToRPN(objInstruction.Condition);
					if (!string.IsNullOrWhiteSpace(strError))
						objInstruction.Error = strError;
				// Lee las instrucciones del bucle
					if (!objInstruction.IsError)
						ReadInstructions(objInstruction.Instructions);
				// Devuelve la instrucción
					return objInstruction;
		}

		/// <summary>
		///		Elimina los tokens de fin de comando (<%end%>)
		/// </summary>
		private void RemoveTokensEndCommand()
		{ Token objToken = GetToken();

				if (objToken.Type == Token.TokenType.StartSentenceBlock)
					RemoveTokens(2);
				else if (objToken.Type == Token.TokenType.Sentence &&
								 objToken.Content.Equals("end", StringComparison.CurrentCultureIgnoreCase))
					{ // Quita la sentencia end
							objToken = GetToken(true);
						// Quita el final de bloque
							if (objToken.Type == Token.TokenType.EndSentenceBlock)
								GetToken();
					}
		}

		/// <summary>
		///		Elimina una serie de tokens
		/// </summary>
		private void RemoveTokens(int intCount)
		{ for (int intIndex = 0; intIndex < intCount; intIndex++)
				GetToken();
		}

		/// <summary>
		///		Lee una instrucción de un identificador de variable
		/// </summary>
		private InstructionVariableIdentifier ReadVariableIdentifier(Token objToken)
		{ InstructionVariableIdentifier objInstruction = new InstructionVariableIdentifier(objToken);
			string strError;

				// Lee la expresión
					objInstruction.Variable = ReadVariableIdentifier(objToken, out strError);
					if (!string.IsNullOrWhiteSpace(strError))
						objInstruction.Error = strError;
				// Devuelve la instrucción
					return objInstruction;
		}

		/// <summary>
		///		Lee un identificador de variable
		/// </summary>
		private ExpressionVariableIdentifier ReadVariableIdentifier(Token objToken, out string strError)
		{ ExpressionVariableIdentifier objExpression = new ExpressionVariableIdentifier(objToken);
			Token objNextToken = GetToken(true);

				// Inicializa los argumentos de salida
					strError = "";
				// Asigna el nombre de la variable
					objExpression.Name = objToken.Content;
				// Comprueba si hay un corchete, es decir, es una variable de array
					if (objNextToken.Type == Token.TokenType.LeftCorchete)
						{ // Quita el token
								GetToken();
							// Asigna la expresión
								objExpression.IndexExpressions = ReadExpression(ExpressionReaderMode.AtVariable, out strError);
								objExpression.IndexExpressionsRPN = objExpressionEvaluator.ConvertToRPN(objExpression.IndexExpressions);
							// Si no ha habido ningún error ...
								if (string.IsNullOrWhiteSpace(strError))
									{ // Obtiene el siguiente token
											objNextToken = GetToken(true);
										// Si no es un corchete, añade un error, si es un corchete, quita el token
											if (objNextToken.Type != Token.TokenType.RightCorchete)
												strError = "Falta el corchete final en la definición de variable";
											else
												{ // Quita el token
														GetToken();
													// ... y deja leído el siguiente para comprobar si es un miembro de variable
														objNextToken = GetToken(true);
												}
									}
						}
				// Comprueba si hay un signo ->, es decir, tenemos una variable miembro
					if (objNextToken.Type == Token.TokenType.VariablePointer)
						{ // Quita el token
								GetToken();
							// Obtiene el siguiente token sin sacarlo del buffer
								objNextToken = GetToken(true);
							// Si es un literal, lo considera una variable (para los casos en $a->b en lugar de $a->$b)
								if (objNextToken.Type == Token.TokenType.Literal)
									{ objNextToken.Content = "$" + objNextToken.Content;
										objNextToken.Type = Token.TokenType.Variable;
									}
							// Comprueba que sea una variable
								if (objNextToken.Type == Token.TokenType.Variable) // ... si es así, obtiene el identificador del miembro
									objExpression.Member = ReadVariableIdentifier(GetToken(), out strError);
								else // ... si no es así, indica el error
									strError = "Falta el identificador de la variable miembro";
						}
				// Devuelve la expresión
					return objExpression;
		}

		/// <summary>
		///		Lee las expresiones
		/// </summary>
		private ExpressionsCollection ReadExpression(ExpressionReaderMode intMode, out string strError)
		{ ExpressionsCollection objColExpressions = new ExpressionsCollection();
			Token objNextToken = GetToken(true);

				// Inicializa los valores de salida
					strError = "";
				// Lee las expresiones
					while (!IsEof(objNextToken) && objNextToken.IsExpressionPart && string.IsNullOrWhiteSpace(strError))
						{ // Añade el token a la colección de expresiones
								if (objNextToken.Type == Token.TokenType.Variable)
									objColExpressions.Add(ReadVariableIdentifier(GetToken(), out strError));
								else
									objColExpressions.Add(new ExpressionBase(GetToken()));
							// Obtiene el siguiente token
								objNextToken = GetToken(true);
						}
				// Comprueba si ha habido algún error
					switch (intMode)
						{ case ExpressionReaderMode.Normal:
									if (objNextToken.Type != Token.TokenType.EndSentenceBlock &&
											objNextToken.Type != Token.TokenType.RightLlave && objNextToken.Type != Token.TokenType.Sentence)
										strError = "Falta el final de sentencia en la expresión";
								break;
							case ExpressionReaderMode.AtVariable:
									if (objNextToken.Type != Token.TokenType.RightCorchete)
									  strError = "Falta el corchete final en la definición de variable";
								break;
							case ExpressionReaderMode.AtAttributes:
									if (objNextToken.Type != Token.TokenType.RightLlave && objNextToken.Type != Token.TokenType.Literal)
										strError = "Falta el final de sentencia en la definición de atributos";
								break;
						}
				// Devuelve la colección de expresiones
					return objColExpressions;
		}

		/// <summary>
		///		Lee una sentencia Let
		/// </summary>
		private InstructionLet ReadCommandLet(Token objToken)
		{ InstructionLet objInstruction = new InstructionLet(objToken);
			Token objNextToken = GetToken(true);
			string strError;

				// Lee la variable
					if (objNextToken.Type == Token.TokenType.Variable)
						{ // Asigna la variable
								objInstruction.Variable = ReadVariableIdentifier(GetToken(), out strError);
							// Comprueba si hay algún error antes de continuar
								if (!string.IsNullOrWhiteSpace(strError))
									objInstruction.Error = strError;
								else
									{ // Signo igual
											objNextToken = GetToken(true);
											if (objNextToken.Type == Token.TokenType.Equal)
												{ // Quita el igual
														GetToken();
													// Lee las expresiones
														objInstruction.Expressions = ReadExpression(ExpressionReaderMode.Normal, out strError);
														objInstruction.ExpressionsRPN = objExpressionEvaluator.ConvertToRPN(objInstruction.Expressions);
														if (!string.IsNullOrEmpty(strError))
															objInstruction.Error = strError;
												}
											else
												objInstruction.Error = "No se encuentra el signo igual";
									}
						}
					else
						objInstruction.Error = "Debe existir una variable en la parte izquierda";
				// Devuelve la instrucción
					return objInstruction;
		}

		/// <summary>
		///		Lee una sentencia Code
		/// </summary>
		private InstructionCode ReadCommandCode(Token objToken)
		{ InstructionCode objInstruction = new InstructionCode(objToken);
			Token objNextToken = GetToken(true);

				// Comprueba si es un final de sentencia
					if (objNextToken.Type != Token.TokenType.EndSentenceBlock && !objToken.Content.EndsWith("%>"))
						objInstruction.Error = "Falta el marcador de final de sentencia '%>' en la instrucción 'code'";
					else
						{ // Quita el token de fin de bloque y lee el siguiente
								if (!objToken.Content.EndsWith("%>"))
									GetToken();
								objNextToken = GetToken();
							// Si es un literal, se añade al contenido
								if (objNextToken.Type == Token.TokenType.Literal)
									{ // Guarda el contenido
											objInstruction.Content = objNextToken.Content;
										// Lee el siguiente token
											objNextToken = GetToken(true);
										// Comprueba que sea el final de sentencia
											if (IsEndCommand(objNextToken))
												RemoveTokensEndCommand();
											else
												AddError(objNextToken, "No se ha encontrado el final de la sentencia <%code%>");
									}
								else
									AddError(objNextToken, "No se ha encontrado el contenido de la sentencia <%code%>");
						}
				// Devuelve la instrucción
					return objInstruction;
		}

		/// <summary>
		///		Lee una sentencia for
		/// </summary>
		private InstructionBase ReadCommandFor(Token objToken)
		{ InstructionFor objInstruction = new InstructionFor(objToken);
			Token objNextToken = GetToken(true);
			string strError;

				// Lee la variable
					if (objNextToken.Type == Token.TokenType.Variable)
						{ // Asigna la variable
								objInstruction.IndexVariable = ReadVariableIdentifier(GetToken(), out strError);
							// Comprueba si hay algún error antes de continuar
								if (!string.IsNullOrWhiteSpace(strError))
									objInstruction.Error = strError;
								else
									{ // Signo igual
											objNextToken = GetToken(true);
											if (objNextToken.Type != Token.TokenType.Equal)
												objInstruction.Error = "Falta el signo igual en el bucle for";
											else
												{ // Quita el igual
														GetToken();
													// Lee el valor inicial
														objInstruction.StartValue = ReadExpression(ExpressionReaderMode.Normal, out strError);
														objInstruction.StartValueRPN = objExpressionEvaluator.ConvertToRPN(objInstruction.StartValue);
														if (!string.IsNullOrEmpty(strError))
															objInstruction.Error = strError;
														else
															{ // Sentencia to
																	objNextToken = GetToken(true);
																	if (objNextToken.Type != Token.TokenType.Sentence ||
																			!objNextToken.Content.Equals("to", StringComparison.CurrentCultureIgnoreCase))
																		objInstruction.Error = "Falta la sentencia to en el bucle for";
																	else
																		{ // Quita el to
																				objNextToken = GetToken();
																			// Lee el valor final
																				objInstruction.EndValue = ReadExpression(ExpressionReaderMode.Normal, out strError);
																				objInstruction.EndValueRPN = objExpressionEvaluator.ConvertToRPN(objInstruction.EndValue);
																				if (!string.IsNullOrEmpty(strError))
																					objInstruction.Error = strError;
																				else
																					{ // Sentencia step
																							objNextToken = GetToken(true);
																						// Obtiene el valor del incremento (si es necesario)
																							if (objNextToken.Type == Token.TokenType.Sentence &&
																									objNextToken.Content.Equals("step", StringComparison.CurrentCultureIgnoreCase))
																								{ // Quita el step
																										GetToken();
																									// Lee el valor del step
																										objInstruction.StepValue = ReadExpression(ExpressionReaderMode.Normal, out strError);
																										objInstruction.StepValueRPN = objExpressionEvaluator.ConvertToRPN(objInstruction.StepValue);
																									// Comprueba el error y lee las instrucciones
																										if (!string.IsNullOrEmpty(strError))
																											objInstruction.Error = strError;
																								}
																						// Lee las instrucciones del for
																							if (!objInstruction.IsError)
																								objToken = ReadInstructions(objInstruction.Instructions);
																					}
																		}
															}
												}
									}
						}
					else
						objInstruction.Error = "Debe existir una variable después del for";
				// Devuelve la instrucción
					return objInstruction;
		}

		/// <summary>
		///		Lee una sentencia foreach
		/// </summary>
		private InstructionForEach ReadCommandForEach(Token objToken)
		{ InstructionForEach objInstruction = new InstructionForEach(objToken);
			Token objNextToken = GetToken(true);
			string strError;

				// Lee los datos de la instrucción
					if (objNextToken.Type == Token.TokenType.Variable)
						{ // Lee la variable
								objInstruction.IndexVariable = ReadVariableIdentifier(objNextToken, out strError);
							// Comprueba los errores
								if (!string.IsNullOrEmpty(strError))
									objInstruction.Error = "Error al leer la variable índice: " + strError;
								else
									{ // Lee la parte in
											objNextToken = GetToken(true);
											if (objNextToken.Type == Token.TokenType.Literal && (objNextToken.Content ?? "").Equals("in", StringComparison.CurrentCultureIgnoreCase))
												{ // Quita el in de la cadena de tokens
														GetToken();
													// Lee el siguiente token
														objNextToken = GetToken(true);
													// Comprueba si es la variable de lista
														if (objNextToken.Type == Token.TokenType.Variable)
															{ // Guarda la variable de lista
																	objInstruction.ListVariable = ReadVariableIdentifier(objNextToken, out strError);
																// Comprueba los errores antes de continuar
																	if (!string.IsNullOrEmpty(strError))
																		objInstruction.Error = "Error al leer la variable del bucle: " + strError;
																	else
																		ReadInstructions(objInstruction.Instructions);
															}
														else
															objInstruction.Error = "Falta la variable de lista";
												}
											else
												objInstruction.Error = "Falta la sentencia in";
									}
						}
					else
						objInstruction.Error = "No se ha definido la variable índice";
				// Devuelve la instrucción
					return objInstruction;
		}

		/// <summary>
		///		Lee los atributos de una instrucción
		/// </summary>
		private void ReadAttributes(InstructionNhaml objInstruction)
		{ Token objToken = GetToken();
			bool blnError = false; // ... supone que hay algún error

				// Lee el primer token (el anterior sólo ha quitado la llave)
					objToken = GetToken();
				// Lee los atributos
					while (!IsEof(objToken) && objToken.Type != Token.TokenType.RightLlave && !blnError)
						{ Parameter objAttribute = new Parameter();

								// Obtiene el atributo
									if (objToken.Type == Token.TokenType.Literal)
										{ // Nombre del atributo
												objAttribute.Name = objToken.Content;
											// El nombre de un atributo puede ser del tipo http-equiv, es decir, puede tener un guión intermedio,
											// aquí obtiene el resto
												objToken = GetToken(true);
												if (objToken.Type == Token.TokenType.Literal && objToken.Content.StartsWith("-"))
													{ // Quita el token
															objToken = GetToken();
														// Añade el guión al nombre del atributo
															objAttribute.Name += objToken.Content;
													}
											// Signo igual
												objToken = GetToken();
												if (objToken.Type == Token.TokenType.Equal)
													{ // Literal
															objToken = GetToken(true);
														// Comprueba y lo añade
															if (objToken.IsExpressionPart) // objToken.Type == Token.TokenType.String || objToken.Type == Token.TokenType.Variable)
																{	string strError;

																		// Obtiene las expresiones de la variable
																			objAttribute.Variable = ReadExpression(ExpressionReaderMode.AtAttributes, out strError);
																			objAttribute.VariableRPN = objExpressionEvaluator.ConvertToRPN(objAttribute.Variable);
																		// Indica si hay un error
																			if (!strError.IsEmpty())
																				{ blnError = true;
																					objInstruction.Error = strError;
																				}
																}
															else
																blnError = false;
													}
										}
									else
										blnError = true;
								// Si no hay ningún error, añade el atributo y lee el siguiente token
									if (!blnError)
										{ // Añade el atributo
												objInstruction.Attributes.Add(objAttribute);
											// Lee el siguiente token
												objToken = GetToken();
										}
						}
				// Si hay algún error, lo añade
					if (blnError)
						objInstruction.Error = "Error en la definición de parámetros";
		}

		/// <summary>
		///		Comprueba si un token es una instrucción diferente
		/// </summary>
		private bool IsNextInstruction(Token objNextToken)
		{ return objNextToken.Type == Token.TokenType.StartComment || objNextToken.Type == Token.TokenType.StartSentenceBlock ||
						 objNextToken.Type == Token.TokenType.Sentence || objNextToken.Type == Token.TokenType.TagHTML;
		}

		/// <summary>
		///		Comprueba si un token es una instrucción diferente que debe estar dentro de esta instrucción
		/// </summary>
		private bool IsNextInstructionInternal(InstructionBase objInstruction, Token objNextToken)
		{ return IsNextInstruction(objNextToken) && objNextToken.Indent > objInstruction.Token.Indent;
		}

		/// <summary>
		///		Comprueba si un token es un fin de comando
		/// </summary>
		private bool IsEndCommand(Token objNextToken)
		{ return (objNextToken.Type == Token.TokenType.StartSentenceBlock &&
								GetNextTokensString(3).Equals("<%end%>", StringComparison.CurrentCultureIgnoreCase)) ||
						 (objNextToken.Type == Token.TokenType.Sentence &&
								objNextToken.Content.Equals("end", StringComparison.CurrentCultureIgnoreCase));
		}

		/// <summary>
		///		Comprueba si un token es un comando else
		/// </summary>
		private bool IsElseCommand(Token objToken)
		{ return (objToken.Type == Token.TokenType.StartSentenceBlock && 
								GetNextTokensString(3).Equals("<%else%>", StringComparison.CurrentCultureIgnoreCase)) ||
						 (objToken.Type == Token.TokenType.Sentence &&
								objToken.Content.Equals("else", StringComparison.CurrentCultureIgnoreCase));
		}

		/// <summary>
		///		Obtiene el siguiente token
		/// </summary>
		private Token GetToken(bool blnIsSimulated = false)
		{ if (Tokens == null || IndexToken > Tokens.Count - 1)
				return new Token { Type = Token.TokenType.EOF };
			else if (blnIsSimulated)
				return Tokens[IndexToken];
			else
				return Tokens[IndexToken++];
		}

		/// <summary>
		///		Obtiene una cadena con los siguientes tokens
		/// </summary>
		private string GetNextTokensString(int intCount)
		{ string strContent = "";

				// Obtiene las cadenas de los siguientes tokens
					for (int intIndex = 0; intIndex < intCount; intIndex++)
						if (IndexToken + intIndex < Tokens.Count)
							strContent += Tokens[IndexToken + intIndex].Content;
				// Devuelve el contenido
					return strContent;
		}

		/// <summary>
		///		Obtiene una instrucción de error
		/// </summary>
		private InstructionBase GetInstructionError(Token objToken, string strError)
		{ InstructionBase objInstruction = new InstructionBase(objToken);

				// Indica que es un error
					objInstruction.Error = "Tipo de instrucción desconocida";
				// Devuelve la instrucción
					return objInstruction;
		}

		/// <summary>
		///		Añade un error
		/// </summary>
		private void AddError(Token objToken, string strMessage)
		{ Compiler.LocalErrors.Add(objToken, strMessage);
		}

		/// <summary>
		///		Se recupera del error (busca el primer token de Html)
		/// </summary>
		private void RecoverError()
		{ Token objToken = GetToken(true);

				while (!IsEof(objToken) && objToken.Type != Token.TokenType.TagHTML)
					objToken = GetToken();
		}

		/// <summary>
		///		Lanza el evento de depuración de los tokens
		/// </summary>
		private void DebugTokens()
		{ string strResult = "";

				// Obtiene los tokens de la cadena
					foreach (Token objToken in Tokens)
						strResult += Environment.NewLine + objToken.ToString();
				// Lanza el evento de depuración
					Compiler.RaiseEventDebug(EventArgs.DebugEventArgs.Mode.Tokenizer, "Tokens", strResult);
		}

		/// <summary>
		///		Lanza el evento de depuración de las instrucciones
		/// </summary>
		private void DebugInstructions()
		{ Compiler.RaiseEventDebug(EventArgs.DebugEventArgs.Mode.Instructions, "Instructions", Instructions.GetDebugString());
		}

		/// <summary>
		///		Comprueba si es el final del archivo
		/// </summary>
		private bool IsEof(Token objToken)
		{ return objToken.Type == Token.TokenType.EOF;
		}

		/// <summary>
		///		Compilador
		/// </summary>
		private Compiler Compiler { get; set; }

		/// <summary>
		///		Tokens
		/// </summary>
		internal TokensCollection Tokens { get; private set; }

		/// <summary>
		///		Instrucciones
		/// </summary>
		internal InstructionsBaseCollection Instructions { get; private set; }

		/// <summary>
		///		Indice del token actual
		/// </summary>
		private int IndexToken { get; set; }
	}
}
