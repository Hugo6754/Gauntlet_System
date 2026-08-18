using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gauntlet_System.Interfaces
{
    interface ITier // Interface for Calculating elo
   {
        void CalculateNewElo(int opponent_elo, string result);
    }
}
