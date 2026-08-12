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
        // Interfaces for upgrading/ downgrading players
        interface IUpgradePlayer
        {
            GauntletPlayer UpgradePlayer();
        }

        
        interface IDowngradePlayer
        {
            Player DowngradePlayer();
        }
        interface ICalculateElo
        {
            void CalculateNewElo(int opponent_elo, string result);
        }

        public abstract class Participant: ICalculateElo
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
                new_elo = Convert.ToInt32(this.Elo + 32 * (outcome - expected_score));
                this.Elo = new_elo;
            }

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
            Dictionary<string, Participant> participants = new Dictionary<string, Participant> 
            { { "ObliVion", new Player("ObliVion", "RSA", 2000, 4, true) },
                {"Mwetie", new GauntletPlayer("Mwetie", "RUS", 2000, -4, true) },
                { "Vortex", new Player("Vortex", "GER", 2100, 2, true)}
            };


            ProcessMatch(participants, "ObliVion", "W");

            foreach (var p in participants.Values)
            {
                Console.WriteLine($"Player: {p.Username} | Type: {p.GetType().Name} | Elo: {p.Elo} | Streak: {p.Winstreak}");
            }
        }

        static void ProcessMatch(Dictionary<string, Participant> registry, string challengerKey, string result)
        {
            if (!registry.ContainsKey(challengerKey)) return;

            Participant p1 = registry[challengerKey];

            // Match making logic for Gauntlet players
            int targetElo = p1.Elo;
            if (p1 is GauntletPlayer && p1.Winstreak > 0)
            {
                targetElo += (p1.Winstreak / 3) * 100;
            }

            //Candidate
            Participant p2 = registry.Values
                .Where(p => p.Isactive && p.Username != p1.Username)
                .OrderBy(p => Math.Abs(p.Elo - targetElo))
                .FirstOrDefault();

            if (p2 == null)
            {
                Console.WriteLine("[ERROR] No active opponents found.");
                return;
            }

            Console.WriteLine($"[MATCH START] {p1.Username} ({p1.GetType().Name}, Elo {p1.Elo}) vs {p2.Username} ({p2.GetType().Name}, Elo {p2.Elo})");

            // Update Elo and Streaks
            string p2Result = result == "W" ? "L" : (result == "L" ? "W" : "D");
            p1.CalculateNewElo(p2.Elo, result);
            p2.CalculateNewElo(p1.Elo, p2Result);

            //Upgrade/downgrades
            CheckAndSwapTier(registry, p1.Username);
            CheckAndSwapTier(registry, p2.Username);
        }

        static void CheckAndSwapTier(Dictionary<string, Participant> registry, string username)
        {
            if (!registry.ContainsKey(username)) return;

            // Check for Upgrade 
            if (registry[username].Winstreak >= 5 && registry[username] is IUpgradePlayer upgradable)
            {
                registry[username] = upgradable.UpgradePlayer();
                Console.WriteLine($"{username} reached 5 wins and UPGRADED to GauntletPlayer");
            }
            // Check for Downgrade 
            else if (registry[username].Winstreak <= -5 && registry[username] is IDowngradePlayer downgradable)
            {
                registry[username] = downgradable.DowngradePlayer();
                Console.WriteLine($"{username} dropped to -5 lossstreak and DEMOTED to Player");
            }
        }
    }
}