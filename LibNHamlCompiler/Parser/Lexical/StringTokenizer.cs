using System;

using Bau.Libraries.LibHelper.Extensors;
using Bau.Libraries.NhamlCompiler.Parser.Tokens;

namespace Bau.Libraries.NhamlCompiler.Parser.Lexical
{
	/// <summary>
	///		Conversor de una cadena en tokens
	/// </summary>
	internal class StringTokenizer
	{ // Constantes privadas
			private string[] cnstArrStrSentences = { "code", "<%code%>", "else", "end", "for", "to", "step", "foreach", "if", "let", "else", "while" };

		internal StringTokenizer(string strSource)
		{ Wordizer = new StringWord(strSource);
		}

		/// <summary>
		///		Obtiene todos los tokens de la cadena
		/// </summary>
		internal TokensCollection GetAllTokens()
		{ TokensCollection objColTokens = new TokensCollection();

				// Obtiene todos los tokens
					do
						{ objColTokens.Add(GetNextToken());
						}
					while (objColTokens[objColTokens.Count - 1].Type != Token.TokenType.EOF);
				// Devuelve la colección de tokens
					return objColTokens;
		}

		/// <summary>
		///		Obtiene el siguiente token
		/// </summary>
		internal Token GetNextToken()
		{ Token objToken = new Token();
			StringWord.WordStruct objWord = Wordizer.GetNextWord();

				// Pasa los datos de la cadena al token
					objToken.Content = objWord.Content;
					if (!string.IsNullOrEmpty(objToken.Content))
						objToken.Content = objToken.Content.Trim();
					objToken.Row = objWord.Row;
					objToken.Column = objWord.Column;
					objToken.Indent = objWord.Indent;
				// Obtiene el tipo de token
					if (objWord.IsEof)
						objToken.Type = Token.TokenType.EOF;
					else
						objToken.Type = GetType(objToken.Content);
				// Devuelve el token
					return objToken;
		}

		/// <summary>
		///		Obtiene el tipo de contenido
		/// </summary>
		private Token.TokenType GetType(string strContent)
		{ if (strContent.StartsWith("\""))
				return Token.TokenType.String;
			else if (strContent == "<!--")
				return Token.TokenType.StartComment;
			else if (strContent == "-->")
				return Token.TokenType.EndComment;
			else if (strContent == "[")
				return Token.TokenType.LeftCorchete;
			else if (strContent == "]")
				return Token.TokenType.RightCorchete;
			else if (strContent == "->")
				return Token.TokenType.VariablePointer;
			else if (Wordizer.IsCodeMode && strContent == "&&" || strContent == "||")
				return Token.TokenType.RelationalOperator;
			else if (strContent.StartsWith("$"))
				return Token.TokenType.Variable;
			else if (strContent.EqualsIgnoreCase("<%code%>"))
				return Token.TokenType.Sentence;
			else if (strContent.StartsWith("<%"))
				return Token.TokenType.StartSentenceBlock;
			else if (strContent == "%>")
				return Token.TokenType.EndSentenceBlock;
			else if (strContent == "{")
				return Token.TokenType.LeftLlave;
			else if (strContent == "}")
				return Token.TokenType.RightLlave;
			else if (Wordizer.IsCodeMode && strContent == "(")
				return Token.TokenType.LeftParentesis;
			else if (Wordizer.IsCodeMode && strContent == ")")
				return Token.TokenType.RightParentesis;
			else if (strContent == "#")
				return Token.TokenType.RightTagHTMLInner;
			else if (strContent == "=")
				return Token.TokenType.Equal;
			else if (Wordizer.IsCodeMode && (strContent == "+" || strContent == "-" || strContent == "*" || 
																			 strContent == "/" || strContent == "\\"))
				return Token.TokenType.ArithmeticOperator;
			else if (Wordizer.IsCodeMode && (strContent == ">" || strContent == "<" || strContent == ">=" || 
																			 strContent == "<=" || strContent == "=>" || strContent == "=<" || 
																			 strContent == "==" || strContent == "!="))
				return Token.TokenType.LogicalOperator;
			else if (Wordizer.IsCodeMode && strContent != "" && IsNumeric(strContent))
				return Token.TokenType.Number;
			else if (strContent.StartsWith("#") && strContent.Length > 1)
				return Token.TokenType.LeftTagHTMLInner;
			else if (strContent.StartsWith("%") || strContent.StartsWith("&") || strContent.StartsWith("·"))
				return Token.TokenType.TagHTML;
			else if (Wordizer.IsCodeMode && IsSentence(strContent))
				return Token.TokenType.Sentence;
			else
				return Token.TokenType.Literal;
		}

		/// <summary>
		///		Comprueba si una palabra es una sentencia válida
		/// </summary>
		private bool IsSentence(string strContent)
		{ // Comprueba si una palabra es una sentencia
				if (!string.IsNullOrWhiteSpace(strContent))
					foreach (string strSentence in cnstArrStrSentences)
						if (strSentence.Equals(strContent, StringComparison.CurrentCultureIgnoreCase))
							return true;
			// Si ha llegado hasta aquí es porque no es una sentencia
				return false;
		}

		/// <summary>
		///		Comprueba si es un número
		/// </summary>
		private bool IsNumeric(string strContent)
		{ bool blnExistDot = false;

				// Comprueba todos los caracteres o si la cadena es sólo un punto
					if (strContent == ".") 
						return false;
					else
						foreach (char chrChar in strContent)
							if (!char.IsDigit(chrChar) && chrChar != '.')
								return false;
							else if (chrChar == '.')
								{ if (blnExistDot)
										return false;
									else
										blnExistDot = true;
								}
				// Si ha llegado hasta aquí es porqe es numérico
					return !string.IsNullOrWhiteSpace(strContent);
		}

		/// <summary>
		///		Objeto para interpretación de palabras
		/// </summary>
		private StringWord Wordizer { get; set; }
	}
}
