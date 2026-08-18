using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gauntlet_System.Program;

namespace Gauntlet_System.Classes
{
    public class Player : Participant
    {
        public Player(string _username, string _nationality, int _elo, int _winstreak, bool _isActive)
            : base(_username, _nationality, _elo, _winstreak, _isActive) // Takes the base from the abstract class 
        {
        }

        public override void CalculateNewElo(int opponent_elo, string result) // Normal calculations for chess elo 
        {
            double expected_score;
            int new_elo;
            double outcome;

            if (result == "W") // Setting the outcome of the match as ths will impact the amount of elo gained or lost 
            {
                outcome = 1;
                Winstreak = Winstreak > 0 ? Winstreak + 1 : 1;
            }
            else if (result == "D")
            {
                outcome = 0.5;
            }
            else
            {
                outcome = 0;
                Winstreak = Winstreak < 0 ? Winstreak - 1 : -1;
            }

            expected_score = Convert.ToDouble(1 / (1 + Math.Pow(10, (opponent_elo - this.Elo) / 400.0)));  // Calculates the probability of the player to win, using their elos as refrences 

            new_elo = Convert.ToInt32(this.Elo + 32 * (outcome - expected_score));  // Multiplies the actual outcome (the outcome minus the expected) by a set k-factor to increase or decrease the elo by a set minimal amount 

            this.Elo = new_elo; //reasigns the new elo 
        }

        public override Participant UpgradePlayer()
        {
            return new GauntletPlayer(this.Username, this.Nationality, this.Elo, 0, this.Isactive);
        }

        public override Participant DowngradePlayer()
        {
            return this;
        }
    }
}
