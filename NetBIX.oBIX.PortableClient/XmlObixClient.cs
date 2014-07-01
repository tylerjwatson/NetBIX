using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Xml.Linq;
using System.Net.Http;
using NetBIX.oBIX.Client.Framework;
using NetBIX.oBIX.Client.Extensions;
using Flurl;
using System.Xml;
using System.Linq;

namespace NetBIX.oBIX.Client {
    /// <summary>
    /// Implementation of oBIX Client for oBIX XML over HTTP(s)
    /// </summary>
    public class XmlObixClient : Framework.ObixClient {
        protected Uri signUpUri = null;
        protected XmlBatchClient batchClient = null;

        /// <summary>
        /// Provides obix:Batch functionality for the XML oBIX client.
        /// </summary>
        public XmlBatchClient Batch {
            get {
                if (this.BatchUri == null) {
                    throw new NotSupportedException("The oBIX server does not support the oBIX:Batch mechanism.");
                }

                return this.batchClient;
            }
            protected set {
                this.batchClient = value;   
            }
        }


        /// <summary>
        /// Gets the URI of the batch endpoint on the oBIX server for this instance of the oBIX client.
        /// </summary>
        public Uri BatchUri { get; protected set; }

        /// <summary>
        /// Gets the URI of the obix:About endpoint on the oBIX server for this instance of the oBIX client.
        /// </summary>
        public Uri AboutUri { get; protected set; }

