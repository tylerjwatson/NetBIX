using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;

namespace NetBIX.oBIX.Client.Extensions {
    /// <summary>
    /// Provides System.Xml.Linq extensions for working with oBIX contracts straight from the X-interface.
    /// </summary>
    public static class XElementObixExtensions {

        /// <summary>
        /// Returns true if this XElement is null, or an obix:Null contract.
        /// </summary>
        /// <returns>true if this XElement is null or a null contract, false otherwise.</returns>
        public static bool IsNullOrNullContract(this XElement element) {
            if (element == null) {
                return true;
            }

            return element.HasAttributes 
                && element.Attribute("null") != null 
                && element.Attribute("null").Value == "true";
        }

        /// <summary>
        /// Returns true if this XElement is an oBIX:Nil contract.
        /// </summary>
        /// <returns>true if this XElement is an oBIX:Nil contract, false otherwise or if there was an error.</returns>
        public static bool IsObixNullContract(this XElement element) {
            return element.HasAttributes
                && element.Attribute("null") != null
                && element.Attribute("null").Value == "true";
        }

        /// <summary>
        /// Retrieves the oBIX Value specified by attrName.
        /// </summary>
        /// <param name="attrName">The name of the oBIX attribute</param>
        /// <returns>The oBIX attribute value, or null if an error occured.</returns>
        public static string ObixAttrValue(this XElement element, string attrName) {
            XAttribute valAttr = null;

            if (IsNullOrNullContract(element)) {
                return null;
            }

            valAttr = element.Attribute(attrName);
            if (valAttr == null) {
                return null;
            }

            return valAttr.Value;
        }

        /// <summary>
        /// Gets the oBIX Name of this XElement
        /// </summary>
        /// <returns>An oBIX name, or null if there has been an error.</returns>
        public static string ObixName(this XElement element) {
            return ObixAttrValue(element, "name");
        }

        /// <summary>
        /// Gets the oBIX href of this XElement
        /// </summary>
        /// <returns>An oBIX href, or null if an error occured.</returns>
        public static string ObixHref(this XElement element) {
            return ObixAttrValue(element, "href");
        }

        /// <summary>
        /// Gets the oBIX display name of this XElement
        /// </summary>
        /// <returns>an oBIX display name, or null if an error occured.</returns>
        public static string ObixDisplay(this XElement element) {
            return ObixAttrValue(element, "display");
        }


        /// <summary>
        /// Gets the oBIX display name of this XElement
        /// </summary>
        /// <returns>an oBIX display name, or null if an error occured.</returns>
        public static string ObixDisplayName(this XElement element) {
            return ObixAttrValue(element, "displayName");
        }

        /// <summary>
        /// Gets the oBIX value of this XElement.
        /// </summary>
        /// <returns>The value of the oBIX element, or null if the provided object is a null contract.</returns>
        public static string ObixValue(this XElement element) {
            return ObixAttrValue(element, "val");
        }

        /// <summary>
        /// Gets the name of the contract that this oBIX object conforms to, via the @is attribute.
        /// </summary>
        /// <returns>The name of the contract that this oBIX object conforms to, or null if this XElement is not an oBIX object or an error occured.</returns>
        public static string ObixIs(this XElement element) {
            return ObixAttrValue(element, "is");
        }

        public static string ObixIn(this XElement element) {
            return ObixAttrValue(element, "in");
        }

        public static string ObixOut(this XElement element) {
            return ObixAttrValue(element, "out");
        }

		/// <summary>
		/// Returns the immediate child element that matches the oBIX name provided by name.
		/// </summary>
		/// <returns>The child element if it exists and matches the name, or null if there was an error.</returns>
		public static XElement ObixChildElement(this XElement element, string name) {
			XElement childElement = null;
			if (element.HasElements == false) {
				return null;
			}

			if ((childElement = element.Elements().FirstOrDefault(i => i.ObixName() == name)) == null) {
				return null;
			}

			return childElement;
		}

        /// <summary>
        /// Returns if this XEelement has an oBIX value.
        /// </summary>
        /// <returns>Returns true if this XElement has an oBIX value, false if the oBIX val attribute doesn't exist, or the provided object is a null contract.</returns>
        public static bool HasObixValue(this XElement element) {
            return string.IsNullOrEmpty(ObixValue(element)) == false;
        }

        /// <summary>
        /// Returns a value indicating whether this XElement instance is an oBIX error contract.
        /// </summary>
        /// <returns>true if this XElement is an oBIX contract, false otherwise or if an error occured.</returns>
        public static bool IsObixErrorContract(this XElement element) {
            if (element == null) {
                return false;
            }

            return element.Name.LocalName == "err";
        }

        /// <summary>
        /// Concatenates the full oBIX href for the given XElement, all the way up to the root.
        /// </summary>
        /// <returns>The full href path of this oBIX XElement, or null if there was an error.</returns>
		public static string ObixFullHrefPath(this XElement element) {
			StringBuilder sb = new StringBuilder();
			XElement parentElement = null;
			string href = null;

			if (element == null) {
				return null;
			}

			href = element.ObixHref();
			if (string.IsNullOrEmpty(href) == true) {
				return null;
			}

			if (element.Parent == null) {
				return href;
			}

			parentElement = element.Parent;

			sb.Insert(0, href);

			do {
				href = parentElement.ObixHref();
				if (string.IsNullOrEmpty(href) == true) {
					continue;
				}

				if (href.EndsWith("/") == false) {
					href += '/';
				}

				sb.Insert(0, href);

				if (href.StartsWith("/") == true || href.Contains("://") == true) {
					break;
				}
			} while((parentElement = parentElement.Parent) != null);

			return sb.ToString();
		}

    }
}
