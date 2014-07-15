using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NetBIX.oBIX.Client.Extensions {
    public static class BooleanObixExtensions {

        /// <summary>
        /// Returns an oBIX XML equivalent bool element.
        /// </summary>
        /// <returns></returns>
        public static XElement ObixXmlValue(this bool boolean) {
            XElement element = new XElement("bool");
            element.SetAttributeValue("val", boolean ? "true" : "false");
            return element;
        }

		public static XElement ObixXmlValue(this bool input, string name, string href)
		{
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
        /// Returns a nullable boolean from its oBIX bool element representation.
        /// </summary>
        /// <returns>
        /// True or false equivalent of the providded oBIX XML element, or null if the provided
        /// element is null, or not an oBIX bool.</returns>
        public static bool? ObixBoolValue(this XElement element) {
            XAttribute valAttr;
            
            if (element.Name.LocalName != "bool" || element.HasAttributes == false || (valAttr = element.Attribute("val")) == null) {
                return null;
            }

            return valAttr.Value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
        }

    }
}
