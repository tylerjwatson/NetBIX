using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetBIX.oBIX.Client.Framework {

    /// <summary>
    /// Class that contains a list of known oBIX results, and errors.
    /// 
    /// Used when the oBIX API method does not return a result.
    /// </summary>
    public class ObixResult {
        protected int _result;

        public const int kObixClientUnknownError = -1;
        public const int kObixClientSuccess = 0;
        public const int kObixClientNotConnectedError = 1;
        public const int kObixClientSocketError = 2;
        public const int kObixClientXMLParseError = 3;
        public const int kObixClientXMLElementNotFoundError = 4;
        public const int kObixClientIOError = 5;
        public const int kObixClientInputError = 6;
        public const int kObixClientException = 7;
        public const int kObixServerUnknownUriError = 8;
        public const int kObixServerUnsupportedError = 9;
        public const int kObixServerError = 10;

        private static Dictionary<int, string> itemDict = new Dictionary<int, string>()
        { 
            { ObixResult.kObixClientUnknownError, "Unknown error." },
            { ObixResult.kObixClientSuccess, "The operation completed successfully." },
            { ObixResult.kObixClientNotConnectedError, "The instance of the oBIX client is not connected.  Use Connect() before attempting any operations on it." },
            { ObixResult.kObixClientSocketError, "The oBIX client encountered a socket error." },
            { ObixResult.kObixClientXMLParseError, "The XML parser could not understand the XML document provided." },
            { ObixResult.kObixClientXMLElementNotFoundError, "The XML Parser not find an element in the source document." },
            { ObixResult.kObixClientIOError, "The oBIX client recieved an I/O error." },
            { ObixResult.kObixClientInputError, "Parameter input error." },
            { ObixResult.kObixServerUnknownUriError, "The oBIX server returned an oBIX:BadUriErr error contract."},
            { ObixResult.kObixServerUnsupportedError, "The oBIX server returned an oBIX:UnsupportedErr error contract."},
            { ObixResult.kObixServerError, "The oBIX server returned an oBIX error contract."}
        };

        public ObixResult() {
            _result = kObixClientUnknownError;
        }

        public ObixResult(int result) {
            this._result = result;
        }

        public static implicit operator int(ObixResult result) {
            return result._result;
        }

        public static implicit operator ObixResult(int result) {
            return new ObixResult(result);
        }

        public static bool operator ==(int leftOperand, ObixResult rightOperand) {
            return (int)rightOperand == leftOperand;
        }

        public static bool operator !=(int leftOperand, ObixResult rightOperand)
        {
            return (int)rightOperand != leftOperand;
        }

        public static ObixResult<TResult> FromResult<TResult>(ObixResult result, TResult obj) {
            return new ObixResult<TResult>(result, obj);
        }

        /// <summary>
        /// Retrieves an oBIX client error message.
        /// </summary>
        /// <returns>The obix result message.</returns>
        /// <param name="result">Result.</param>
        public static string Message(int result) {
            ObixResult obixResult = ObixResult.kObixClientUnknownError;

            obixResult = (ObixResult)result;

            if (itemDict.ContainsKey(obixResult) == false) {
                return "Unknown error: " + result;
            }

            return itemDict[obixResult];
        }


        public override string ToString() {
            return Message(this);
        }
    }

    /// <summary>
    /// Object that encapsulates calls to oBIX APIs with a return value.
    /// </summary>
    /// <typeparam name="TResultType">The type of the return value contained in the Result property.</typeparam>
    public class ObixResult<TResultType> : ObixResult {
        public TResultType Result { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the oBIX result succeeded and has a value.
        /// </summary>
        public bool ResultSucceeded {
            get {
                return Result != null && _result == ObixResult.kObixClientSuccess;
            }
        }

        public ObixResult(int ObixResult, TResultType Result) {
            _result = ObixResult;
            this.Result = Result;
        }

    }
}
