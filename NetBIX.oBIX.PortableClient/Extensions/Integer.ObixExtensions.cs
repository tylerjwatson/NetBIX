using System;
using System.Xml.Linq;
using NetBIX.oBIX.Client.Extensions;

namespace NetBIX.oBIX.Client.Extensions {
	public static class IntegerObixExtensions {
		/// <summary>
		/// Returns an oBIX XML equivalent int element.
		/// </summary>
		public static XElement ObixXmlValue(this int number) {
			XElement element = new XElement("int");
			element.SetAttributeValue("val", number.ToString());
			return element;
		}

		public static XElement ObixXmlValue(this int number, string name, string href) {
			XElement element = ObixXmlValue(number);

			if (string.IsNullOrEmpty(href) == false) {
				element.SetAttributeValue("href", href);
			}

			if (string.IsNullOrEmpty(name) == false) {
				element.SetAttributeValue("name", name);
			}

			return element;
		}

		/// <summary>
		/// Returns a nullable integer from its oBIX int element representation.
		/// </summary>
		/// <returns>
		/// The integer value of this oBIX int value, null if there was an error.
		/// </returns>
		public static int? ObixIntValue(this XElement element) {
			XAttribute valAttr;
			int value;

			if (element.Name.LocalName != "int" || element.HasAttributes == false || (valAttr = element.Attribute("val")) == null) {
				return null;
			}

			if (int.TryParse(valAttr.Value, out value) == false) {
				return null;
			}

			return value;
		}
	}
}

