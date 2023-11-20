using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace FHTW.Swen1.Swamp
{
    /// <summary>This class provides HTTP server event arguments.</summary>
    public class HttpSvrEventArgs : EventArgs
    {
        // Protected member: TCP client
        protected TcpClient _Client;

        // Constructor
        public HttpSvrEventArgs(TcpClient client, string plainMessage)
        {
            _Client = client;
            PlainMessage = plainMessage;
            Payload = string.Empty;

            // Process the plain message to extract details
            string[] lines = plainMessage.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            ParseHttpRequest(lines);
        }

        // Public properties
        public virtual string Method { get; protected set; } = string.Empty;
        public virtual string Path { get; protected set; } = string.Empty;
        public virtual List<HttpHeader> Headers { get; protected set; } = new List<HttpHeader>();
        public virtual string Payload { get; protected set; } = string.Empty;
        public string PlainMessage { get; protected set; }

        // Reply method
        public virtual void Reply(int status, string? payload = null)
        {
            string data;

            switch (status)
            {
                case 200:
                    data = "HTTP/1.1 200 OK\n"; break;
                case 400:
                    data = "HTTP/1.1 400 Bad Request\n"; break;
                case 404:
                    data = "HTTP/1.1 404 Not Found\n"; break;
                default:
                    data = "HTTP/1.1 503 Service Unavailable\n"; 
                    payload = "Sorry, can't process your request. I'm currently busy contemplating the meaning of the universe."; 
                    break;
            }

            if (string.IsNullOrEmpty(payload))
            {
                data += "Content-Length: 0\n";
            }
            else
            {
                data += "Content-Length: " + Encoding.ASCII.GetByteCount(payload) + "\n";
            }
            data += "Content-Type: text/plain\n\n";

            if (!string.IsNullOrEmpty(payload)) { data += payload; }

            byte[] buf = Encoding.ASCII.GetBytes(data);
            _Client.GetStream().Write(buf, 0, buf.Length);
            _Client.Close();
            _Client.Dispose();
        }

        // Private methods for parsing HTTP request
        private void ParseRequestLine(string requestLine)
        {
            string[] parts = requestLine.Split(' ');
            if (parts.Length >= 2)
            {
                Method = parts[0];
                Path = parts[1];
            }
            else
            {
                // Handle invalid request line
            }
        }

        private void ParseHeaders(string[] lines, int endIndex)
        {
            for (int i = 1; i < endIndex; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    Headers.Add(new HttpHeader(lines[i]));
                }
            }
        }

        private string BuildPayload(string[] lines, int startIndex)
        {
            StringBuilder payloadBuilder = new StringBuilder();
            for (int i = startIndex; i < lines.Length; i++)
            {
                if (i > startIndex) payloadBuilder.AppendLine();
                payloadBuilder.Append(lines[i]);
            }
            return payloadBuilder.ToString();
        }

        private void ParseHttpRequest(string[] lines)
        {
            ParseRequestLine(lines[0]);

            int payloadStartIndex = Array.FindIndex(lines, 1, line => string.IsNullOrWhiteSpace(line)) + 1;
            ParseHeaders(lines, payloadStartIndex);
            Payload = BuildPayload(lines, payloadStartIndex);
        }
    }
}
