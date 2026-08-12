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
        //Inferfaces for upgrading / downgrading players
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
            Dictionary<string, Participant> participants = new Dictionary<string, Participant>();

            participants.Add("ObliVion", new Player("ObliVion", "RSA", 1000, 4, true)); 
            participants.Add("Novice_Joe", new Player("Novice_Joe", "USA", 1000, 0, true));
            participants.Add("Mid_Vortex", new Player("Mid_Vortex", "GER", 1120, 2, true));
            participants.Add("High_Kratos", new GauntletPlayer("High_Kratos", "GRE", 1250, 6, true));


            Console.WriteLine("=== SIMULATION START ===");
            Console.WriteLine($"Player: ObliVion | Type: {participants["ObliVion"].GetType().Name} | Elo: {participants["ObliVion"].Elo} | Streak: {participants["ObliVion"].Winstreak}\n");

            Console.WriteLine("--- Match 1: Searching opponent for ObliVion... ---");
            ProcessMatch(participants, "ObliVion", "W");

            Console.WriteLine($"\nPost-Match Status:");
            Console.WriteLine($"ObliVion Type: {participants["ObliVion"].GetType().Name} | Elo: {participants["ObliVion"].Elo} | Streak: {participants["ObliVion"].Winstreak}\n");

            // --- Match 2: ObliVion is now a GauntletPlayer (Streak = 0) ---
            Console.WriteLine("--- Match 2: Searching opponent for ObliVion (as GauntletPlayer)... ---");
            ProcessMatch(participants, "ObliVion", "W");

            // Manually simulate winstreak up to 5 in Gauntlet tier to demonstrate handicap jump
            for (int i = 0; i < 4; i++)
            { 
                ProcessMatch(participants, "ObliVion", "W");
            }

            Console.WriteLine($"\nPost-Match Status:");
            Console.WriteLine($"ObliVion Type: {participants["ObliVion"].GetType().Name} | Elo: {participants["ObliVion"].Elo} | Streak: {participants["ObliVion"].Winstreak}\n");

            // --- Match 7: ObliVion is GauntletPlayer with Streak = 5 ---
            // Target Elo = ObliVion's Elo + (5/5 * 100) = ~1160 + 100 = 1260 Elo.
            // Matchmaker should skip lower Elo players and pull High_Kratos (1250 Elo) or Boss_Mwetie!
            Console.WriteLine("--- Match 7: Searching opponent with +100 Gauntlet Handicap... ---");
            ProcessMatch(participants, "ObliVion", "W");

            Console.WriteLine($"\n=== FINAL STATUS ===");
            Console.WriteLine($"ObliVion Type: {participants["ObliVion"].GetType().Name} | Elo: {participants["ObliVion"].Elo} | Streak: {participants["ObliVion"].Winstreak}");
        }

        static Participant FindOpponent(Dictionary<string, Participant> registry, Participant challenger)
        {
            const int MAX_CEILING = 10000;
            int targetElo = challenger.Elo;
            //Matchmaking for Gauntlet players
            //Finds players with +100 elo for Gauntlet player with streaks
            if (challenger is GauntletPlayer && challenger.Winstreak > 0)
            {
                 
                targetElo += ((challenger.Winstreak / 5) * 100);

                //settles to the highest elo
                if (targetElo > MAX_CEILING)
                {
                    targetElo = MAX_CEILING;
                }
            }

            Console.WriteLine($"[MATCHMAKER] Searching for opponent near Target Elo: {targetElo} (Base Elo: {challenger.Elo}, Streak: {challenger.Winstreak})");
            //Matchmaking for Players
            int delta = 25;
            int maxDelta = 100;
            Participant candidate = null;

            // Expanding search window loop
            while (candidate == null && delta <= maxDelta)
            {
                int minElo = targetElo - delta;
                int maxElo = targetElo + delta;

               //filters players
                var candidates = registry.Values.Where(p =>
                    p.Isactive &&
                    p.Username != challenger.Username &&
                    p.Elo >= minElo &&
                    p.Elo <= maxElo
                ).ToList();

                if (candidates.Count > 0)
                {
                    // Select the candidate closest to targetElo
                    candidate = candidates.OrderBy(p => Math.Abs(p.Elo - targetElo)).First();
                    Console.WriteLine($"[MATCHMAKER] Match Found! Opponent: {candidate.Username} (Elo: {candidate.Elo}) within bracket [±{delta}]");
                    return candidate;
                }

                // Expand window
                delta += 25;
            }

            // Fallback for candidates above maxDelta
            var fallback = registry.Values
                .Where(p => p.Isactive && p.Username != challenger.Username)
                .OrderBy(p => Math.Abs(p.Elo - targetElo))
                .FirstOrDefault();

            if (fallback != null)
            {
                Console.WriteLine($"[MATCHMAKER] Fallback Match Found: {fallback.Username} (Elo: {fallback.Elo})");
            }

            return fallback;
        }

        //Match logic
        static void ProcessMatch(Dictionary<string, Participant> registry, string challengerKey, string challengerResult)
        {
            if (!registry.ContainsKey(challengerKey))
            {
                Console.WriteLine("[ERROR] Challenger key not found in registry.");
                return;
            }

            Participant p1 = registry[challengerKey];

            // Auto-select opponent using expanding window algorithm
            Participant p2 = FindOpponent(registry, p1);

            if (p2 == null)
            {
                Console.WriteLine("[ERROR] No active opponents available for matchmaking.");
                return;
            }

            //Match logic
            string p2Result = challengerResult == "W" ? "L" : (challengerResult == "L" ? "W" : "D");

            // Calculate Elo and Streaks
            p1.CalculateNewElo(p2.Elo, challengerResult);
            p2.CalculateNewElo(p1.Elo, p2Result);

            // Promotion and Demotion check
            CheckAndSwapTier(registry, p1.Username);
            CheckAndSwapTier(registry, p2.Username);
        }

 
        static void CheckAndSwapTier(Dictionary<string, Participant> registry, string username)
        {
            if (!registry.ContainsKey(username)) return;

            Participant p = registry[username];

            
            if (p.Winstreak >= 5 && p is IUpgradePlayer upgradable)
            {
                registry[username] = upgradable.UpgradePlayer();
                Console.WriteLine($"{username} reached 5 wins and UPGRADED to GauntletPlayer.");
            }
            
            else if (p.Winstreak <= -5 && p is IDowngradePlayer downgradable)
            {
                registry[username] = downgradable.DowngradePlayer();
                Console.WriteLine($"{username} dropped to -5 streak and DEMOTED to Player");
            }
        }
    }
}