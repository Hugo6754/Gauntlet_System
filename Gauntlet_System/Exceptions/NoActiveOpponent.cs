using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gauntlet_System.Exceptions
{
    public class NoActiveOpponentException : Exception
    {
        public NoActiveOpponentException(string message)
            : base(message)
        {
        }
    }
}
