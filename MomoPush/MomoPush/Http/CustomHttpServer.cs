using System.ComponentModel;
using System.Text;

namespace MomoPush
{
    public abstract class CustomHttpServer : HttpServer
    {
        public string UserAgent { set; get; }

        public CustomHttpServer(string ip, int port) : base(ip, port)
        {
        }

        public CustomHttpServer(ISynchronizeInvoke syncObj, string ip, int port) : base(ip, port)
        {
            SynchronizingObject = syncObj;
        }

        public override void handleRequest(HttpRequest request, ref HttpResponse response)
        {
            if (request.Url.ToUpper().Equals("/favicon.ico".ToUpper()))
            {
                return;
            }

            OnReqeust(new HttpRequestEventArgs(request));

            response.StatusCode = 200;
            response.Contents = Encoding.UTF8.GetBytes(ResponseProcess(request));
        }

        protected abstract string ResponseProcess(HttpRequest request);
    }
}