using NetBIX.oBIX.Client.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace NetBIX.oBIX.Client {
    public class XmlBatch : ObixBatch {
        protected readonly object __batchItemListMutex = new object();
        public List<ObixXmlBatchItem> XmlBatchItemList { get; protected set; }

        public XmlBatch() {
            this.XmlBatchItemList = new List<ObixXmlBatchItem>();
        }

        /// <summary>
        /// Adds an oBIX XML batch item to the current batch request.
        /// </summary>
        /// <param name="Op">The oBIX Operation type, being one of obix:Read, obix:Write, or obix:Invoke</param>
        /// <param name="uri">The URI of the read, write or Invoke</param>
        /// <param name="ObixParameters">An optional XML parameter to be supplied to an oBIX:Write, or obix:Invoke</param>
        /// <returns>An instance of the newly created ObixXmlBatchItem, or null if an error occured.</returns>
        public ObixXmlBatchItem AddXmlBatchItem(ObixBatchOperation Op, string uri, XElement ObixParameters = null) {
            ObixXmlBatchItem item = new ObixXmlBatchItem(Op, uri, ObixParameters);
            
            if (item == null) {
                return null; 
            }

            try {
                lock (__batchItemListMutex) {
                    XmlBatchItemList.Add(item);
                }
            } catch (Exception) {
                return null;
            }

            return item;
        }

        /// <summary>
        /// Gets the number of obix:BatchIn items in this current batch instance.
        /// </summary>
        public int BatchItemCount {
            get {
                return XmlBatchItemList.Count;
            }
        }

        public ObixXmlBatchItem GetXmlBatchItem(int index) {
            if (index < 0 || index > XmlBatchItemList.Count) {
                return null;
            }

            return XmlBatchItemList[index];
        }


        /// <summary>
        /// Generates an obix:BatchIn contract with all the oBIX Batch items in it.
        /// </summary>
        /// <returns>An ObixResult indicating success and the obix:BatchIn wrapped if the operation is to succeed, another value otherwise.</returns>
        public ObixResult<XElement> ToBatchInContract() {
            XElement batchInContract = null;
            XElement batchInItem = null;
            try {
                batchInContract = new XElement("list", new XAttribute("is", "obix:BatchIn"));

                //prevent modification of the batch list mid-iteration
                
                //TODO: Think about how holding a mutex for this amount of
                //code is going to affect performance.  Cut down the critical
                //region?
                lock (__batchItemListMutex) {
                    foreach (ObixXmlBatchItem item in XmlBatchItemList) {
                        batchInItem = new XElement("uri");
                        string obixOperation = "obix:Read";

                        switch (item.Operation) {
                            case ObixBatchOperation.kObixBatchOperationWrite:
                                obixOperation = "obix:Write";
                                break;
                            case ObixBatchOperation.kObixBatchOperationInvoke:
                                obixOperation = "obix:Invoke";
                                break;
                            default:
                                break;
                        }

                        if (obixOperation == null) {
                            continue;
                        }

                        batchInItem.SetAttributeValue("is", obixOperation);
                        batchInItem.SetAttributeValue("val", item.UriString);

                        if (item.XmlRequestData != null) {
                            batchInItem.Add(item.XmlRequestData);
                        }

                        batchInContract.Add(batchInItem);
                    }
                }
            } catch (Exception) {
                return ObixResult.FromResult(ObixResult.kObixClientException, (XElement)null);
            }

            return ObixResult.FromResult(ObixResult.kObixClientSuccess, batchInContract);
        }
    }

    public class ObixXmlBatchItem : ObixBatchItem {
        /// <summary>
        /// Contains the XML response from this Batch operation from the oBIX Server
        /// </summary>
        public XElement XmlBatchResponse { get; set; }

        public XElement XmlRequestData { get; protected set; }

        public ObixXmlBatchItem(ObixBatchOperation Op, string uri, XElement xmlData) : base(Op, uri, null) {
            this.XmlRequestData = xmlData;
        }

        protected string GetOperationCode() {
            string r = null;
            switch (this.Operation) {
                case ObixBatchOperation.kObixBatchOperationInvoke:
                    r = "obix:Invoke";
                    break;
                case ObixBatchOperation.kObixBatchOperationRead:
                    r = "obix:Read";
                    break;
                case ObixBatchOperation.kObixBatchOperationWrite:
                    r = "obix:Write";
                    break;
                default:
                    r = "(unknown)";
                    break;
            }
            return r;
        }

        public override string ToString() {
            return string.Format("{0} of href {1}", GetOperationCode(), this.UriString);
        }
    }
}
