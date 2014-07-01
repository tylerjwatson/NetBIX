using System;
using System.Xml.Linq;
using NetBIX.oBIX.Client.Extensions;

namespace NetBIX.oBIX.Client {
	public static class DateTimeObixExtensions {
		public static XElement ObixXmlValue(this DateTime value) {
			XElement element = new XElement("abstime");
			element.SetAttributeValue("val", value.ToString());
			return element;
		}

		public static DateTime? ObixAbstimeValue(this XElement element) {
			XAttribute valAttr;
			DateTime val;

			if (element.Name.LocalName != "abstime" || element.HasAttributes == false || (valAttr = element.Attribute("val")) == null) {
				return null;
			}

			if (DateTime.TryParse(element.ObixValue(), out val) == false) {
				return null;
			}

			return val;
		}
	}
}

