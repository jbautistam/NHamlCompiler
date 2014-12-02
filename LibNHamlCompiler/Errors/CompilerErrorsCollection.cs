using System;
using System.Collections.Generic;

namespace Bau.Libraries.NhamlCompiler.Errors
{
	/// <summary>
	///		Colección de <see cref="CompilerError"/>
	/// </summary>
	public class CompilerErrorsCollection : List<CompilerError>
	{
		/// <summary>
		///		Añade un error a partir de un token
		/// </summary>
		internal void Add(Parser.Tokens.Token objToken, string strError)
		{ CompilerError objError = new CompilerError();

				// Asigna las propiedades
					objError.Token = objToken.Content;
					objError.Row = objToken.Row;
					objError.Column = objToken.Column;
					objError.Description = strError;
				// Añade el error
					Add(objError);
		}
	}
}
