using System;

using Bau.Libraries.NhamlCompiler.Parser.Tokens;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Instrucción con los datos de un literal
	/// </summary>
	internal class InstructionLiteral : InstructionBase
	{
		internal InstructionLiteral(Token objToken) : base(objToken) {}
	}
}
