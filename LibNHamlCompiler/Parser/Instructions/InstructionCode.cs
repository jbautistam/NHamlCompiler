using System;

using Bau.Libraries.NhamlCompiler.Parser.Tokens;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Instrucción de bloque de código
	/// </summary>
	internal class InstructionCode : InstructionBase
	{
		internal InstructionCode(Token objToken) : base(objToken) {}

		/// <summary>
		///		Contenido del bloque de código
		/// </summary>
		internal string Content { get; set; }
	}
}
