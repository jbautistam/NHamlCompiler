using System;

using Bau.Libraries.NhamlCompiler.Parser.Tokens;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Clase con los datos de una expresión
	/// </summary>
	internal class ExpressionBase
	{
		internal ExpressionBase(Token objToken)
		{ Token = objToken;
		}

		/// <summary>
		///		Información de depuración
		/// </summary>
		internal virtual string GetDebugInfo()
		{ return Token.Content;
		}

		/// <summary>
		///		Token asociado a la expresión
		/// </summary>
		internal Token Token { get; private set; }

		/// <summary>
		///		Prioridad de la expresión
		/// </summary>
		public int Priority
		{ get
				{ if (Token.Content == "*" || Token.Content == "/" || Token.Content == "%")
						return 20;
					else if (Token.Content == "+" || Token.Content == "-")
						return 19;
					else if (Token.Content == "<" || Token.Content == ">" || Token.Content == ">=" || Token.Content == "<=")
						return 18;
					else if (Token.Content == "==" || Token.Content == "!=")
						return 17;
					else if (Token.Content == "&&")
						return 16;
					else if (Token.Content == "||")
						return 15;
					else if (Token.Content == "=")
						return 14;
					else
						return 0;
				}
		}

		/// <summary>
		///		Clona una expresión
		/// </summary>
		internal virtual ExpressionBase Clone()
		{ return new ExpressionBase(Token);
		}
	}
}
