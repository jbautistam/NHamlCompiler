using System;

using Bau.Libraries.NhamlCompiler.Parser.Tokens;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Instrucción de código Nhaml
	/// </summary>
	internal class InstructionNhaml : InstructionBase
	{
		internal InstructionNhaml(Token objToken) : base(objToken) 
		{ Attributes = new ParametersCollection();
		}

		/// <summary>
		///		Obtiene la información de depuración
		/// </summary>
		protected override string GetDebugInfo(int intIndent)
		{ string strDebug = "";

				// Añade información de los atributos
					if (Attributes.Count > 0)
						{ // Cabecera
								strDebug = Environment.NewLine + GetIndent(intIndent + 1) + "---> Parámetros: ";
							// Parámetros
								foreach (Parameter objParameter in Attributes)
									strDebug += objParameter.Name + " = " + objParameter.Variable.GetDebugInfo();
						}
				// Devuelve la cadena informativa
					return strDebug;
		}

		/// <summary>
		///		Indica si es una subinstrucción de HTML
		/// </summary>
		internal bool IsInner
		{ get { return Token.Content.StartsWith("#"); }
		}

		/// <summary>
		///		Atributos
		/// </summary>
		internal ParametersCollection Attributes { get; private set; }
	}
}
