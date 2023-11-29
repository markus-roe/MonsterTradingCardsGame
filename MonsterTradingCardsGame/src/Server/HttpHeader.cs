using System;

namespace MonsterTradingCardsGame.Server
{
    /// <summary>This class represents a HTTP header.</summary>
    public class HttpHeader
    {
        /// <summary>Creates a new instance of this class.</summary>
        /// <param name="header">Header text.</param>
        public HttpHeader(string header)
        {
            Key = Value = string.Empty;

            try
            {
                int n = header.IndexOf(':');
                Key = header.Substring(0, n).Trim();
                Value = header.Substring(n + 1).Trim();
            }
            catch (Exception) { }
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the header name.</summary>
        public string Key
        {
            get; protected set;
        }


        /// <summary>Gets the header value.</summary>
        public string Value
        {
            get; protected set;
        }
    }
}
