using System;

using Bau.Libraries.NhamlCompiler.Parser.Tokens;

namespace Bau.Libraries.NhamlCompiler.Parser.Instructions
{
	/// <summary>
	///		Comentario
	/// </summary>
	internal class InstructionComment : InstructionBase
	{
		internal InstructionComment(Token objToken) : base(objToken) {}

		/// <summary>
		///		Obtiene la información de depuración
		/// </summary>
		protected override string GetDebugInfo(int intIndent)
		{ return Environment.NewLine + GetIndent(intIndent + 1) + "--> Contenido: " + Content;
		}

		/// <summary>
		///		Comentario
		/// </summary>
		internal string Content { get; set; }
	}
}
