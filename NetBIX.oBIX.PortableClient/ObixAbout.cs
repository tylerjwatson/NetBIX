using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using NetBIX.oBIX.Client.Framework;
using NetBIX.oBIX.Client.Extensions;

namespace NetBIX.oBIX.Client {

    public class ObixAbout {
        public string Version { get; set; }
        public string ServerName { get; set; }
        public string ServerTime { get; set; }
        public string ServerBootTime { get; set; }
        public string VendorName { get; set; }
        public string VendorUrl { get; set; }
        public string ProductName { get; set; }
        public string ProductVersion { get; set; }
        public string ProductUrl { get; set; }

        public override string ToString() {
            return String.Format("Obix Server v{0}: Name={1} Vendor={2} ({3}) Product={4} ({5})", this.Version, this.ServerName, this.VendorName, this.VendorUrl, this.ProductName, this.ProductUrl);
        }

        /// <summary>
        /// Converts a provided obix:About XElement contract into an ObixAbout instance.
        /// </summary>
        /// <param name="parentElement">An instance an oBIX:About contract</param>
        /// <returns>An ObixAbout object if conversion succeeded, null if an error was encountered.</returns>
        public static ObixAbout FromXElement(XElement parentElement) {
            ObixAbout about = null;
            string name = null;
            string val = null;
            string version = null;
            string serverName = null;
            string serverTime = null;
            string serverBootTime = null;
            string vendorName = null;
            string vendorUrl = null;
            string productName = null;
            string productVersion = null;
            string productUrl = null;

            if (parentElement == null 
                || parentElement.Name.LocalName != "obj" 
                || parentElement.Attribute("is") == null 
                || string.IsNullOrEmpty(parentElement.Attribute("is").Value) == true 
                || parentElement.Attribute("is").Value != "obix:About") {
                return null;
            }

            foreach (XElement child in parentElement.Elements()) {
                if (child.IsNullOrNullContract() || child.HasObixValue() == false 
                    || string.IsNullOrEmpty(child.ObixName()) == true) {
                    continue;
                }

                name = child.ObixName();
                val = child.ObixValue();

                switch (name) {
                    case "obixVersion":
                        version = val;
                        break;
                    case "serverName":
                        serverName = val;
                        break;
                    case "serverTime":
                        serverTime = val;
                        break;
                    case "serverBootTime":
                        serverBootTime = val;
                        break;
                    case "vendorName":
                        vendorName = val;
                        break;
                    case "vendorUrl":
                        vendorUrl = val;
                        break;
                    case "productName":
                        productName = val;
                        break;
                    case "productVersion":
                        productVersion = val;
                        break;
                    case "productUrl":
                        productUrl = val;
                        break;
                    default: break;
                }
            }

            about = new ObixAbout() {
                ProductName = productName,
                ProductUrl = productUrl,
                ProductVersion = productVersion,
                ServerBootTime = serverBootTime,
                ServerName = serverName,
                ServerTime = serverTime,
                VendorName = vendorName,
                VendorUrl = vendorUrl,
                Version = version
            };

            return about;
        }
    }
}
