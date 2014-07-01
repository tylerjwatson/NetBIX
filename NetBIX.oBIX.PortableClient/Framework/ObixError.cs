using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using NetBIX.oBIX.Client.Extensions;

namespace NetBIX.oBIX.Client.Framework {

    public class ObixError {
        public DateTime ErrorDate { get; private set;}
        public int ErrorCode { get; private set; }
        public int AuxErrorCode { get; private set; }
        public string AuxErrorMessage { get; private set; }
        public Exception Exception { get; private set; }
        public Type @Type { get; private set; }

        public ObixError(Type @Type, XElement ErrContract) {
            string href = null;
            string displayName = null;

            if (ErrContract == null  
                || ErrContract.IsNullOrNullContract() == true 
                || ErrContract.IsObixErrorContract() == false) {
                return;
            }

            this.Type = @Type;
            this.ErrorDate = DateTime.UtcNow;

            href = ErrContract.ObixHref();
            if (href != null) {
                this.AuxErrorMessage = href;
            }

            displayName = ErrContract.ObixDisplayName();
            if (displayName != null) {
                this.AuxErrorMessage += " - " + displayName;
            }
            
            switch (ErrContract.ObixIs()) {
                case "obix:BadUriErr":
                    this.ErrorCode = (int)ObixResult.kObixServerUnknownUriError;
                    break;
                case "obix:UnsupportedErr":
                    this.ErrorCode = (int)ObixResult.kObixServerUnsupportedError;
                    break;
                default:
                    this.ErrorCode = (int)ObixResult.kObixServerError;
                    break;
            }
        }

        public ObixError(Type @Type, int ErrorCode) {
            this.Type = Type;
            this.ErrorCode = ErrorCode;
            this.AuxErrorMessage = ObixResult.Message(ErrorCode);
            this.ErrorDate = DateTime.UtcNow;
        }

        public ObixError(Type @Type, int ErrorCode, int AuxErrorCode, string AuxErrorMessage) {
            this.Type = Type;
            this.ErrorCode = ErrorCode;
            this.AuxErrorCode = AuxErrorCode;
            this.AuxErrorMessage = AuxErrorMessage;
            this.ErrorDate = DateTime.UtcNow;
        }

        public ObixError(Type @Type, int ErrorCode, Exception ex) {
            this.Type = Type;
            this.ErrorCode = ErrorCode;
            this.Exception = ex;
            this.AuxErrorMessage = ex.Message;
            this.ErrorDate = DateTime.UtcNow;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            if (this.Exception == null) {
                sb.AppendFormat("[{3} - {0}] Error {1}: {2}", this.ErrorDate.ToLocalTime(), this.ErrorCode, ObixResult.Message(this.ErrorCode), this.Type.ToString());
                if (string.IsNullOrEmpty(this.AuxErrorMessage) == false) {
                    sb.AppendFormat(" ({0}: {1})", this.AuxErrorCode, this.AuxErrorMessage);
                }
            } else {
                sb.AppendFormat("[{3} - {0}] Exception {1}: {2}", this.ErrorDate, this.ErrorCode, this.Exception.GetType().ToString(), this.Exception.Message);
            }
            return sb.ToString();
        }
    }

    public class ObixErrorStack {
        public event EventHandler<ObixErrorEventArgs> ErrorAdded;

        public readonly object errorMutex = new object();
        List<ObixError> errorStack = new List<ObixError>();

        public bool HasErrors {
            get {
                return errorStack.Count != 0;
            }
        }

        /// <summary>
        /// Pushes an ObixError onto the stack if not cancelled.
        /// </summary>
        /// <param name="error">An instance of an Obix Error.</param>
        public ObixResult Push(ObixError error) {
            ObixErrorEventArgs args = null;

            if (error == null) {
                return ObixResult.kObixClientUnknownError;
            }

            args = new ObixErrorEventArgs(error);
            if (ErrorAdded != null) {
                ErrorAdded(null, args);
            }

            if (args.Cancelled == true) {
                return ObixResult.kObixClientUnknownError;
            }

            lock (errorMutex) {
                errorStack.Insert(0, error);
                if (errorStack.Count >= 100) {
                    errorStack.RemoveAt(errorStack.Count);
                }
            }

            return (ObixResult)error.ErrorCode;
        }
        public ObixResult Push(Type @Type, Exception ex) {
            return Push(new ObixError(@Type, (int)ObixResult.kObixClientException, ex));
        }

        public ObixResult Push(Type @Type, ObixResult Result) {
            return Push(new ObixError(@Type, (int)Result));
        }
        public ObixResult Push(Type @Type, ObixResult Result, int AuxErrorCode, string AuxMessage) {
            return Push(new ObixError(@Type, (int)Result, AuxErrorCode, AuxMessage));
        }
        public ObixResult Push(Type @Type, ObixResult Result, string AuxMessage) {
            return Push(new ObixError(@Type, (int)Result, 0, AuxMessage));
        }

        public ObixResult<TObject> PushWithObject<TObject>(Type @Type, Exception ex, TObject obj = default(TObject)) {
            return new ObixResult<TObject>(Push(new ObixError(@Type, (int)ObixResult.kObixClientException, ex)), obj);
        }

        public ObixResult<TObject> PushWithObject<TObject>(Type @Type, ObixResult Result, TObject obj = default(TObject)){
            return new ObixResult<TObject>(Push(Type, Result), obj);
        }

        public ObixResult<TObject> PushWithObject<TObject>(Type @Type, ObixResult Result, int AuxErrorCode, string AuxMessage, TObject obj = default(TObject)) {
            return new ObixResult<TObject>(Push(new ObixError(@Type, (int)Result, AuxErrorCode, AuxMessage)), obj);
        }

        public ObixResult<TObject> PushWithObject<TObject>(Type @Type, ObixResult Result, string AuxMessage, TObject obj = default(TObject)) {
            return new ObixResult<TObject>(Push(new ObixError(@Type, (int)Result, 0, AuxMessage)), obj);
        }

        /// <summary>
        /// Pops a single error off the stack.
        /// </summary>
        /// <returns>A single ObixError object, or null if empty or an error.</returns>
        public ObixError Pop() {
            ObixError error = null;

            lock (errorMutex) {
                if (errorStack.Count > 0) {
                    error = errorStack[0];
                }
                if (error != null) {
                    errorStack.Remove(error);
                }
            }

            return error;
        }

        /// <summary>
        /// Pops the entire error stack into an array.
        /// </summary>
        /// <returns>An array of ObixErrors, or NULL if an error occured.</returns>
        public ObixError[] PopArray() {
            ObixError[] errArry = null;
            
            if (errorStack.Count == 0) {
                return null;
            }

            lock (errorMutex) {
                errArry = new ObixError[errorStack.Count];
                errorStack.CopyTo(errArry);
                errorStack.Clear();
            }

            return errArry;
        }
    }

    public class ObixErrorEventArgs : EventArgs {
        /// <summary>
        /// A reference to the oBIX Error that sent the event
        /// </summary>
        public ObixError Error { get; private set; }
        /// <summary>
        /// Disables pushing of the error into the error stack. 
        /// Set to true to disable logging of the error into the
        /// error list.
        /// </summary>
        public bool Cancelled { get; set; }

        public ObixErrorEventArgs(ObixError Error) {
            this.Error = Error;
            this.Cancelled = false;
        }
    }
}

