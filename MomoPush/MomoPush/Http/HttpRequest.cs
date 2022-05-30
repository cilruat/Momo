using System;
using System.Collections;
using System.Collections.Specialized;
using System.Web;

namespace MomoPush
{
    public class HttpRequest
    {
        public string Protocol { set; get; }

        public Uri Uri { set; get; }

        public string Method { set; get; }

        public string Url { get { return Uri.LocalPath; } }

        public string UserAgent { set; get; }

        public string ContentType { set; get; }

        public Hashtable Headers = new Hashtable();

        public NameValueCollection GetParameters = HttpUtility.ParseQueryString(string.Empty);

        public NameValueCollection PostParameters = HttpUtility.ParseQueryString(string.Empty);

        public string json { set; get; }

        public HttpRequest()
        {
        }
    }
}