        /// <summary>
        /// Gets the URI of the obix:WatchIn endpoint on the oBIX server for this instance of the oBIX client.
        /// </summary>
        public Uri WatchUri { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this instance of the oBIX client is connected and ready to serve requests.
        /// </summary>
        public bool IsConnected {
            get {
                return connected;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetBIX.oBIX.Client.XmlObixClient"/> class for oBIX XML over HTTP.
        /// </summary>
        /// <param name="ObixLobbyUri">Obix lobby URI.</param>
        public XmlObixClient(Uri ObixLobbyUri)
            : base(ObixLobbyUri) {
            WebClient = new HttpClient();
            ErrorStack = new ObixErrorStack();

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetBIX.oBIX.Client.XmlObixClient"/> class for oBIX XML over HTTP, with the provided signUp Uri.
        /// </summary>
        /// <param name="ObixLobbyUri">An oBIX Lobby URI</param>
        /// <param name="RegisterUri">A relative path for an operation enabling devices to register themselves into the server.</param>
        public XmlObixClient(Uri ObixLobbyUri, string RegisterUri)
            : base(ObixLobbyUri) {
                WebClient = new HttpClient();
            this.signUpUri = new Uri(Url.Combine(ObixLobbyUri.ToString(), RegisterUri));
        }

        #region "oBIX Connecting"

        public override ObixResult Connect() {
            byte[] data = null;
            XDocument doc = null;
            MemoryStream ms = null;
            ObixResult result;
            HttpResponseMessage responseMessage = null;

            if (WebClient == null || connected == true) {
                return ObixResult.kObixClientInputError;
            }

            try {
                responseMessage = WebClient.GetAsync(base.LobbyUri).Result;
            } catch (Exception ex) {
                return ErrorStack.Push(this.GetType(), ex);
            }

            if (responseMessage.IsSuccessStatusCode == false) {
                return ErrorStack.Push(GetType(), ObixResult.kObixClientSocketError, responseMessage.ReasonPhrase);
            }

            try {
                data = responseMessage.Content.ReadAsByteArrayAsync().Result;
            } catch (Exception ex) {
                return ErrorStack.Push(GetType(), ex);
            }

            using (ms = new MemoryStream(data)) {
                try { 
                    doc = XDocument.Load(ms);
                } catch (Exception ex) {
                    return ErrorStack.Push(this.GetType(), ex);
                }
            }

            result = ParseLobbyContract(doc);
            if (result != ObixResult.kObixClientSuccess) {
                return ErrorStack.Push(this.GetType(), result);
            }

            result = ParseAboutContract();
            if (result != ObixResult.kObixClientSuccess) {
                return ErrorStack.Push(this.GetType(), result);
            }

            connected = true;
            return ObixResult.kObixClientSuccess;
        }

        public override async Task<ObixResult> ConnectAsync() {
            byte[] data = null;
            XDocument doc = null;
            MemoryStream ms = null;
            ObixResult result;
            HttpResponseMessage responseMessage = null;

            if (WebClient == null || connected == true) {
                return ObixResult.kObixClientInputError;
            }

            try {
                responseMessage = await WebClient.GetAsync(this.LobbyUri);
            } catch (Exception ex) {
                return ErrorStack.Push(this.GetType(), ex);
            }

            if (responseMessage.IsSuccessStatusCode == false) {
                return ErrorStack.Push(GetType(), ObixResult.kObixClientSocketError, responseMessage.ReasonPhrase);
            }

            try {
                data = await responseMessage.Content.ReadAsByteArrayAsync();
            } catch (Exception ex) {
                return ErrorStack.Push(GetType(), ex);
            }

            ms = new MemoryStream(data);
            try {
                doc = await Task.Factory.StartNew(() => XDocument.Load(ms));
            } catch (Exception ex) {
                ErrorStack.Push(this.GetType(), ex);
                return ObixResult.kObixClientXMLParseError;
            }
            ms.Dispose();
            ms = null;

            result = ParseLobbyContract(doc);
            if (result != ObixResult.kObixClientSuccess) {
                return ErrorStack.Push(this.GetType(), result);
            }

            result = ParseAboutContract();
            if (result != ObixResult.kObixClientSuccess) {
                return ErrorStack.Push(this.GetType(), result);
            }

            connected = true;
            return ObixResult.kObixClientSuccess;
        }

        /// <summary>
        /// Parses the About contract after a client successfully connects and downloads the lobby contract.
        /// </summary>
        /// <returns>kObixClientSuccess if the operation succeeded, another value otherwise.</returns>
        private ObixResult ParseAboutContract() {
            ObixResult<XElement> data = null;
            ObixAbout about = null;

            if (WebClient == null || AboutUri == null) {
                return ErrorStack.Push(this.GetType(), ObixResult.kObixClientInputError,
                    "WebClient is nothing, or the oBIX:About URI could not be found from the lobby contract.");
            }

            data = ReadUriXml(AboutUri);
            if (data.ResultSucceeded == false) {
                return ErrorStack.Push(this.GetType(), ObixResult.kObixClientXMLParseError,
                    "ParseAboutContract received an error from ReadUriXml.");
            }

            about = ObixAbout.FromXElement(data.Result);
            if (about == null) {
                return ErrorStack.Push(this.GetType(), ObixResult.kObixClientXMLParseError,
                    "ParseAboutContract could not parse the obix:About contract.");
            }

            this.About = about;

            return ObixResult.kObixClientSuccess;
        }

        /// <summary>
        /// Parses the lobby contract.
        /// </summary>
        /// <returns>kObixResultSuccess on success, another value otherwise.</returns>
        /// <param name="doc">An XDocument from the server.</param>
        private ObixResult ParseLobbyContract(XDocument doc) {
            XElement rootNode = null;
            XElement lobby = null;

            if (doc == null) {
                return ErrorStack.Push(this.GetType(), ObixResult.kObixClientInputError,
                    "ParseLobbyContract got passed a null document.");
            }

            rootNode = doc.Root;
            if (rootNode == null) {
                return ErrorStack.Push(this.GetType(), ObixResult.kObixClientXMLParseError,
                    "Could not locate the root element in the provided XML document.");
            }

            if (rootNode.Name.LocalName == "obj" && rootNode.ObixIs().Contains("obix:Lobby")) {
                lobby = rootNode;
            }

            foreach (XElement element in rootNode.Descendants()) {
                XAttribute isAttr = null;
                XAttribute hrefAttr = null;
                string isValue = null;
                string hrefValue = null;

                if (element == null || element.HasAttributes == false) {
                    continue;
                }

				isAttr = element.Attribute("is");
                if (isAttr == null || string.IsNullOrEmpty(isAttr.Value) == true) {
                    continue;
                }

                isValue = isAttr.Value;
                hrefAttr = element.Attribute("href");

                if (hrefAttr != null && string.IsNullOrEmpty(hrefAttr.Value) == false) {
                    hrefValue = hrefAttr.Value;
                }

                if (string.IsNullOrEmpty(hrefValue) == false) {
                    if (isValue.Contains("obix:Lobby") == true) {
                        lobby = element;
                    } else if (isValue.Contains("obix:WatchService") == true) {
                        WatchUri = LobbyUri.Concat(hrefValue);

                    } else if (isValue.Contains("obix:About") == true) {
                        AboutUri = LobbyUri.Concat(hrefValue);
                    }
                }
            }

            if (lobby == null) {
                return ErrorStack.Push(this.GetType(), ObixResult.kObixClientXMLElementNotFoundError,
                    "ParseLobbyContract could not find the oBIX Lobby in the response.");
            }

            foreach (XElement lobbyElement in lobby.Elements()) {
                if (lobbyElement.HasAttributes && lobbyElement.ObixIn() == "obix:BatchIn" && lobbyElement.ObixOut() == "obix:BatchOut") {
                    BatchUri = LobbyUri.Concat(lobbyElement.ObixHref());
                }
            }

            //batchService = lobby.XPathSelectElement(".//op[@in=\"obix:BatchIn\" and @out=\"obix:BatchOut\"]");
            //if (batchService != null && batchService.Attribute("href") != null) {
            //    BatchUri = LobbyUri.Concat(batchService.Attribute("href").Value);
            //}

            if (BatchUri != null) {
                batchClient = new XmlBatchClient(this);
            }

            return ObixResult.kObixClientSuccess;
        }

        #endregion

        #region "oBIX Read support"

        /// <summary>
        /// Gets data from the oBIX server specified by <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The URI of the oBIX data point to read</param>
        /// <returns>kObixClientResultSuccess with an XElement representing the 
        /// oBIX XML object on success, another value otherwise.</returns>
        public ObixResult<XElement> ReadUriXml(Uri uri) {
            ObixResult<byte[]> data = null;
            XDocument doc = null;
            MemoryStream ms = null;

            if (WebClient == null) {
                return ErrorStack.PushWithObject(this.GetType(), ObixResult.kObixClientNotConnectedError, (XElement)null);
            }

            data = ReadUri(uri);
            if (data.ResultSucceeded == false) {
                return ErrorStack.PushWithObject(this.GetType(), ObixResult.kObixClientXMLParseError,
                    "ReadUriXml could not understand the downloaded document at URI " + uri.ToString(), (XElement)null);
            }

            using (ms = new MemoryStream(data.Result)) {
                ms.Position = 0;
                try {
                    doc = XDocument.Load(ms);
                } catch (Exception ex) {
                    return ErrorStack.PushWithObject(this.GetType(), ex, (XElement)null);
                }
            }

            if (doc == null || doc.Root == null) {
                return ErrorStack.PushWithObject(this.GetType(), ObixResult.kObixClientXMLParseError,
                    "ReadUriXml could not understand the downloaded document at URI " + uri.ToString(), (XElement)null);
            }

            if (doc.Root.IsObixErrorContract()) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixServerError, doc.Root);
            }

            return ObixResult.FromResult(ObixResult.kObixClientSuccess, doc.Root);
        }

        /// <summary>
        /// Asynchronously gets data from the oBIX server specified by <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The URI of the oBIX data point to read</param>
        /// <returns>kObixClientResultSuccess with an XElement representing the 
        /// oBIX XML object on success, another value otherwise.</returns>
        public async Task<ObixResult<XElement>> ReadUriXmlAsync(Uri uri) {
            ObixResult<byte[]> data = null;
            XDocument doc = null;
            MemoryStream ms = null;

            if (WebClient == null) {
                return ErrorStack.PushWithObject(this.GetType(), ObixResult.kObixClientNotConnectedError, (XElement)null);
            }

            data = await ReadUriAsync(uri);
            if (data.ResultSucceeded == false) {
                return ErrorStack.PushWithObject(this.GetType(), ObixResult.kObixClientXMLParseError,
                    "ReadUriXml could not understand the downloaded document at URI " + uri.ToString(), (XElement)null);
            }

            ms = new MemoryStream(data.Result);
            try {
                doc = await Task.Factory.StartNew<XDocument>(() => XDocument.Load(ms));
            } catch (Exception ex) {
                return ErrorStack.PushWithObject(this.GetType(), ex, (XElement)null);
            }

            ms.Dispose();
            ms = null;

            if (doc == null || doc.Root == null) {
                return ErrorStack.PushWithObject(this.GetType(), ObixResult.kObixClientXMLParseError,
                    "ReadUriXml could not understand the downloaded document at URI " + uri.ToString(), (XElement)null);
            }

            if (doc.Root.IsObixErrorContract()) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixServerError, doc.Root);
            }

            return ObixResult.FromResult(ObixResult.kObixClientSuccess, doc.Root);
        }

