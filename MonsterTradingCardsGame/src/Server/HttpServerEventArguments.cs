using System;
using System.Collections.Specialized;
using System.Net.Sockets;
using System.Text;
using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Server
{
    /// <summary>
    /// This class provides HTTP server event arguments, handling the parsing
    /// of HTTP requests and sending replies.
    /// </summary>
    public class HttpServerEventArguments : EventArgs
    {
        protected TcpClient _Client; // TCP client for the current connection

        // Constructor
        public HttpServerEventArguments(TcpClient client, string plainMessage)
        {
            _Client = client;
            PlainMessage = plainMessage;
            Payload = string.Empty;
            QueryParameters = new Dictionary<string, string>();
            ParseHttpRequest(plainMessage);
        }

        // Public properties
        public string Method { get; private set; } = string.Empty;
        public string Path { get; private set; } = string.Empty;
        public Dictionary<string, string> QueryParameters { get; private set; }
        public List<HttpHeader> Headers { get; private set; } = new List<HttpHeader>();
        public string Payload { get; private set; } = string.Empty;
        public string PlainMessage { get; private set; }
        public bool ResponseSent { get; private set; } = false;
        public int ResponseStatusCode { get; set; }
        public User User { get; set; }

        /// <summary>
        /// Sends a reply to the client.
        /// </summary>
        /// <param name="status">HTTP status code of the response.</param>
        /// <param name="payload">Optional payload to include in the response.</param>
        public virtual void Reply(int status, string? payload = null)
        {
            this.ResponseStatusCode = status;
            string responseHeader = BuildResponseHeader(status, payload);
            byte[] buffer = Encoding.ASCII.GetBytes(responseHeader);

            _Client.GetStream().Write(buffer, 0, buffer.Length);
            ResponseSent = true;

            CloseClientConnection();
        }

        // Private helper methods

        protected virtual void CloseClientConnection()
        {
            _Client.Close();
            _Client.Dispose();
        }

        private string BuildResponseHeader(int status, string? payload)
        {
            var responseBuilder = new StringBuilder();
            responseBuilder.AppendLine($"HTTP/1.1 {status} {GetStatusMessage(status)}");
            responseBuilder.AppendLine("Content-Type: text/plain");

            if (string.IsNullOrEmpty(payload))
            {
                responseBuilder.AppendLine("Content-Length: 0");
            }
            else
            {
                responseBuilder.AppendLine($"Content-Length: {Encoding.ASCII.GetByteCount(payload)}");
                responseBuilder.AppendLine();
                responseBuilder.Append(payload);
            }

            return responseBuilder.ToString();
        }

        private string GetStatusMessage(int status)
        {
            return status switch
            {
                200 => "OK",
                400 => "Bad Request",
                404 => "Not Found",
                409 => "Conflict",
                _ => "Service Unavailable",
            };
        }

        private void ParseHttpRequest(string plainMessage)
        {
            string[] lines = plainMessage.Replace("\r\n", "\n").Split('\n');
            ParseRequestLine(lines[0]);

            int payloadStartIndex = Array.FindIndex(lines, 1, line => string.IsNullOrWhiteSpace(line)) + 1;
            ParseHeaders(lines, payloadStartIndex);
            Payload = BuildPayload(lines, payloadStartIndex);
        }

        private void ParseRequestLine(string requestLine)
        {
            string[] parts = requestLine.Split(' ');
            if (parts.Length >= 2)
            {
                Method = parts[0];
                var fullPath = parts[1].Split('?');
                Path = fullPath[0];

                if (fullPath.Length > 1)
                {
                    ParseQueryString(fullPath[1]);
                }
            }
            else
            {
                // TODO: Handle invalid request line
            }
        }

        private void ParseQueryString(string queryString)
        {
            var queries = queryString.Split('&');
            foreach (var query in queries)
            {
                var pair = query.Split('=');
                if (pair.Length == 2)
                {
                    QueryParameters[pair[0]] = pair[1];
                }
                // TODO: Handle cases where the query string is not valiid
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
    }
}
