using Gauntlet_System.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gauntlet_System.Events
{
    public static class EventManager
    {
        public static event StreakThresholdReachedHandler StreakThresholdReached;//Event for Streak 

        public static event MatchCompletedHandler MatchCompleted;//Event for completed match 

        public static void TriggerMatchCompleted(Participant p1, Participant p2, string result)
        {
            MatchCompleted?.Invoke(p1, p2, result);
        }

        // Add this method to allow other files to trigger the Streak event
        public static void TriggerStreakThresholdReached(Participant player)
        {
            StreakThresholdReached?.Invoke(player);
        }
    }
}
