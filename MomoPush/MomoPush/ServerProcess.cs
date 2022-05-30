using System.ComponentModel;

namespace MomoPush
{
    public class ServerProcess : CustomHttpServer
    {
        public ServerProcess(ISynchronizeInvoke syncObj, string ip, int port) : base(syncObj, ip, port) { }

        protected override string ResponseProcess(HttpRequest request)
        {
            return "";
        }
    }
}
