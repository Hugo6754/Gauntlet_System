using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gauntlet_System.Exceptions
{
    public class InvalidPlayerDataException : Exception
    {
        public InvalidPlayerDataException(string message)
            : base(message)
        {
        }
    }
}
