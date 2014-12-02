using System;

using Bau.Libraries.LibHelper.Extensors;

namespace Bau.Libraries.NhamlCompiler.Parser.Lexical
{
	/// <summary>
	///		Clase para obtener los caracteres de una cadena
	/// </summary>
	internal class StringWord
	{
		/// <summary>
		///		Modo de interpretación
		/// </summary>
		internal enum ParseMode
			{ 
				/// <summary>Desconocido. No se debería utilizar</summary>
				Unknown,
				/// <summary>Nhaml</summary>
				Nhaml,
				/// <summary>Sentencias de código</summary>
				Code,
				/// <summary>Sentencias de bloque de código</summary>
				CodeBlock,
				/// <summary>Comentario</summary>
				Comment,
				/// <summary>Expresión en líneas de código Nhaml</summary>
				ExpressionNhaml,
				/// <summary>Expresión en líneas de código</summary>
				ExpressionCode
			}

		/// <summary>
		///		Estructura con los datos de una palabra
		/// </summary>
		internal struct WordStruct
			{ internal int Row, Column, Indent;
				internal bool IsEof;
				internal string Content;
			}

		internal StringWord(string strSource)
		{ Source = strSource;
			Mode = ParseMode.Nhaml;
			Row = 1;
			Column = 1;
			IndexActualChar = 0;
			PreviousChar = ' ';
			PreviousWord = new WordStruct();
		}

		/// <summary>
		///		Obtiene la siguiente palabra
		/// </summary>
		internal WordStruct GetNextWord()
		{	WordStruct objWord = new WordStruct();

				// Obtiene la siguiente cadena (o null si ha terminado con el archivo)
					if (IsEof())
						objWord.IsEof = true;
					else
						{ // Salta los espacios
								SkipSpaces();
							// Asigna la fila y columna (cuando se han saltado los espacios)
								objWord.Row = Row;
								objWord.Column = Column;
								objWord.Indent = Indent;
							// Dependiendo del modo ...
								switch (Mode)
									{ case ParseMode.Nhaml:
												objWord.Content = GetNextStringNhaml();
											break;
										case ParseMode.Comment:
												objWord.Content = GetNextStringComment();
											break;
										case ParseMode.ExpressionCode:
										case ParseMode.ExpressionNhaml:
												objWord.Content = GetNextStringExpression();
											break;
										case ParseMode.Code:
												objWord.Content = GetNextStringCode();
											break;
										case ParseMode.CodeBlock:
												objWord.Content = GetNextStringBlockCode();
											break;
									}
						}
				// Guarda la palabra anterior
					PreviousWord = objWord;
				// Devuelve la palabra
					return objWord;
		}

		/// <summary>
		///		Obtiene la siguiente cadena para código Nhaml
		/// </summary>
		private string GetNextStringNhaml()
		{ char chrChar = GetChar();
			string strNhaml = chrChar.ToString();

				// Dependiendo del carácter inicial, lee el resto de los caracteres
					if (chrChar == '\"')
						strNhaml += ReadToEndString();
					else if (chrChar == '#' || chrChar == '%' || chrChar == '·' || chrChar == '&')
						strNhaml += ReadToEndNHaml();
					else if (chrChar == '$')
						strNhaml += ReadToEndVariable();
					else if (chrChar == '[')
						Mode = ParseMode.ExpressionNhaml;
					else if (chrChar == '-' && GetFirstChars(1, false) == ">")
						{ strNhaml = "->";
							SkipChars(1);
						}
					else if (chrChar == '<' && GetFirstChars(3, false) == "!--")
						{ // Crea la cadena Nhaml
								strNhaml = "<!--";
								SkipChars(3); // ... no 4 porque el primero ya está
							// Cambia el modo de lectura
								Mode = ParseMode.Comment;
						}
					else if (chrChar == '<' && GetFirstChars(7, false).EqualsIgnoreCase("%code%>"))
						{ // Crea la cadena NHaml
								strNhaml = "<%code%>";
								SkipChars(7);
							// Cambia el modo de lectura
								Mode = ParseMode.CodeBlock;
						}
					else if (chrChar == '<' && GetFirstChars(1, false) == "%")
						{ // Crea la cadena Nhaml
								strNhaml = "<%";
								SkipChars(1); // ... no 2 porque el primero ya no está en el buffer
							//// Lee la sentencia
							//  strNhaml += ReadToEndStartSentence();
							// Cambia el modo de lectura
							//  if (!strNhaml.EndsWith("%>"))
							  Mode = ParseMode.Code;
						}
					else if (!IsEndCharNHaml(chrChar.ToString())) // ... si no está entre los caracteres finales
						strNhaml += ReadToEndWord();
				// Devuelve la cadena Nhaml
					return strNhaml;
		}

