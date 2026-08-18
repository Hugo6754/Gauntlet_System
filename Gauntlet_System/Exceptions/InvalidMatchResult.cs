using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gauntlet_System.Exceptions
{
    public class InvalidMatchResultException : Exception
    {
        public InvalidMatchResultException(string message)
            : base(message)
        {
        }
    }
}
