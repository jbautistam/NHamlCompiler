using System;

namespace Bau.Libraries.NhamlCompiler.Errors
{
	/// <summary>
	///		Clase con los datos de un error
	/// </summary>
	public class CompilerError
	{
		/// <summary>
		///		Token que originó el error
		/// </summary>
		public string Token { get; set; }

		/// <summary>
		///		Fila en la que se originó el error
		/// </summary>
		public int Row { get; set; }

		/// <summary>
		///		Columna en la que se originó el error
		/// </summary>
		public int Column { get; set; }

		/// <summary>
		///		Descripción del error
		/// </summary>
		public string Description { get; set; }
	}
}