		/// <summary>
		///		Obtiene la sigiente cadena para código
		/// </summary>
		private string GetNextStringCode()
		{ char chrChar = GetChar();
			string strResult = chrChar.ToString();
			char chrNextChar = GetNextChar();

				// Dependiendo del carácter inicial, lee el resto de los caracteres
					if (chrChar == '\"')
						strResult += ReadToEndString();
					else if (chrChar == '$')
						strResult += ReadToEndVariable();
					else if (chrChar == '[')
						Mode = ParseMode.ExpressionCode;
					else if (chrChar == '-' && GetFirstChars(1, false) == ">")
						{ strResult = "->";
							SkipChars(1);
						}
					else if (chrChar == '%' && chrNextChar == '>')
						{ // Crea el resultado
								strResult = "%>";
								SkipChars(1); // ... no 2 porque el primero ya no está en el buffer
							// Cambia el modo de lectura
								Mode = ParseMode.Nhaml;
						}
					else if (IsDigit(chrChar.ToString()))
						strResult += ReadToEndNumber();
					else if ((IsLogicOperator(chrChar) && chrNextChar == '=') || // <= ó >=
									 (chrChar == '=' && IsLogicOperator(chrNextChar)) || // => ó <=
									 (chrChar == '=' && chrNextChar == '=') || // ==
									 (chrChar == '!' && chrNextChar == '=') || // !=
									 (chrChar == '&' && chrNextChar == '&') || // &&
									 (chrChar == '|' && chrNextChar == '|') // ||
									)
						{ strResult += chrNextChar.ToString();
							SkipChars(1);
						}
					else if (!IsArithmeticOperator(chrChar) && !IsLogicOperator(chrChar))
						strResult += ReadToEndWord();
				// Devuelve la cadena de código
					return strResult;
		}

		/// <summary>
		///		Obtiene la siguiente cadena de comentario
		/// </summary>
		private string GetNextStringComment()
		{ string strResult = "";

				// Busca la cadena final para el comentario
					if (GetFirstChars(3, false) == "-->")
						{ // Asigna la cadena final y las quita del buffer
								strResult = "-->";
								SkipChars(3);
							// Cambia el modo de lectura
								Mode = ParseMode.Nhaml;
						}
					else // ... obtiene el contenido del comentario
						while (!IsEof() && GetFirstChars(3, false) != "-->")
							strResult += GetChar();
				// Devuelve el comentrario
					return strResult;
		}

		/// <summary>
		///		Obtiene una cadena con un bloque de código
		/// </summary>
		private string GetNextStringBlockCode()
		{ string strResult = "";

				// Obtiene la cadena hasta el final del bloque de código
					while (!IsEof() && !GetFirstChars(7, false).EqualsIgnoreCase("<%end%>"))
						strResult += GetChar();
				// Cambia el modo de lectura a código (para que se lea correctamente el end
					Mode = ParseMode.Nhaml;
				// Devuelve el comentrario
					return strResult;
		}

		/// <summary>
		///		Obtiene la siguiente cadena de una expresión
		/// </summary>
		private string GetNextStringExpression()
		{ char chrChar = GetChar();
			char chrNextChar = GetNextChar();
			string strResult = chrChar.ToString();

				// Obtiene la siguiente cadena de la expresión
					if (chrChar == ']') // ... fin de expresión, cambia el modo
						{ if (Mode == ParseMode.ExpressionCode)
								Mode = ParseMode.Code;
							else 
								Mode = ParseMode.Nhaml;
						}
					else if (chrChar == '[') // ... inicio de índice
						strResult = "["; // ... no es necesario, se añade por claridad
					else if (IsDigit(chrChar.ToString()))
						strResult += ReadToEndNumber();
					else if ((IsLogicOperator(chrChar) && chrNextChar == '=') || // <= ó >=
									 (chrChar == '=' && IsLogicOperator(chrNextChar)) || // => ó <=
									 (chrChar == '=' && chrNextChar == '=') || // ==
									 (chrChar == '&' && chrNextChar == '&') || // &&
									 (chrChar == '|' && chrNextChar == '|') // ||
									)
						{ strResult += chrNextChar.ToString();
							SkipChars(1);
						}
					else if (!IsArithmeticOperator(chrChar) && !IsLogicOperator(chrChar))
						strResult += ReadToEndWord();
				// Devuelve la cadena de la expresión
					return strResult;
		}

