using System;

namespace Bau.Libraries.NhamlCompiler.Variables
{
	/// <summary>
	///		Clase base para las variables
	/// </summary>
	public class Variable
	{ // Variables privadas
			private string strName;

		public Variable(string strName, ValueBase objValue = null, int intIndex = 0, int intScope = 0)
		{ Name = strName;
			Value = objValue;
			Index = intIndex;
			Scope = intScope;
			Members = new VariablesCollection();
		}

		/// <summary>
		///		Normaliza el nombre de una variable
		/// </summary>
		internal static string Normalize(string strName)
		{ // Asigna el valor
				strName = (strName ?? "").Trim();
			// Normaliza el valor
				if (!strName.StartsWith("$"))
					return "$" + strName;
				else
					return strName;
		}

		/// <summary>
		///		Clona el contenido de una variable
		/// </summary>
		internal Variable Clone()
		{ Variable objVariable = new Variable(Name, Value, Index, Scope);

				// Clona los miembros
					objVariable.Members.AddRange(Members.Clone());
				// Devuelve la variable
					return objVariable;
		}

		/// <summary>
		///		Nombre de la variable
		/// </summary>
		public string	Name 
		{ get { return strName; }
			set { strName = Normalize(value); }
		}

		/// <summary>
		///		Valor de la variable
		/// </summary>
		public ValueBase Value { get; set; }

		/// <summary>
		///		Indice de la variable
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		///		Miembros de la variable
		/// </summary>
		public VariablesCollection Members { get; private set; }

		/// <summary>
		///		Nivel de ámbito
		/// </summary>
		public int Scope { get; set; }
	}
}
