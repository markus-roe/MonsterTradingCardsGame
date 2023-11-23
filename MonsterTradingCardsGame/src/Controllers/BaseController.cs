using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
