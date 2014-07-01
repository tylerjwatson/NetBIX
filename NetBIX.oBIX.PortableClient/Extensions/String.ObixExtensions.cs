using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NetBIX.oBIX.Client.Extensions {
    public static class StringObixExtensions {

        /// <summary>
        /// Gets the oBIX XML object equivalent for this string. Returns a str
        /// element with @val set to the string provided by input.
        /// </summary>
		public static XElement ObixXmlValue(this string input) {
			XElement element = new XElement("str");
			element.SetAttributeValue("val", input);
			return element;
		}

		/// <summary>
		/// Gets the oBIX XML object equivalent for this string. Returns a str
		/// element with @val set to the string provided by input.
		/// </summary>
		public static XElement ObixXmlValue(this string input, string name, string href) {
			XElement element = input.ObixXmlValue();

			if (string.IsNullOrEmpty(name) == false) {
				element.SetAttributeValue("name", name);
			}

			if (string.IsNullOrEmpty(href) == false) {
				element.SetAttributeValue("href", href);
			}

			return element;
		}


        /// <summary>
        /// Gets the string value from this oBIX str element.
        /// </summary>
        /// <returns>
        /// The string equivalent of this oBIX str element, or null if the provided element
        /// is null, or not a str.
        /// </returns>
        public static string ObixStringValue(this XElement element) {
            XAttribute valAttr;

            if (element.Name.LocalName != "str" || element.HasAttributes == false || (valAttr = element.Attribute("val")) == null) {
                return null;
            }

            return valAttr.Value;
        }
         
    }
}
