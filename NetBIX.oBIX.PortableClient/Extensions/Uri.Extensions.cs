using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;

namespace NetBIX.oBIX.Client.Extensions {
    public static class UriExtensions {

        public static Uri Concat(this Uri uri, string uriString) {
            if (uriString.Contains(":") && Uri.IsWellFormedUriString(uriString, UriKind.Absolute)) {
                return new Uri(uriString);
            } else if (uriString.StartsWith("/")) {
                return new Uri(uri, uriString);
             } else {
                 return new Uri(Url.Combine(uri.ToString(), uriString));
             }
        }

    }
}
