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
        //Inferfaces that promote and demote players
        interface IUpgradePlayer
        {
            GauntletPlayer UpgradePlayer();
        }

        interface IDowngradePlayer
        {
            Player DowngradePlayer();
        }

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

        public class Player : Participant, IUpgradePlayer
        {
            public Player(string _username, string _nationality, int _elo, int _winstreak, bool _isActive)
                : base(_username, _nationality, _elo, _winstreak, _isActive)
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
                    // If player wins, increment winstreak, else reset to 1
                    Winstreak = Winstreak > 0 ? Winstreak + 1 : 1;
                }
                else if (result == "D")
                {
                    outcome = 0.5;
                }
                else
                {
                    outcome = 0;
                    // If player loses, decrement lossstreak
                    Winstreak = Winstreak < 0 ? Winstreak - 1 : -1;
                }

                expected_score = Convert.ToDouble(1 / (1 + Math.Pow(10, (opponent_elo - this.Elo) / 400.0)));
                new_elo = Convert.ToInt32(this.Elo + 32 * (outcome - expected_score));
                this.Elo = new_elo;
            }

            // Upgrade player
            public GauntletPlayer UpgradePlayer()
            {
                return new GauntletPlayer(this.Username, this.Nationality, this.Elo, 0, this.Isactive);
            }
        }

        public class GauntletPlayer : Participant, IDowngradePlayer
        {
            public GauntletPlayer(string _username, string _nationality, int _elo, int _winstreak, bool _isActive)
                : base(_username, _nationality, _elo, _winstreak, _isActive)
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
                new_elo = Convert.ToInt32(this.Elo + 64 * (outcome - expected_score));
                this.Elo = new_elo;
            }

            // Downgrade player
            public Player DowngradePlayer()
            {
                return new Player(this.Username, this.Nationality, this.Elo, 0, this.Isactive);
            }
        }

        public class MatchRecord
        {
            public int wins { get; private set; }
            public int losses { get; private set; }
            public int draws { get; private set; }

            public void Recordresult(string _recordresult)
            {
                if (_recordresult == "W")
                {
                    wins++;
                }
                else if (_recordresult == "D")
                {
                    draws++;
                }
                else
                {
                    losses++;
                }
            }
        }

        static void Main(string[] args)
        {
            Dictionary<string, Participant> participants = new Dictionary<string, Participant>();
            Dictionary<string, MatchRecord> matchRecord = new Dictionary<string, MatchRecord>();

            // Setup initial state:
            // ObliVion starts at streak 4 (1 win away from upgrade)
            // Mwetie starts at streak -4 (1 loss away from downgrade)
            participants.Add("ObliVion", new Player("ObliVion", "RSA", 2000, 4, true));
            participants.Add("Mwetie", new GauntletPlayer("Mwetie", "RUS", 2000, -4, true));

            Console.WriteLine("--- Match 1: ObliVion wins against Mwetie ---");
            ProcessMatch(participants, "ObliVion", "Mwetie", "W");

            Console.WriteLine("\n--- Match 2: ObliVion loses to Mwetie ---");
            ProcessMatch(participants, "ObliVion", "Mwetie", "L");

            Console.WriteLine("\n--- Final Participant Status ---");
            Console.WriteLine($"ObliVion Type: {participants["ObliVion"].GetType().Name}, Elo: {participants["ObliVion"].Elo}, Streak: {participants["ObliVion"].Winstreak}");
            Console.WriteLine($"Mwetie  Type: {participants["Mwetie"].GetType().Name}, Elo: {participants["Mwetie"].Elo}, Streak: {participants["Mwetie"].Winstreak}");
        }
        //Evaluate player tier
        static void CheckAndSwapTier(Dictionary<string, Participant> registry, string username)
        {
            if (!registry.ContainsKey(username)) return;

            Participant p = registry[username];

            
            if (p.Winstreak >= 5 && p is IUpgradePlayer upgradable)
            {
                registry[username] = upgradable.UpgradePlayer();
                Console.WriteLine($"{username} reached 5 wins and upgraded to GauntletPlayer");
            }
            
            else if (p.Winstreak <= -5 && p is IDowngradePlayer downgradable)
            {
                registry[username] = downgradable.DowngradePlayer();
                Console.WriteLine($"{username} dropped to 5 losses and demoted to Player");
            }
        }
        //Logic to analyze match results and update Elo
        static void ProcessMatch(Dictionary<string, Participant> registry, string player1Key, string player2Key, string p1Result)
        {
            
            if (!registry.ContainsKey(player1Key) || !registry.ContainsKey(player2Key))
            {
                Console.WriteLine("[ERROR] One or both players not found in registry.");
                return;
            }

            Participant p1 = registry[player1Key];
            Participant p2 = registry[player2Key];

            
            string p2Result = p1Result == "W" ? "L" : (p1Result == "L" ? "W" : "D");

            
            p1.CalculateNewElo(p2.Elo, p1Result);
            p2.CalculateNewElo(p1.Elo, p2Result);

            
            CheckAndSwapTier(registry, player1Key);
            CheckAndSwapTier(registry, player2Key);
        }
    }
}