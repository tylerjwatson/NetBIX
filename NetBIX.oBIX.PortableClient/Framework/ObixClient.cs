using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NetBIX.oBIX.Client.Framework {

    /// <summary>
    /// Main abstraction for oBIX client implementations.  Implement 
    /// this class to abstract between different flabours of the oBIX
    /// protocol.
    /// </summary>
    public abstract class ObixClient : IDisposable {
        protected bool _disposed = false;
        protected bool connected = false;

        /// <summary>
        /// A reference to the .net web client that will be making normal calls to the oBIX server
        /// via HTTP.
        /// </summary>
        public HttpClient WebClient { get; protected set; }

        /// <summary>
        /// Provides access to a local error stack with at maximum 100 errors that get generated
        /// by this instance of the oBIX client when it encounters an error.
        /// </summary>
        public ObixErrorStack ErrorStack { get; protected set; }

        /// <summary>
        /// Provides information about the oBIX server this instance of ObixClient is connected to.
        /// </summary>
        public ObixAbout About { get; protected set; }

        /// <summary>
        /// URI of the oBIX:Lobby
        /// </summary>
        public Uri LobbyUri { get; private set; }

        /// <summary>
        /// Connects to the oBIX Server - Connect() must be invoked before any other operation on this
        /// instance of the oBIX client.
        /// </summary>
        /// <returns>kObixClientResultSuccess on success, another value otherwise</returns>
        public abstract ObixResult Connect();
        
        /// <summary>
        /// Asynchronously connects to the oBIX Server - Connect() must be invoked before any other 
        /// operation on this instance of the oBIX client.
        /// </summary>
        /// <returns>kObixClientResultSuccess on success, another value otherwise</returns>
        public abstract Task<ObixResult> ConnectAsync();

        /// <summary>
        /// Gets raw data from the oBIX server specified by <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The URI of the oBIX data point to read</param>
        /// <returns>kObixClientResultSuccess with the data array on success, another value otherwise.</returns>
        /// <remarks>This method should be regarded as raw data, see the derived implementations of ReadUri*.</remarks>
        public ObixResult<byte[]> ReadUri(Uri uri) {
            byte[] data = null;

            if (WebClient == null) {
				return ErrorStack.PushWithObject<byte[]>(this.GetType(), ObixResult.kObixClientNotConnectedError, (byte[])null);
            }

            try {
                data = WebClient.GetByteArrayAsync(uri.ToString()).Result;
            } catch (Exception ex) {
                connected = false;
                return ErrorStack.PushWithObject<byte[]>(this.GetType(), ex, null);
            }

            return ObixResult.FromResult(ObixResult.kObixClientSuccess, data);
        }
        
        /// <summary>
        /// Asynchronously gets raw data from the oBIX server specified by <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The URI of the oBIX data point to read</param>
        /// <returns>kObixClientResultSuccess with the data array on success, another value otherwise.</returns>
        /// <remarks>
        /// This method should be regarded as raw data, see the derived implementations of ReadUriAsync
        /// for high-level oBIX reads.
        /// </remarks>
        public async Task<ObixResult<byte[]>> ReadUriAsync(Uri uri) {
            byte[] data = null;

            if (WebClient == null) {
				return ErrorStack.PushWithObject<byte[]>(this.GetType(), ObixResult.kObixClientNotConnectedError, (byte[])null);
            }

            try {
                data = await WebClient.GetByteArrayAsync(uri.ToString());
            } catch (Exception ex) {
                return ErrorStack.PushWithObject<byte[]>(this.GetType(), ex, null);
            }

            return ObixResult.FromResult(ObixResult.kObixClientSuccess, data);
        }

        /// <summary>
        /// Writes the data provided by <paramref name="data"/> to the endpoint specified by
        /// <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The oBIX endpoint to send data to</param>
        /// <param name="data">A byte array of the data to send to the oBIX server</param>
        /// <returns>kObixClientResultSuccess on success with the response byte array from the server, 
        /// another value otherwise.</returns>
        /// <remarks>
        /// This method should be regarded as raw data, see the derived implementations of ReadUriAsync
        /// for high-level oBIX reads.
        /// </remarks>
        public ObixResult<byte[]> WriteUri(Uri uri, byte[] data) {
            HttpResponseMessage response;
            byte[] responseContent = null;

            if (WebClient == null || connected == false) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientNotConnectedError, (byte[])null);
            }

            if (data == null || data.Length == 0) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientInputError,
                    "WriteUri called with no data to write to the oBIX endpoint.", (byte[])null);
            }

            try {
                response = WebClient.PutAsync(uri, new ByteArrayContent(data)).Result;
            } catch (Exception ex) {
                return ErrorStack.PushWithObject(GetType(), ex, (byte[])null);
            }

            if (response.IsSuccessStatusCode == false) {
                return ErrorStack.PushWithObject<byte[]>(GetType(), ObixResult.kObixClientSocketError, response.ReasonPhrase);
            }

            responseContent = response.Content.ReadAsByteArrayAsync().Result;

            return ObixResult.FromResult(ObixResult.kObixClientSuccess, responseContent);
        }

        /// <summary>
        /// Asynchronously writes the data provided by <paramref name="data"/> to the endpoint specified by
        /// <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">The oBIX endpoint to send data to</param>
        /// <param name="data">A byte array of the data to send to the oBIX server</param>
        /// <returns>kObixClientResultSuccess on success with the response byte array from the server, 
        /// another value otherwise.</returns>
        /// <remarks>
        /// This method should be regarded as raw data, see the derived implementations of ReadUriAsync
        /// for high-level oBIX reads.
        /// </remarks>
        public async Task<ObixResult<byte[]>> WriteUriAsync(Uri uri, byte[] data) {
            HttpResponseMessage response;
            byte[] responseContent;

            if (WebClient == null || connected == false) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientNotConnectedError, (byte[])null);
            }

            if (data == null || data.Length == 0) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientInputError,
                    "WriteUri called with no data to write to the oBIX endpoint.", (byte[])null);
            }

            try {
                response = await WebClient.PutAsync(uri, new ByteArrayContent(data));
            } catch (Exception ex) {
                return ErrorStack.PushWithObject(GetType(), ex, (byte[])null);
            }

            if (response.IsSuccessStatusCode == false) {
                return ErrorStack.PushWithObject<byte[]>(GetType(), ObixResult.kObixClientSocketError, response.ReasonPhrase);
            }

            try {
                responseContent = await response.Content.ReadAsByteArrayAsync();
            } catch (Exception ex) {
                return ErrorStack.PushWithObject(GetType(), ex, (byte[])null);
            }

            return ObixResult.FromResult(ObixResult.kObixClientSuccess, responseContent);
        }

        /// <summary>
        /// Invokes an obix:op(eration) on the oBIX server at endpoint <paramref name="uri"/>, optionally
        /// with the data specified by <paramref name="data"/>.
        /// </summary>
        /// <param name="uri">The URI of the obix:op endpoint to invoke</param>
        /// <param name="data">(optional) The data to send as parameters to the obix:op</param>
        /// <returns>kObixClientResultSuccess on success with the response byte array on success, 
        /// another value otherwise.</returns>
        public ObixResult<byte[]> InvokeUri(Uri uri, byte[] data) {
            HttpResponseMessage responseMessage = null;
            HttpRequestMessage requestMessage = null;

            byte[] response = null;

            if (WebClient == null || connected == false) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientNotConnectedError, (byte[])null);
            }

            if (data == null) {
                data = new byte[0];
            }

            try {
                requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
                requestMessage.Headers.ExpectContinue = false;
                requestMessage.Content = new ByteArrayContent(data);
                requestMessage.Content.Headers.ContentLength = data.Length;

                responseMessage = WebClient.SendAsync(requestMessage).Result;
            } catch (Exception ex) {
                return ErrorStack.PushWithObject(GetType(), ex, (byte[])null);
            }

            if (responseMessage.IsSuccessStatusCode == false) {
                return ErrorStack.PushWithObject<byte[]>(GetType(), ObixResult.kObixClientSocketError, responseMessage.ReasonPhrase);
            }

            response = responseMessage.Content.ReadAsByteArrayAsync().Result;

            return ObixResult.FromResult(ObixResult.kObixClientSuccess, response);
        }

        /// <summary>
        /// Asynchronously invokes an obix:op(eration) on the oBIX server at endpoint <paramref name="uri"/>, optionally
        /// with the data specified by <paramref name="data"/>.
        /// </summary>
        /// <param name="uri">The URI of the obix:op endpoint to invoke</param>
        /// <param name="data">(optional) The data to send as parameters to the obix:op</param>
        /// <returns>kObixClientResultSuccess on success with the response byte array on success, 
        /// another value otherwise.</returns>
        public async Task<ObixResult<byte[]>> InvokeUriAsync(Uri uri, byte[] data) {
            HttpResponseMessage responseMessage = null;
            HttpRequestMessage requestMessage = null;
            byte[] response = null;

            if (WebClient == null || connected == false) {
                return ErrorStack.PushWithObject(GetType(), ObixResult.kObixClientNotConnectedError, (byte[])null);
            }

            try {
                requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
                requestMessage.Headers.ExpectContinue = false;
                requestMessage.Content = new ByteArrayContent(data);
                requestMessage.Content.Headers.ContentLength = data.Length;

                responseMessage = await WebClient.SendAsync(requestMessage);
            } catch (Exception ex) {
                return ErrorStack.PushWithObject(GetType(), ex, (byte[])null);
            }

            if (responseMessage.IsSuccessStatusCode == false) {
                return ErrorStack.PushWithObject<byte[]>(GetType(), ObixResult.kObixClientSocketError, responseMessage.ReasonPhrase);
            }

            response = await responseMessage.Content.ReadAsByteArrayAsync();

            return ObixResult.FromResult(ObixResult.kObixClientSuccess, response);
        }

        public ObixClient(Uri ObixLobbyUri) {
            LobbyUri = ObixLobbyUri;
        }

        #region IDisposable implementation

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        protected virtual bool Dispose(bool disposing) {
            if (_disposed == true) {
                return false;
            }

            if (disposing) {
                //free IDisposable elements
            }

            //set everything else to null

            _disposed = true;
            return true;
        }

        #endregion

    }
}

