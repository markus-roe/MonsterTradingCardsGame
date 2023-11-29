using MonsterTradingCardsGame.Server;

namespace MonsterTradingCardsGame.Middleware
{
    public interface IMiddleware
    {
        void Invoke(HttpServerEventArguments httpEventArguments);
    }

}
