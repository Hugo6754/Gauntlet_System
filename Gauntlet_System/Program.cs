using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Gauntlet_System
{
    internal class Program
    {
        public abstract class Participant
        {
            public string Username { get; private set; }
            public string Nationality { get; private set; }
            public int Elo { get; protected set; }
            public int Winstreak { get; protected set; }
            public bool Isactive { get; private set; }

            public Participant(string _username, string _nationality, int _elo, int _winstreak, bool _isActive)
            {
                Username = _username;
                Nationality = _nationality;
                Elo = _elo;
                Winstreak = _winstreak;
                Isactive = _isActive;
            }

            public abstract void CalculateNewElo(int opponent_elo, string result);
        }

        public class Player : Participant
        {
            public Player(string _username, string _nationality, int _elo, int _winstreak, bool _isActive) : base(_username, _nationality, _elo, _winstreak, _isActive)
            {
            }

            public override void CalculateNewElo(int opponent_elo, string result)
            {
                double expected_score;
                int new_elo;
                double outcome;
                if(result == "W")
                {
                    outcome = 1;
                } else if (result == "D")
                {
                    outcome = 0.5;
                } else
                {
                    outcome = 0;
                }


                expected_score = Convert.ToDouble(1 / (1 + Math.Pow(10,(opponent_elo - this.Elo) / 400.0)));

                new_elo = Convert.ToInt32(this.Elo + 32 * (outcome - expected_score));

                this.Elo = new_elo;
            }
        }

        public class GauntletPlayer : Participant
        {
            public GauntletPlayer(string _username, string _nationality, int _elo, int _winstreak, bool _isActive) : base(_username, _nationality, _elo, _winstreak, _isActive)
            {
               
            }
            public override void CalculateNewElo(int opponent_elo, string result)
            {
                double expected_score;
                int new_elo;
                double outcome;
                if (result == "W")
                {
                    outcome = 1;
                }
                else if (result == "D")
                {
                    outcome = 0.5;
                }
                else
                {
                    outcome = 0;
                }


                expected_score = Convert.ToDouble(1 / (1 + Math.Pow(10, (opponent_elo - this.Elo) / 400.0)));

                new_elo = Convert.ToInt32(this.Elo + 64 * (outcome - expected_score));

                this.Elo = new_elo;
            }
        }

        public class MatchRecord
        {
            public int wins { get; private set; }
            public int losses { get; private set; }
            public int draws { get; private set; }

            public void Recordresult(string _recordresult)
            {
                if(_recordresult == "W")
                {
                    wins++;
                } else if (_recordresult == "D")
                {
                    draws++;
                } else
                {
                    losses++;
                }
            }
        }
        static void Main(string[] args)
        {
            Player player = new Player("ObliVion", "RSA", 2000, 0, true);
            GauntletPlayer player1 = new GauntletPlayer("Mwetie", "RUS", 2000, 0, true); 

            player.CalculateNewElo(2001, "L");
            player1.CalculateNewElo(3500, "W");

            Console.WriteLine(player.Elo);
            Console.WriteLine(player1.Elo);
        }
    }
}
