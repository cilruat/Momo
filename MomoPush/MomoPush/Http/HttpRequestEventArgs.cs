using System;

namespace MomoPush
{
    public class HttpRequestEventArgs : EventArgs
    {
        public HttpRequest Request { protected set; get; }

        public HttpRequestEventArgs(HttpRequest request) : base()
        {
            Request = request;
        }
    }
}