using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Gauntlet_System
{
    internal class Program
    {
        // Composite Interfaces defining core player behaviors and progression
        public interface ITier
        {
            void CalculateNewElo(int opponent_elo, string result);
        }

        public interface ITierPromotable
        {
            Participant GetNextTier();
            Participant GetPreviousTier();
        }

        // Custom Exceptions
        public class PlayerNotFoundException : Exception
        {
            public PlayerNotFoundException(string message)
                : base(message)
            {
            }
        }

        public class InvalidMatchResultException : Exception
        {
            public InvalidMatchResultException(string message)
                : base(message)
            {
            }
        }

        public class NoActiveOpponentException : Exception
        {
            public NoActiveOpponentException(string message)
                : base(message)
            {
            }
        }

        //Delegates
        public delegate void StreakThresholdReachedHandler(Participant player);//Delegate for Streak
        //Delegate for a completed match
        public delegate void MatchCompletedHandler(
            Participant player1,
            Participant player2,
            string result);

        // Events
        public static event StreakThresholdReachedHandler StreakThresholdReached;//Event for Streak

        public static event MatchCompletedHandler MatchCompleted;//Event for completed match

        //Subscribers
        static void OnStreakThresholdReached(Participant player)//Subscriber for Streak 
        {
            Console.WriteLine(
                $"[GAUNTLET] {player.Username} reached a 3-win streak and triggered a Gauntlet pairing!");
        }

        static void OnMatchCompleted(
            Participant player1,
            Participant player2,
            string result)
        {
            Console.WriteLine(
                $"[MATCH COMPLETED] {player1.Username} vs {player2.Username} | Result: {result}");
        }//Subscriber for completed match

        public abstract class Participant : ITier, ITierPromotable
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
            //Derived from the interfaces
            public abstract void CalculateNewElo(int opponent_elo, string result); 
            public abstract Participant GetNextTier();
            public abstract Participant GetPreviousTier();
        }

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

            public override Participant GetNextTier()
            {
                return new Gauntlet(this.Username, this.Nationality, this.Elo, 0, this.Isactive);
            }

            public override Participant GetPreviousTier()
            {
                return this; // Cannot downgrade below standard Player for now
            }
        }

        public class Gauntlet : Participant
        {
            public Gauntlet(string _username, string _nationality, int _elo, int _winstreak, bool _isActive)
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
                    double expononent = (opponent_elo - this.Elo) / 400; // calculates a exponent to use to increase the k-factor using elo as referance
                    k_factor = expononent * k_factor; //creates new k-factor
                }

                new_elo = Convert.ToInt32(this.Elo + k_factor * (outcome - expected_score));

                this.Elo = new_elo; //Reasigns elo
                // Interestingly when facing a high elo player the actual outcome will be so minimal it will not change the elo of the lower rated player if they lose
            }

            public override Participant GetNextTier()
            {
                return this; //no tier above gauntlet for now
            }

            public override Participant GetPreviousTier()
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
            StreakThresholdReached += OnStreakThresholdReached;// Subscribes the event to the method
            MatchCompleted += OnMatchCompleted;// Subscribes the event to the method

            Dictionary<string, Participant> participants = new Dictionary<string, Participant>
            {
                { "ObliVion", new Player("ObliVion", "RSA", 2000, 4, true) },
                { "Mwetie", new Player("Mwetie", "RUS", 2000, -4, true) },
                { "Vortex", new Player("Vortex", "GER", 2100, 2, true) }
            };

            try
            {
                StartMatch(participants, "ObliVion", "W");
            }

            catch (PlayerNotFoundException ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
            catch (InvalidMatchResultException ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
            catch (NoActiveOpponentException ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UNEXPECTED ERROR] {ex.Message}");
            }

            foreach (var p in participants.Values)
            {
                Console.WriteLine($"Player: {p.Username} | Type: {p.GetType().Name} | Elo: {p.Elo} | Streak: {p.Winstreak}");
            }
        }

        static void StartMatch(Dictionary<string, Participant> registry, string challengerKey, string result)
        {
            if (!registry.ContainsKey(challengerKey))
            {
                throw new PlayerNotFoundException(
                    $"Player '{challengerKey}' was not found.");//Added throw for when the player is not found in the registry
            }

            Participant p1 = registry[challengerKey];

            // Match making logic for Gauntlet players
            int targetElo = p1.Elo;
            if (p1 is Gauntlet && p1.Winstreak > 0)
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
                throw new NoActiveOpponentException(
                    "No active opponents are currently available.");
            }

            ExecuteMatch(registry, p1, p2, result);
        }

        static void ExecuteMatch(Dictionary<string, Participant> registry, Participant p1, Participant p2, string result)
        {
            Console.WriteLine($"[MATCH START] {p1.Username} ({p1.GetType().Name}, Elo {p1.Elo}) vs {p2.Username} ({p2.GetType().Name}, Elo {p2.Elo})");

            // Update Elo and Streaks
            string p2Result = result == "W" ? "L" : (result == "L" ? "W" : "D");
            // Validate result input
            result = result.ToUpper();

            if (result != "W" && result != "L" && result != "D")
            {
                throw new InvalidMatchResultException(
                    "Invalid match result. Please enter W, L, or D.");
            } // Validate result input

            p1.CalculateNewElo(p2.Elo, result);
            p2.CalculateNewElo(p1.Elo, p2Result);

            //Upgrade/downgrades
            CheckAndSwapTier(registry, p1.Username);
            CheckAndSwapTier(registry, p2.Username);

            // Match has now completed
            MatchCompleted?.Invoke(p1, p2, result);
        }

        static void CheckAndSwapTier(Dictionary<string, Participant> registry, string username)
        {
            if (!registry.ContainsKey(username)) return;

            Participant current = registry[username];

            // Check for Upgrade 
            if (current.Winstreak >= 5)
            {
                Participant upgraded = current.GetNextTier();
                if (upgraded != current)
                {
                    registry[username] = upgraded;
                    Console.WriteLine($"{username} reached 3 wins and UPGRADED to {upgraded.GetType().Name}");
                }
            }
            // Check for Downgrade 
            else if (current.Winstreak <= -5)
            {
                Participant downgraded = current.GetPreviousTier();
                if (downgraded != current)
                {
                    registry[username] = downgraded;
                    Console.WriteLine($"{username} dropped to -3 lossstreak and DEMOTED to {downgraded.GetType().Name}");
                }
            }
        }
    }
}