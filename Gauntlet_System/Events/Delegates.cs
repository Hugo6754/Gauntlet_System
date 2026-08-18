using Gauntlet_System.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gauntlet_System.Events
{

    //Delegates 

    public delegate void StreakThresholdReachedHandler(Participant player);//Delegate for Streak 

    //Delegate for a completed match 

    public delegate void MatchCompletedHandler(
        Participant player1,
        Participant player2,
        string result);
}
