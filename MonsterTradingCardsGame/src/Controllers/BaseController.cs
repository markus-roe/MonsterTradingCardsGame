
namespace MonsterTradingCardsGame.Controllers
{
    public abstract class BaseController
    {

        public enum Response
        {
            Success,
            UsernameAlreadyExists,
            InvalidUsername,
            InvalidPassword
        }

    }
}
