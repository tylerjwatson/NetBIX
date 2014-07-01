using NetBIX.oBIX.Client.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NetBIX.oBIX.Client.Extensions;

namespace NetBIX.oBIX.Client {
    public class XmlBatchClient {

        /// <summary>
        /// Gets a reference to the XML oBIX client used to send and retrieve messages for this
        /// batch operation.
        /// </summary>
        XmlObixClient ObixClient { get; set; }

        public XmlBatchClient(XmlObixClient ObixClient) {
            this.ObixClient = ObixClient;
        }

        public XmlBatch CreateBatch() {
            XmlBatch batch = new XmlBatch();
            return batch;
        }

        /// <summary>
        /// Parses an obix:BatchOut contract referenced by <paramref name="BatchOutContract"/> from an oBIX server into the XML batch referenced by <paramref name="BatchObject"/>.
        /// 
        /// Results returned from an oBIX server are placed into the XmlBatchResponse property of each item in the XmlBatch container.
        /// </summary>
        /// <param name="BatchOutContract">A reference to an XElement containing the XML obix:BatchOut contract</param>
        /// <param name="BatchObject">A reference to the XmlBatch object containing the request data to bind to.</param>
        /// <returns>kObixClientSuccess if the operation is to succeed without error, another value otherwise.</returns>
        protected ObixResult ParseBatchOutContract(ref XElement BatchOutContract, ref XmlBatch BatchObject) {
            IEnumerable<XElement> batchOutElements = null;
            int batchOutCount = 0;

            if (BatchOutContract == null || BatchObject == null) {
                return ObixClient.ErrorStack.Push(GetType(), ObixResult.kObixClientInputError, "ParseBatchOutContract failed: Provided BatchOutContract or BatchObject is null.");
            }

            if (BatchOutContract.ObixIs() != "obix:BatchOut") {
                return ObixClient.ErrorStack.Push(GetType(), ObixResult.kObixServerError, "The response provided from the obix:Batch operation was not an obix:BatchOut contract.");
            }

            batchOutElements = BatchOutContract.Elements();
            if (batchOutElements == null) {
                return ObixClient.ErrorStack.Push(GetType(), ObixResult.kObixClientXMLParseError, "batchOutElements is null.");
            }

            batchOutCount = batchOutElements.Count();
            if (batchOutCount != BatchObject.BatchItemCount) {
                return ObixClient.ErrorStack.Push(GetType(), ObixResult.kObixClientXMLParseError, "The number of response objects in the obix:BatchOut contract does not match " +
                    "the number of items in the obix:BatchIn contract.  Something on the server side is really broken.");
            }

            for (int i = 0; i < batchOutCount; i++) {
                XElement batchOutNode = batchOutElements.ElementAt(i);
                ObixXmlBatchItem batchItem = BatchObject.GetXmlBatchItem(i);

                if (batchOutNode == null || batchItem == null) {
                    return ObixClient.ErrorStack.Push(GetType(), ObixResult.kObixClientXMLElementNotFoundError, "Could not bind the obix:BatchOut node to the batch item, one of the nodes is null.");
                }

                batchItem.XmlBatchResponse = batchOutNode;
            }

            return ObixResult.kObixClientSuccess;
        }

        /// <summary>
        /// Sends an oBIX:Batch to the registered batch endpoint in the XmlObixClient, and waits for
        /// the BatchOut response from the server.
        /// 
        /// The provided reference to <paramref name="Batch"/> will have each Batch item updated
        /// with the response from the obix:Batch operation on the server.
        /// </summary>
        /// <param name="Batch">The XmlBatch object that contains a list of Batch items.</param>
        public ObixResult SubmitBatch(ref XmlBatch Batch) {
            XElement batchOut = null;
            ObixResult<XElement> batchInBuffer = null;
            ObixResult<XElement> batchOutResponse = null;
            ObixResult batchOutParseResult = null;

            if (Batch == null) {
                return ObixClient.ErrorStack.PushWithObject<XElement>(GetType(), ObixResult.kObixClientInputError);
            }

            batchInBuffer = Batch.ToBatchInContract();
            if (batchInBuffer.ResultSucceeded == false) {
                return ObixClient.ErrorStack.PushWithObject<XElement>(GetType(), batchInBuffer);
            }

            //todo: send batch
            batchOutResponse = ObixClient.InvokeUriXml(ObixClient.BatchUri, batchInBuffer.Result);
            if (batchInBuffer.ResultSucceeded == false) {
                return ObixClient.ErrorStack.PushWithObject<XElement>(GetType(), batchInBuffer);
            }

            batchOut = batchOutResponse.Result;
            batchOutParseResult = ParseBatchOutContract(ref batchOut, ref Batch);
            if (batchOutParseResult != ObixResult.kObixClientSuccess) {
                return ObixClient.ErrorStack.PushWithObject<XElement>(GetType(), batchOutParseResult);
            }

            return ObixResult.kObixClientSuccess;
        }

        public async Task<ObixResult> SubmitBatch(XmlBatch Batch) {
            return await Task.Factory.StartNew<ObixResult>(() => { return SubmitBatch(ref Batch); });
        }

    }
}
