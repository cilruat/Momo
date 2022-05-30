using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MomoPush
{
    public abstract class HttpServer
    {
        protected TcpListener listener;
        protected bool is_active = true;
        protected TcpClient client;

        public delegate void HttpReqeustEventHandler(object sender, HttpRequestEventArgs e);

        public event HttpReqeustEventHandler Reqeust;

        #region Synchronizing

        protected ISynchronizeInvoke _SynchronizingObject;

        [Browsable(false), DefaultValue((string)null)]
        public ISynchronizeInvoke SynchronizingObject
        {
            get { return this._SynchronizingObject; }
            set { this._SynchronizingObject = value; }
        }

        #endregion Synchronizing

        public HttpServer(string ip, int port)
        {
            IPEndPoint address = new IPEndPoint(IPAddress.Parse(ip), port);
            listener = new TcpListener(address);
        }

        public void listen()
        {
            listener.Start();
            is_active = true;

            while (is_active)
            {
                try
                {
                    if (!listener.Pending())
                    {
                        Thread.Sleep(100); // choose a number (in milliseconds) that makes sense
                        continue; // skip to next iteration of loop
                    }
                    client = listener.AcceptTcpClient();
                    HttpProcessor processor = new HttpProcessor(client, this);
                    Thread thread = new Thread(new ThreadStart(processor.process));
                    thread.Start();
                    Thread.Sleep(1);
                }
                catch (SocketException)
                {
                }
            }

            client = null;
            listener.Stop();
        }

        public void Stop()
        {
            is_active = false;
            listener.Server.Close();
            if (client != null)
            {
                client.Close();
            }
        }

        public abstract void handleRequest(HttpRequest p, ref HttpResponse response);

        protected void OnReqeust(HttpRequestEventArgs e)
        {
            if (this.Reqeust != null)
            {
                if ((this.SynchronizingObject != null) && this.SynchronizingObject.InvokeRequired)
                {
                    this.SynchronizingObject.BeginInvoke(Reqeust, new object[] { this, e });
                }
                else
                {
                    Reqeust(this, e);
                }
            }
        }
    }
}