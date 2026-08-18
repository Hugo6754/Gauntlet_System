using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gauntlet_System.Classes
{
    public class GauntletPlayer : Participant
    {
        public GauntletPlayer(string _username, string _nationality, int _elo, int _winstreak, bool _isActive)
            : base(_username, _nationality, _elo, _winstreak, _isActive)
        {
        }

        public override void CalculateNewElo(int opponent_elo, string result)  // Will use the same caculations but will increase the k-factor 
        {
            double expected_score;
            int new_elo;
            double outcome;

            if (result == "W")
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

            expected_score = Convert.ToDouble(1 / (1 + Math.Pow(10, (opponent_elo - this.Elo) / 400.0)));

            double k_factor = 32.0; // Normal elo k-factor 

            if (result == "W")
            {
                double exponent = (opponent_elo - this.Elo) / 400.0; // calculates a exponent to use to increase the k-factor using elo as referance 
                k_factor = exponent * k_factor; //creates new k-factor 
            }

            new_elo = Convert.ToInt32(this.Elo + k_factor * (outcome - expected_score));

            this.Elo = new_elo; //Reasigns elo 
                                // Interestingly when facing a high elo player the actual outcome will be so minimal it will not change the elo of the lower rated player if they lose 
        }

        public override Participant UpgradePlayer()
        {
            return this;
        }

        public override Participant DowngradePlayer()
        {
            return new Player(this.Username, this.Nationality, this.Elo, 0, this.Isactive);
        }
    }
}
