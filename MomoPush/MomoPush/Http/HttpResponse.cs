using System;
using System.Collections.Generic;
using System.Net;

namespace MomoPush
{
    public class HttpResponse
    {
        private int _StatusCode = 200;

        public int StatusCode
        {
            set
            {
                if (value == 404 || value == 500)
                {
                    Connection = "close";
                }

                _StatusCode = value;
            }
            get
            {
                return _StatusCode;
            }
        }

        public String StatusString
        {
            get
            {
                switch (StatusCode)
                {
                    case 100: return "Continue";
                    case 101: return "Switching Protocols";
                    case 102: return "Processing (WebDAV; RFC 2518)";
                    case 200: return "OK";
                    case 201: return "Created";
                    case 202: return "Accepted";
                    case 203: return "Non-Authoritative Information (since HTTP/1.1)";
                    case 204: return "No Content";
                    case 205: return "Reset Content";
                    case 206: return "Partial Content (RFC 7233)";
                    case 207: return "Multi-Status (WebDAV; RFC 4918)";
                    case 208: return "Already Reported (WebDAV; RFC 5842)";
                    case 226: return "IM Used (RFC 3229)";
                    case 300: return "Multiple Choices";
                    case 301: return "Moved Permanently";
                    case 302: return "Found";
                    case 303: return "See Other (since HTTP/1.1)";
                    case 304: return "Not Modified (RFC 7232)";
                    case 305: return "Use Proxy (since HTTP/1.1)";
                    case 306: return "Switch Proxy";
                    case 307: return "Temporary Redirect (since HTTP/1.1)";
                    case 308: return "Permanent Redirect (RFC 7538)";
                    //case 308: return "Resume Incomplete (Google)";
                    case 400: return "Bad Request";
                    case 401: return "Unauthorized (RFC 7235)";
                    case 402: return "Payment Required";
                    case 403: return "Forbidden";
                    case 404: return "Not Found";
                    case 405: return "Method Not Allowed";
                    case 406: return "Not Acceptable";
                    case 407: return "Proxy Authentication Required (RFC 7235)";
                    case 408: return "Request Timeout";
                    case 409: return "Conflict";
                    case 410: return "Gone";
                    case 411: return "Length Required";
                    case 412: return "Precondition Failed (RFC 7232)";
                    case 413: return "Payload Too Large (RFC 7231)";
                    case 414: return "Request-URI Too Long";
                    case 415: return "Unsupported Media Type";
                    case 416: return "Requested Range Not Satisfiable (RFC 7233)";
                    case 417: return "Expectation Failed";
                    case 418: return "I'm a teapot (RFC 2324)";
                    case 419: return "Authentication Timeout (not in RFC 2616)";
                    case 420: return "Method Failure (Spring Framework)";
                    //case 420: return "Enhance Your Calm (Twitter)";
                    case 421: return "Misdirected Request (RFC 7540)";
                    case 422: return "Unprocessable Entity (WebDAV; RFC 4918)";
                    case 423: return "Locked (WebDAV; RFC 4918)";
                    case 424: return "Failed Dependency (WebDAV; RFC 4918)";
                    case 426: return "Upgrade Required";
                    case 428: return "Precondition Required (RFC 6585)";
                    case 429: return "Too Many Requests (RFC 6585)";
                    case 431: return "Request Header Fields Too Large (RFC 6585)";
                    case 440: return "Login Timeout (Microsoft)";
                    case 444: return "No Response (Nginx)";
                    case 449: return "Retry With (Microsoft)";
                    case 450: return "Blocked by Windows Parental Controls (Microsoft)";
                    //case 451: return "Unavailable For Legal Reasons (Internet draft)";
                    case 451: return "Redirect (Microsoft)";
                    case 494: return "Request Header Too Large (Nginx)";
                    case 495: return "Cert Error (Nginx)";
                    case 496: return "No Cert (Nginx)";
                    case 497: return "HTTP to HTTPS (Nginx)";
                    case 498: return "Token expired/invalid (Esri)";
                    case 499: return "Client Closed Request (Nginx)";
                    //case 499: return "Token required (Esri)";
                    case 500: return "Internal Server Error";
                    case 501: return "Not Implemented";
                    case 502: return "Bad Gateway";
                    case 503: return "Service Unavailable";
                    case 504: return "Gateway Timeout";
                    case 505: return "HTTP Version Not Supported";
                    case 506: return "Variant Also Negotiates (RFC 2295)";
                    case 507: return "Insufficient Storage (WebDAV; RFC 4918)";
                    case 508: return "Loop Detected (WebDAV; RFC 5842)";
                    case 509: return "Bandwidth Limit Exceeded (Apache bw/limited extension)[83]";
                    case 510: return "Not Extended (RFC 2774)";
                    case 511: return "Network Authentication Required (RFC 6585)";
                    case 520: return "Unknown Error";
                    case 522: return "Origin Connection Time-out";
                    case 598: return "Network read timeout error (Unknown)";
                    case 599: return "Network connect timeout error (Unknown)";
                    default: return String.Empty;
                }
            }
        }

        public List<Cookie> Cookies = new List<Cookie>();

        public String Connection { set; get; }

        public DateTime LastModified { set; get; }

        public String ContentType { set; get; }

        public String CharacterSet { set; get; }

        public byte[] Contents { set; get; }

        public int ContentLength { get { return (Contents != null) ? Contents.Length : 0; } }

        public HttpResponse()
        {
            this.ContentType = HttpCore.DefaultContentType;
            this.CharacterSet = HttpCore.DefaultCharSet;
            this.LastModified = DateTime.Now;
            this.Connection = "keep-alive";
        }
    }
}