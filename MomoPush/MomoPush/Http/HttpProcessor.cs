using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;

namespace MomoPush
{
    public class HttpProcessor
    {
        public TcpClient socket;
        public HttpServer srv;

        public string http_protocol_versionstring;

        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        public HttpProcessor(TcpClient s, HttpServer srv)
        {
            this.socket = s;
            this.srv = srv;
        }

        private string streamReadLine(Stream inputStream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }

        public void process()
        {
            using (Stream inputStream = new BufferedStream(socket.GetStream()))
            {
                HttpRequest request = new HttpRequest();
                HttpResponse response = new HttpResponse();
                try
                {
                    parseRequest(inputStream, ref request);
                    readHeaders(inputStream, ref request);

                    readGetRequestParameter(ref request);

                    if (request.Method.Equals("POST"))
                    {
                        readPostRequestParameter(inputStream, ref request);
                    }

                    response = handleRequest(request);
                }
                catch (Exception ex)
                {
                    response.StatusCode = 500;
                    response.Contents = Encoding.UTF8.GetBytes(ex.ToString().Replace("\n", "<br />"));
                }

                SendResponse(socket.Client, response);
                socket.Close();
            }
        }

        protected void parseRequest(Stream stream, ref HttpRequest request)
        {
            String req = streamReadLine(stream);
            string[] tokens = req.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            request.Method = tokens[0].ToUpper();
            request.Uri = new Uri("http://127.0.0.1" + tokens[1]);
            request.Protocol = tokens[2];

            Debug.WriteLine("starting: " + request);
        }

        protected void readHeaders(Stream stream, ref HttpRequest request)
        {
            Debug.WriteLine("readHeaders()");
            String line;
            while ((line = streamReadLine(stream)) != null)
            {
                if (line.Equals(""))
                {
                    Debug.WriteLine("got headers");
                    return;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                String name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++; // strip any spaces
                }

                string value = line.Substring(pos, line.Length - pos);
                Debug.WriteLine(String.Format("header: {0}:{1}", name, value));
                request.Headers[name] = value;

                if (name.Equals("Content-Type"))
                {
                    request.ContentType = value;
                }
                else if (name.Equals("UserAgent"))
                {
                    request.UserAgent = value;
                }
            }
        }

        protected void readGetRequestParameter(ref HttpRequest request)
        {
            UriBuilder ub = new UriBuilder(request.Uri);
            request.GetParameters = HttpUtility.ParseQueryString(ub.Query);
        }

        private const int BUF_SIZE = 4096;

        protected void readPostRequestParameter(Stream Stream, ref HttpRequest request)
        {
            Debug.WriteLine("get post data start");
            int content_len = 0;
            using (MemoryStream ms = new MemoryStream())
            {
                if (request.Headers.ContainsKey("Content-Length"))
                {
                    content_len = Convert.ToInt32(request.Headers["Content-Length"]);
                    if (content_len > MAX_POST_SIZE)
                    {
                        throw new Exception(
                            String.Format("POST Content-Length({0}) too big for this simple server",
                              content_len));
                    }
                    byte[] buf = new byte[BUF_SIZE];
                    int to_read = content_len;
                    while (to_read > 0)
                    {
                        Debug.WriteLine(String.Format("starting Read, to_read={0}", to_read));

                        int numread = Stream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
                        Debug.WriteLine(String.Format("read finished, numread={0}", numread));
                        if (numread == 0)
                        {
                            if (to_read == 0)
                            {
                                break;
                            }
                            else
                            {
                                throw new Exception("client disconnected during post");
                            }
                        }
                        to_read -= numread;
                        ms.Write(buf, 0, numread);
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                }
                Debug.WriteLine("get post data end");

                using (StreamReader streamReader = new StreamReader(ms))
                {
                    String data = streamReader.ReadToEnd();

                    if (request.ContentType.IndexOf("/json") >= 0)
                    {
                        request.json = data;
                    }
                    else
                    {
                        request.PostParameters = HttpUtility.ParseQueryString(data);
                    }
                }
            }
        }

        protected HttpResponse handleRequest(HttpRequest request)
        {
            HttpResponse response = new HttpResponse();
            srv.handleRequest(request, ref response);
            return response;
        }

        protected static void SendResponse(Socket socket, HttpResponse response)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format("{0} {1} {2}\r\n", HttpCore.HttpVersion, response.StatusCode, response.StatusString));

            sb.Append(string.Format("Date: {0:r}\r\n", DateTime.UtcNow));
            sb.Append(string.Format("Server: {0}\r\n", HttpCore.ServerName));
            foreach (Cookie c in response.Cookies)
            {
                sb.Append(string.Format("Set-Cookie: {0}={1}; path={2}; expires={3}; domain={4}\r\n", c.Name, c.Value, c.Path, c.Expires, c.Domain));
            }
            sb.Append(string.Format("Connection: {0}\r\n", response.Connection));
            sb.Append(string.Format("Last-Modified: {0:r}\r\n", response.LastModified.ToUniversalTime()));
            sb.Append(string.Format("Content-Type: {0}; charset={1}\r\n", response.ContentType, response.CharacterSet));
            sb.Append(string.Format("Content-Length: {0}\r\n", response.ContentLength));
            sb.Append(string.Format("\r\n"));

            try
            {
                socket.Send(Encoding.UTF8.GetBytes(sb.ToString()));
                if (response.StatusCode != 200)
                {
                    String errorMessage = String.Format("<h1>{0} : {1} </h1>", response.StatusCode, response.StatusString);
                    socket.Send(Encoding.UTF8.GetBytes(errorMessage));
                }
                if (response.Contents != null)
                {
                    socket.Send(response.Contents);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}