        #endregion

        #region "oBIX Write support"

        /// <summary>
        /// Writes the oBIX object specified by <paramref name="element"/> to the endpoint specified by
        /// <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The oBIX endpoint to send data to</param>
        /// <param name="element">An oBIX object to send to the oBIX server</param>
        /// <returns>kObixClientResultSuccess on success with the response oBIX object from the server, 
        /// another value otherwise.</returns>
        public ObixResult<XElement> WriteUriXml(Uri uri, XElement element) {
            XmlWriter writer;
            XmlReader reader;
            XElement responseElement;
            XDocument doc;
            MemoryStream ms;
            ObixResult<byte[]> response;

            if (uri == null || element == null) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientInputError,
                    "Uri, or data to write is nothing.", (XElement)null);
            }

            using (ms = new MemoryStream()) {
                using (writer = XmlWriter.Create(ms)) {
                    element.WriteTo(writer);
                }
                response = WriteUri(uri, ms.ToArray());
            }

            if (response.ResultSucceeded == false) {
                return ErrorStack.PushWithObject(GetType(), response,
                    "WriteUri failed to write data to the obix server: " + response.ToString(), (XElement)null);
            }

            try {
                using (ms = new MemoryStream(response.Result)) {
                    using (reader = XmlReader.Create(ms)) {
                        doc = XDocument.Load(reader);
                    }
                }
            } catch (Exception) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientXMLParseError,
                    "Could not parse the XML response from the server.", (XElement)null);
            }

            responseElement = doc.Root;
            if (responseElement.IsObixErrorContract() == true) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixServerError, responseElement.ObixDisplay(), responseElement);
            }

            return ObixResult.FromResult(ObixResult.kObixClientSuccess, responseElement);
        }

        /// <summary>
        /// Asynchronously writes the oBIX object specified by <paramref name="element"/> to the endpoint specified by
        /// <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The oBIX endpoint to send data to</param>
        /// <param name="element">An oBIX object to send to the oBIX server</param>
        /// <returns>kObixClientResultSuccess on success with the response oBIX object from the server, 
        /// another value otherwise.</returns>
        public async Task<ObixResult<XElement>> WriteUriXmlAsync(Uri uri, XElement element) {
            XmlWriter writer;
            XmlReader reader;
            XElement responseElement;
            XDocument doc;
            MemoryStream ms;
            ObixResult<byte[]> response;
            byte[] data;

            if (uri == null || element == null) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientInputError, "Uri, or data to write is nothing.", (XElement)null);
            }

            try {
                ms = new MemoryStream();
                writer = XmlWriter.Create(ms);
                element.WriteTo(writer);
                data = ms.ToArray();

                writer.Dispose();
                writer = null;
                ms.Dispose();
                ms = null;
            } catch (Exception) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientXMLParseError,
                    "WriteUriXmlAsync could not understand the XML document provided.", (XElement)null);
            }

            response = await WriteUriAsync(uri, data);
            if (response.ResultSucceeded == false) {
                return ErrorStack.PushWithObject(GetType(), response, (XElement)null);
            }

            ms = new MemoryStream(response);
            reader = XmlReader.Create(ms);

            try {
                doc = await Task.Factory.StartNew(() => doc = XDocument.Load(reader));
            } catch (Exception) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientXMLParseError,
                    "WriteUriXmlAsync could not understand the response document provided.", (XElement)null);
            }

            responseElement = doc.Root;
            if (responseElement.IsObixErrorContract() == true) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixServerError, responseElement.ObixDisplay(), responseElement);
            }

            return ObixResult.FromResult(ObixResult.kObixClientSuccess, responseElement);
        }

        #endregion

        #region "oBIX Invoke support"

        /// <summary>
        /// Invokes the obix:op(eration) at endpoint <paramref name="uri"/> with optional parameters specified by <paramref name="element"/>.
        /// </summary>
        /// <param name="uri">URI of the obix:op</param>
        /// <param name="element">An oBIX object to send as parameters to the URI.  If null, no parameters are sent.</param>
        /// <returns>kObixClientResultSuccess on success with the result of the obix:op as an XElement, another value otherwise.</returns>
        public ObixResult<XElement> InvokeUriXml(Uri uri, XElement element) {
            XmlWriter writer = null;
            XmlReader reader = null;
            XElement responseElement = null;
            XDocument doc = null;
            MemoryStream ms = null;
            byte[] request = null;
            ObixResult<byte[]> response = null;

            if (uri == null) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientInputError,
                    "Uri is nothing.", (XElement)null);
            }

            if (element != null) {
                using (ms = new MemoryStream()) {
                    using (writer = XmlWriter.Create(ms)) {
                        element.WriteTo(writer);
                    }
                    request = ms.ToArray();
                }
            }

            response = InvokeUri(uri, request);
            if (response.ResultSucceeded == false) {
                return ErrorStack.PushWithObject(GetType(), response,
                    "WriteUri failed to invoke operation: " + uri.ToString() + ": " + ObixResult.Message(response), (XElement)null);
            }

            try {
                using (ms = new MemoryStream(response.Result)) {
                    using (reader = XmlReader.Create(ms)) {
                        doc = XDocument.Load(reader);
                    }
                }
            } catch (Exception ex) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientXMLParseError,
                    "Could not parse the XML response from the server: " + ex.ToString(), (XElement)null);
            }

            responseElement = doc.Root;
            if (responseElement.IsObixErrorContract() == true) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixServerError, responseElement.ObixDisplay(), responseElement);
            }

            return ObixResult.FromResult(ObixResult.kObixClientSuccess, responseElement);
        }

        /// <summary>
        ///Invokes the obix:op(eration) at endpoint <paramref name="uri"/> with optional parameters specified by <paramref name="element"/>.
        /// <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">URI of the obix:op</param>
        /// <param name="element">An oBIX object to send as parameters to the URI.  If null, no parameters are sent.</param>
        /// <returns>kObixClientResultSuccess on success with the result of the obix:op as an XElement, another value otherwise.</returns>
        public async Task<ObixResult<XElement>> InvokeUriXmlAsync(Uri uri, XElement element) {
            XmlWriter writer;
            XmlReader reader;
            XElement responseElement;
            XDocument doc;
            MemoryStream ms;
            ObixResult<byte[]> response;
            byte[] request = null;

            if (uri == null) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientInputError, "Uri is nothing.", (XElement)null);
            }

            if (element != null) {
                try {
                    using (ms = new MemoryStream()) {
                        using (writer = XmlWriter.Create(ms)) {
                            element.WriteTo(writer);
                        }

                        request = ms.ToArray();
                    }
                } catch (Exception) {
                    return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientXMLParseError,
                        "InvokeUriXmlAsync could not understand the XML document provided.", (XElement)null);
                }
            }

            response = await InvokeUriAsync(uri, request);
            if (response.ResultSucceeded == false) {
                return ErrorStack.PushWithObject(GetType(), response, (XElement)null);
            }

            ms = new MemoryStream(response.Result);
            ms.Seek(0, SeekOrigin.Begin);
            reader = XmlReader.Create(ms);

            try {
                doc = await Task.Factory.StartNew(() => doc = XDocument.Load(reader));
            } catch (Exception) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientXMLParseError,
                    "WriteUriXmlAsync could not understand the response document provided.", (XElement)null);
            }

            responseElement = doc.Root;
            if (responseElement.IsObixErrorContract() == true) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixServerError, responseElement.ObixDisplay(), responseElement);
            }

            return ObixResult.FromResult(ObixResult.kObixClientSuccess, responseElement);
        }

        #endregion

        protected override bool Dispose(bool disposing) {
            base.Dispose(disposing);
            if (_disposed == true) {
                return false;
            }

            if (disposing == true) {
                WebClient.Dispose();
            }

            WebClient = null;

            return true;
        }
    }
}

