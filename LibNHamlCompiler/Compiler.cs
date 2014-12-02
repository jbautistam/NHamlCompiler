using System;

namespace Bau.Libraries.NhamlCompiler
{
	/// <summary>
	///		Compilador de Nhaml
	/// </summary>
	public class Compiler
	{ // Eventos públicos
			public event EventHandler<EventArgs.DebugEventArgs> Debug;

		public Compiler()
		{ MaximumRepetitionsLoop = 500;
			LocalErrors = new Errors.CompilerErrorsCollection();
		}

		/// <summary>
		///		Interpreta una cadena
		/// </summary>
		public string Parse(string strSource, int intMaxInstructions = 0, bool blnIsCompressed = false)
		{ return Parse(strSource, new Variables.VariablesCollection(), intMaxInstructions, blnIsCompressed);
		}

		/// <summary>
		///		Interpreta una cadena
		/// </summary>
		public string Parse(string strSource, Variables.VariablesCollection objColVariables, int intMaxInstructions = 0, bool blnIsCompressed = false)
		{ Parser.Translator.Interpreter objInterpreter = new Parser.Translator.Interpreter(this, objColVariables, intMaxInstructions, blnIsCompressed);

				// Interpreta y ejecuta
					return objInterpreter.Parse(strSource);
		}

		/// <summary>
		///		Lanza el evento de depuración
		/// </summary>
		internal void RaiseEventDebug(EventArgs.DebugEventArgs.Mode intMode, string strTitle, string strMessage)
		{ if (Debug != null)
				Debug(this, new EventArgs.DebugEventArgs(intMode, strTitle, strMessage));
		}

		/// <summary>
		///		Errores
		/// </summary>
		public Errors.CompilerErrorsCollection LocalErrors { get; private set; }

		/// <summary>
		///		Número máximo de veces que se puede ejecutar un bucle
		/// </summary>
		public int MaximumRepetitionsLoop { get; set; }
	}
}
