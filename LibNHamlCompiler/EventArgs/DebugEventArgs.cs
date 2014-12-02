using System;

namespace Bau.Libraries.NhamlCompiler.EventArgs
{
	/// <summary>
	///		Argumentos de los eventos de depuración
	/// </summary>
	public class DebugEventArgs : System.EventArgs
	{
		/// <summary>
		///		Modo desde el que se envían los eventos de depuración
		/// </summary>
		public enum Mode
		{
			Unknown,
			Tokenizer,
			Instructions
		}

		public DebugEventArgs(Mode intMode, string strTitle, string strMessage)
		{ DebugMode = intMode;
			Title = strTitle;
			Message = strMessage;
		}

		/// <summary>
		///		Modo
		/// </summary>
		public Mode DebugMode { get; private set; }

		/// <summary>
		///		Título para la depuración
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		///		Mensaje de depuración
		/// </summary>
		public string Message { get; private set; }
	}
}
