using MonsterTradingCardsGame.Models;

namespace MonsterTradingCardsGame.Server
{

    public interface IHttpServerEventArguments
    {
        string Method { get; }
        string Path { get; }
        Dictionary<string, string> QueryParameters { get; }
        List<HttpHeader> Headers { get; }
        string Payload { get; }
        User User { get; set; }

        int ResponseStatusCode { get; set; }

        void Reply(int status, string? payload = null);

    }

}