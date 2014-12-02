using System;
using System.Collections.Generic;
using System.Linq;

using Bau.Libraries.LibHelper.Extensors;

namespace Bau.Libraries.NhamlCompiler.Variables
{
	/// <summary>
	///		Colección de <see cref="Variable"/>
	/// </summary>
	public class VariablesCollection : List<Variable>
	{
		/// <summary>
		///		Añade una variable
		/// </summary>
		public void Add(string strName, string strValue, int intIndex = 0)
		{ Variable objVariable = new Variable(strName, ValueBase.GetInstance(strValue), intIndex);
			int intIndexFound = IndexOf(strName, intIndex);
				
				if (intIndexFound >= 0)
					this[intIndexFound] = objVariable;
				else
					Add(strName, ValueBase.GetInstance(strValue), intIndex);
		}

		/// <summary>
		///		Añade una variable
		/// </summary>
		public void Add(string strName, ValueBase objValue, int intIndex = 0)
		{ Add(new Variable(strName, objValue, intIndex));
		}

		/// <summary>
		///		Obtiene el índice de una variable
		/// </summary>
		public int IndexOf(string strName, int intIndex = 0)
		{ // Normaliza el nombre
				strName = Variable.Normalize(strName);
			// Recorre la colección
				for (int intIndexCollection = 0; intIndexCollection < Count; intIndexCollection++)
					if (this[intIndexCollection].Name.EqualsIgnoreCase(strName) && this[intIndexCollection].Index == intIndex)
						return intIndexCollection;
			// Si ha llegado hasta aquí es porque no ha encontrado nada
				return -1;
		}

		/// <summary>
		///		Busca una variable
		/// </summary>
		public Variable Search(string strName, int intIndex = 0)
		{ Variable objVariable = null;

				// Normaliza el nombre
					strName = Variable.Normalize(strName);
				// Obtiene el primer elemento
					objVariable = this.FirstOrDefault<Variable>(objSearchVariable => objSearchVariable.Name.EqualsIgnoreCase(strName) &&
																																					 objSearchVariable.Index == intIndex);
				// Si no se ha encontrado ninguna variable, la crea
					if (objVariable == null)
						{ // Crea la variable
								objVariable = new Variable(strName, ValueBase.GetInstance("null"), intIndex);
							// ... y la añade a la colección
								Add(objVariable);
						}
				// Devuelve la variable
					return objVariable;
		}

		/// <summary>
		///		Clona una colección de variables
		/// </summary>
		internal VariablesCollection Clone()
		{ VariablesCollection objColVariables = new VariablesCollection();

				// Clona la colección
					foreach (Variable objVariable in this)
						objColVariables.Add(objVariable.Clone());
				// Devuelve la colección
					return objColVariables;
		}

		/// <summary>
		///		Ordena las variables por su índice
		/// </summary>
		internal void SortByIndex()
		{ Sort((objFirst, objSecond) => objFirst.Index.CompareTo(objSecond.Index));
		}
	}
}
