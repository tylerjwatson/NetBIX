using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetBIX.oBIX.Client.Framework {

    /// <summary>
    /// Every obix batch operation must pertain to a Read, write or invoke.
    /// </summary>
    public enum ObixBatchOperation {
        kObixBatchOperationRead,
        kObixBatchOperationWrite,
        kObixBatchOperationInvoke
    }

    /// <summary>
    /// Provides an oBIX:Batch mechanism for batching read, write, and Invoke operations
    /// into a single request.
    /// 
    /// This class is abstract and can't be instantiated, see one of the derived implementations
    /// for an implementation of the relevant oBIX:Batch mechanism.
    /// </summary>
    public abstract class ObixBatch {
        protected readonly object __itemListMutex = new object();
        protected List<ObixBatchItem> itemList;

        public ObixBatch() {
            itemList = new List<ObixBatchItem>();
        }

        /// <summary>
        /// Adds an obix:BatchIn item consisting of the oBIX <paramref name="Op"/>eration, the <paramref name="Uri"/> to send the request to, and optionally 
        /// a byte array of data to send as the parameter to the oBIX:BatchIn item.
        /// </summary>
        /// <param name="Op"></param>
        /// <param name="relativeUri"></param>
        /// <param name="data"></param>
        /// <returns>The oBIX BatchItem if the operation is to succeed, an exception otherwise.</returns>
        protected ObixBatchItem AddBatchItem(ObixBatchOperation Op, string relativeUri, byte[] data) {
            ObixBatchItem item = null;

            item = new ObixBatchItem(Op, relativeUri, data);
            lock (__itemListMutex) {    
                this.itemList.Add(item);
            }

            return item;
        }

        /// <summary>
        /// Removes a batch item from the obix:Batch request.
        /// </summary>
        /// <param name="Item">The obix:BatchIn item to remove</param>
        protected void RemoveBatchItem(ObixBatchItem Item) {
            if (Item == null || this.itemList.Contains(Item) == false) {
                return;
            }

            lock (__itemListMutex) {
                this.itemList.Remove(Item);
            }
        }
    }

    /// <summary>
    /// Holds an oBIX:BatchIn item.  An operation can only be one of obix:Read, obix:Write, or obix:Invoke
    /// </summary>
    public class ObixBatchItem {
        public ObixBatchOperation Operation { get; protected set; }
        public string UriString { get; protected set; }
        public byte[] RequestData { get; protected set; }

        public ObixBatchItem(ObixBatchOperation Op, string uri, byte[] RequestData) {
            this.Operation = Op;
            this.UriString = uri;
            this.RequestData = RequestData;
        }
    }

    /// <summary>
    /// Holds the raw response from an obix:Batch request.
    /// </summary>
    public class ObixBatchResponse {
        public byte[] ResponseData { get; protected set; }
        public ObixBatchItem Request { get; protected set; }

        public ObixBatchResponse(ObixBatchItem Request, byte[] ResponseData) {
            this.Request = Request;
            this.ResponseData = ResponseData;
        }
    }
}
