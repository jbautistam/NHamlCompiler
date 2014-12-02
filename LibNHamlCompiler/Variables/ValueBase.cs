using System;

namespace Bau.Libraries.NhamlCompiler.Variables
{
	/// <summary>
	///		Valor
	/// </summary>
	public abstract class ValueBase
	{ 
		/// <summary>
		///		Obtiene un valor predeterminado a partir del tipo de contenido
		/// </summary>
		public static ValueBase GetInstance(string strValue)
		{ double dblValue;

				if (string.IsNullOrWhiteSpace(strValue))
					return new ValueString("");
				else if (double.TryParse(strValue.Replace(',', '.'), out dblValue))
					return new ValueNumeric(dblValue);
				else if (strValue.Equals(ValueBool.cnstStrTrue, StringComparison.CurrentCultureIgnoreCase))
					return new ValueBool(true);
				else if (strValue.Equals(ValueBool.cnstStrFalse, StringComparison.CurrentCultureIgnoreCase))
					return new ValueBool(false);
				else
					return new ValueString(strValue);
		}

		/// <summary>
		///		Obtiene un valor predeterminado con un error
		/// </summary>
		public static ValueBase GetError(string strError)
		{ ValueString objValue = new ValueString("ERROR");

				// Asigna el error
					objValue.Error = strError;
				// Devuelve el valor
					return objValue;
		}

		/// <summary>
		///		Ejecuta una operación
		/// </summary>
		public abstract ValueBase Execute(ValueBase objValue, string strOperator);

		/// <summary>
		///		Contenido del valor (numérico, cadena ...)
		/// </summary>
		public abstract string Content { get; }

		/// <summary>
		///		Comprueba si hay algún error en el valor
		/// </summary>
		public bool HasError 
		{ get { return !string.IsNullOrEmpty(Error); }
		}

		/// <summary>
		///		Error encontrado en la última operación
		/// </summary>
		public string Error { get; private set; }
	}
}
