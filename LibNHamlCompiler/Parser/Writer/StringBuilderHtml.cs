using System;
using System.Text;

using Bau.Libraries.LibHelper.Extensors;

namespace Bau.Libraries.NhamlCompiler.Parser.Writer
{
	/// <summary>
	///		Clase para generación de HTML
	/// </summary>
	internal class StringBuilderHtml
	{ // Variables privadas
			private string [] arrStrTagsAutoClose = { "img", "br", "hr", "meta", "link" };
			private string [] arrStrTagsNoWrite = { "NoTag" };

		internal StringBuilderHtml(bool blnIsCompressed = false)
		{ Builder = new StringBuilder();
			IsCompressed = blnIsCompressed;
			Indent = 0;
		}

		/// <summary>
		///		Añade un texto
		/// </summary>
		internal void Add(string strText)
		{ Builder.Append(strText);
		}

		/// <summary>
		///		Añade una etiqueta
		/// </summary>
		internal void AddTag(string strText, bool blnEnd, bool blnIsInner)
		{ bool blnIsAutoEnd; 
		
		    // Normaliza la etiqueta
		      strText = (strText ?? "").Trim();
		    // Comprueba si es una etiqueta de autocierre
		      blnIsAutoEnd = CheckIsAutoEnd(strText);
		    // Añade la indentación
		      if (!blnIsInner && (!blnEnd || (blnEnd && !blnIsAutoEnd)))
		        AddIndent();
		      else if (blnIsInner && !blnEnd)
		        Add(" ");
		    // Añade la etiqueta de apertura o cierre
					if (!CheckIsTagNoWritable(strText))
						{ if (blnEnd)
								{ if (!blnIsAutoEnd)
										Add("</" + strText + "> ");
								}
							else 
								Add("<" + strText + ">");
						}
		}

		/// <summary>
		///		Añade la indentación
		/// </summary>
		internal void AddIndent(bool blnFixed = false)
		{ if (!IsCompressed || blnFixed)
				{ // Añade el salto de línea
						Add(Environment.NewLine);
					// Añade la tabulación
						for (int intIndex = 0; intIndex < Indent; intIndex++)
							Add("\t");
				}
		}

		/// <summary>
		///		Comprueba si es una etiqueta de autocierre
		/// </summary>
		private bool CheckIsAutoEnd(string strText)
		{ // Comprueba si es una etiqueta autocerrada
				foreach(string strTag in arrStrTagsAutoClose)
					if (strText.Equals(strTag, StringComparison.CurrentCultureIgnoreCase) ||
							strText.StartsWith(strTag + " ", StringComparison.CurrentCultureIgnoreCase))
						return true;
			// Si ha llegado hasta aquí es porque no es una etiqueta autocerrada
				return false;
		}

		/// <summary>
		///		Comprueba si es una etiqueta que no se debe imprimir
		/// </summary>
		private bool CheckIsTagNoWritable(string strText)
		{ // Comprueba si está entre las etiquetas no imprimibles
				foreach (string strTag in arrStrTagsNoWrite)
					if (strText.EqualsIgnoreCase(strTag))
						return true;
			// Si ha llegado hasta aquí es porque no es una etiqueta que o se debe imprimir
				return false;
		}

		/// <summary>
		///		Cadena generada
		/// </summary>
		internal StringBuilder Builder { get; private set; }

		/// <summary>
		///		Indica si la generación de datos se comprime
		/// </summary>
		internal bool IsCompressed { get; private set; }

		/// <summary>
		///		Indentación
		/// </summary>
		internal int Indent { get; set; }
	}
}