		/// <summary>
		///		Lee hasta el final de la palabra (letras y dígitos)
		/// </summary>
		private string ReadToEndWord()
		{ string strResult = "";
			string strNextChar = GetFirstChars(1, false);

				// Busca el carácter final para la palabra (letras y dígitos)
					while (!IsEof() && (IsAlphabetic(strNextChar) || IsDigit(strNextChar)))
						{ // Añade el carácter al resultado
								strResult += GetCharSkipBars(false).ToString();
							// Obtiene el siguiente carácter por adelantado
								strNextChar = GetFirstChars(1, false);
						}
				// Devuelve la cadena
					return strResult;
		}

		/// <summary>
		///		Lee hasta el final de la variable (letras, dígitos y cadena ->)
		/// </summary>
		private string ReadToEndVariable()
		{ string strResult = "";
			string strNextChar = GetFirstChars(1, false);

				// Busca el carácter final para la variable (letras y dígitos y los caracteres _)
					while (!IsEof() && (IsAlphabetic(strNextChar) || IsDigit(strNextChar) || strNextChar == "_"))
															// (strNextChar == "-" && GetFirstChars(2, false) == "->")))
						{ // Añade el carácter al resultado
								strResult += GetCharSkipBars(false).ToString();
							// Añade el > si estamos en un -
								if (strNextChar == "-")
									strResult += GetCharSkipBars(false).ToString();
							// Obtiene el siguiente carácter por adelantado
								strNextChar = GetFirstChars(1, false);
						}
				// Devuelve la cadena
					return strResult;
		}

		/// <summary>
		///		Busca un carácter final para Nhaml (espacio o carácter de fin de Nhaml)
		/// </summary>
		private string ReadToEndNHaml()
		{ string strResult = "";
			string strNextChar = GetFirstChars(1, false);

				// Busca el carácter final para Nhaml
					while (!IsEof() && (IsAlphabetic(strNextChar) || IsDigit(strNextChar) || strNextChar == "-" || strNextChar == "_"))
						{ // Añade el carácter al resultado
								strResult += GetCharSkipBars(false).ToString();
							// Obtiene el siguiente carácter por adelantado
								strNextChar = GetFirstChars(1, false);
						}
				// Devuelve la cadena
					return strResult;
		}

		/// <summary>
		///		Lee hasta el final de la cadena
		/// </summary>
		private string ReadToEndString()
		{ string strResult = "";
			string strNextChar = GetFirstChars(1, true);

				// Busca el carácter final para la cadena
					while (!IsEof() && strNextChar != "\"")
						{ // Añade el carácter al resultado
								strResult += GetChar();
							// Obtiene el siguiente carácter por adelantado
								strNextChar = GetFirstChars(1, false);
						}
				// Añade las comillas finales
					if (strNextChar == "\"")
						strResult += GetChar();
				// Devuelve la cadena
					return strResult;
		}

		/// <summary>
		///		Lee hasta el final del número
		/// </summary>
		private string ReadToEndNumber()
		{ string strResult = "";
			string strNextChar = GetFirstChars(1, true);

				// Busca el carácter final para la cadena
					while (!IsEof() && (IsDigit(strNextChar) || strNextChar == "."))
						{ // Añade el carácter al resultado
								strResult += GetChar();
							// Obtiene el siguiente carácter por adelantado
								strNextChar = GetFirstChars(1, false);
						}
				// Devuelve la cadena
					return strResult;
		}

		/// <summary>
		///		Salta los espacios
		/// </summary>
		private void SkipSpaces()
		{ if (!IsEof())
				{ string strNextChars = GetFirstChars(1, false);

						// Se salta los espacios
							while (!IsEof() && IsSpace(strNextChars))
								{ char chrChar = GetChar();

										// Incrementa el número de tabuladores
											if (chrChar == '\t')
												Indent++;
										// Obtiene el siguiente carácter (sin sacarlo del buffer)
											strNextChars = GetFirstChars(1, false);
								}
				}
		}

		/// <summary>
		///		Se salta una serie de caracteres
		/// </summary>
		private void SkipChars(int intLength)
		{ for (int intIndex = 0; intIndex < intLength; intIndex++)
				GetChar();
		}

		/// <summary>
		///		Obtiene el carácter actual
		/// </summary>
		private char GetChar()
		{ char chrChar = ' ';

				// Obtiene el siguiente carácter (si existe)
					if (!IsEof())
						{ // Obtiene el carácter
								chrChar = Source[IndexActualChar];
							// Incrementa el índice actual
								IndexActualChar++;
							// Incrementa filas y columnas
								if (chrChar == '\r' || (chrChar == '\n' && PreviousChar != '\r'))
									{ Row++;
										Column = 1;
										Indent = 0;
									}
								else
									Column++;
							// Guarda el carácter en los caracteres previos
								PreviousChar = chrChar;
						}
				// Devuelve el carácter
					return chrChar;
		}

