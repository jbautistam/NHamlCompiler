using System;

namespace Bau.Libraries.NhamlCompiler.Variables
{
	/// <summary>
	///		Valor lógico
	/// </summary>
	public class ValueBool : ValueBase
	{ // Constantes internas
			public const string cnstStrTrue = "#true#";
			public const string cnstStrFalse = "#false#";

		public ValueBool(bool blnValue)
		{ Value = blnValue;
		}

		/// <summary>
		///		Ejecuta una operación aritmética / lógica
		/// </summary>
		public override ValueBase Execute(ValueBase objValue, string strOperator)
		{ ValueBase objNewValue = null;

				// Ejecuta la operación
					if (objValue is ValueBool)
						{ ValueBool objSecond = objValue as ValueBool;

								switch (strOperator)
									{ case "==":
												objNewValue = new ValueBool(Value == objSecond.Value);
											break;
										case "!=":
												objNewValue = new ValueBool(Value != objSecond.Value);
											break;
										case "&&":
												objNewValue = new ValueBool(Value && objSecond.Value);
											break;
										case "||":
												objNewValue = new ValueBool(Value || objSecond.Value);
											break;
									}
						}
				// Crea el error
					if (objNewValue == null)
						objNewValue = ValueBase.GetError(string.Format("No se puede utilizar el operador '{0}' entre los valores {1} y {2}", strOperator, Content, objValue.Content));
				// Devuelve el valor
					return objNewValue;
		}

		/// <summary>
		///		Valor
		/// </summary>
		public bool Value { get; set; }

		/// <summary>
		///		Contenido en formato cadena
		/// </summary>
		public override string Content
		{ get 
				{ if (Value)
						return cnstStrTrue;
					else
						return cnstStrFalse; 
				}
		}
	}
}