		/// <summary>
		///		Obtiene un carácter saltándose las barras invertidas
		/// </summary>
		private char GetCharSkipBars(bool blnAtString)
		{ char chrChar = GetChar();

				// Se salta las barras invertidas
					if (chrChar == '\\' && !blnAtString)
						chrChar = GetChar();
				// Devuelve el carácter
					return chrChar;
		}

		/// <summary>
		///		Obtiene los primeros n caracteres (sin sacarlos del buffer)
		/// </summary>
		private string GetFirstChars(int intLength, bool blnAtString)
		{ string strNextChars = "";
			int intStartPosition = IndexActualChar;

				// Obtiene los siguientes carácter (si existe)
					for (int intIndex = 0; intIndex < intLength; intIndex++)
						if (intStartPosition + intIndex < Source.Length)
							{ // Se salta la barra actual
									if (!blnAtString && Source[intStartPosition + intIndex] == '\\')
										intStartPosition++;
								// Obtiene el carácter
									strNextChars += Source[intStartPosition + intIndex];
							}
				// Devuelve la cadena de siguientes caracteres
					return strNextChars;
		}

		/// <summary>
		///		Obtiene el siguiente carácter sin sacarlo del buffer
		/// </summary>
		private char GetNextChar()
		{ string strNextChars = GetFirstChars(1, false);

				// Obtiene el primer carácter
				if (!string.IsNullOrWhiteSpace(strNextChars))
					return strNextChars[0];
				else
					return ' ';
		}

		/// <summary>
		///		Indica si es final de archivo
		/// </summary>
		private bool IsEof()
		{ return IndexActualChar >= Source.Length;
		}

		/// <summary>
		///		Comprueba si un carácter es un espacio
		/// </summary>
		private bool IsSpace(string strChar)
		{ return strChar == " " || strChar == "\t" || strChar == "\r" || strChar == "\n";
		}

		/// <summary>
		///		Comprueba si una cadena es un carácter final en Nhaml
		/// </summary>
		private bool IsEndCharNHaml(string strNextChar)
		{ return strNextChar == "{" || strNextChar == "}" || strNextChar == "#" || strNextChar == "=" || strNextChar == "[" || strNextChar == "]";
		}

		/// <summary>
		///		Comprueba si es un dígito
		/// </summary>
		private bool IsDigit(string strNextChar)
		{ return !string.IsNullOrWhiteSpace(strNextChar) && strNextChar.Length > 0 && char.IsDigit(strNextChar[0]);
		}

		/// <summary>
		///		Comprueba si es un carácter alfabético
		/// </summary>
		private bool IsAlphabetic(string strNextChar)
		{ return !string.IsNullOrWhiteSpace(strNextChar) && strNextChar.Length > 0 && char.IsLetter(strNextChar[0]);
		}

		/// <summary>
		///		Comprueba si es un operador lógico
		/// </summary>
		private bool IsLogicOperator(char chrChar)
		{ return chrChar == '<' || chrChar == '>';
		}

		/// <summary>
		///		Comprueba si es un operador aritmético
		/// </summary>
		private bool IsArithmeticOperator(char chrChar)
		{ return chrChar == '+' || chrChar == '-' || chrChar == '/' || chrChar == '*' || chrChar == '\\' || chrChar == '(' || chrChar == ')';
		}

		/// <summary>
		///		Texto original
		/// </summary>
		internal string Source { get; private set; }

		/// <summary>
		///		Fila actual
		/// </summary>
		internal int Row { get; private set; }

		/// <summary>
		///		Columna actual
		/// </summary>
		internal int Column { get; private set; }

		/// <summary>
		///		Carácter anterior
		/// </summary>
		internal char PreviousChar { get; private set; }

		/// <summary>
		///		Palabra anterior
		/// </summary>
		internal WordStruct PreviousWord { get; private set; }

		/// <summary>
		///		Carácter actual
		/// </summary>
		internal int IndexActualChar { get; private set; }

		/// <summary>
		///		Indentación
		/// </summary>
		internal int Indent { get; private set; }

		/// <summary>
		///		Modo de interpretación
		/// </summary>
		internal ParseMode Mode { get; set; }

		/// <summary>
		///		Indica si el modo de interpretación es uno de los de código (código o expresión)
		/// </summary>
		internal bool IsCodeMode
		{ get { return Mode == ParseMode.Code || Mode == ParseMode.ExpressionCode || Mode == ParseMode.ExpressionNhaml; }
		}
	}
}
