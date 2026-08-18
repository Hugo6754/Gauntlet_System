using System;

using System.Collections.Generic;

using System.Linq;

using System.Security.Cryptography.X509Certificates;

using System.Text;

using System.Threading.Tasks;

using System.Threading;

using System.Text.Json;

using System.IO;

using System.Runtime.InteropServices;



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



        public abstract class Participant : ICalculateElo // Abtract class so any type of player inherits these properties 

        {

            public string Username { get; private set; }

            public string Nationality { get; private set; }

            public int Elo { get; protected set; }

            public int Winstreak { get; protected set; }

            public bool Isactive { get; private set; }



            public void SetActive(bool isActive)   // Active players 

            {

                Isactive = isActive;

            }



            public Participant(string _username, string _nationality, int _elo, int _winstreak, bool _isActive) // Constructer for the class 

            {

                Username = _username;

                Nationality = _nationality;

                Elo = _elo;

                Winstreak = _winstreak;

                Isactive = _isActive;

            }



            public abstract void CalculateNewElo(int opponent_elo, string result); //Function they need to inherit could also be inherited from a interface 

        }



        public class Player : Participant, IUpgradePlayer

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



            public Player DowngradePlayer()

            {

                return new Player(this.Username, this.Nationality, this.Elo, 0, this.Isactive);

            }

        }



        public class PlayerRecord

        {

            public string Type { get; set; }

            public string Username { get; set; }

            public string Nationality { get; set; }

            public int Elo { get; set; }

            public int Winstreak { get; set; }

            public bool Isactive { get; set; }

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



        private static readonly object RegistryLock = new object(); // Lock object for thread safety 



        //Win-streak threshold that triggers a gauntlet pairing 

        private const int StreakThreshold = 3;



        private static readonly Random Rng = new Random(); // Random number generator 



        private static volatile bool _monitorRunning = true; // Flag to control the monitoring thread 



        static void Main(string[] args)

        {

            StreakThresholdReached += OnStreakThresholdReached;// Subscribes the event to the method 

            MatchCompleted += OnMatchCompleted;// Subscribes the event to the method 



            Dictionary<string, Participant> participants = new Dictionary<string, Participant>

            { { "ObliVion", new Player("ObliVion", "RSA", 2000, 4, true) },

                {"Mwetie", new GauntletPlayer("Mwetie", "RUS", 2000, -4, true) },

                { "Vortex", new Player("Vortex", "GER", 2100, 2, true)}

            };



            // Starts the thread to monitor the gauntlet 

            Thread monitorThread = new Thread(() => MatchmakingMonitor(participants))

            {

                IsBackground = true

            };

            monitorThread.Start();



            /*try 

            { 

                ProcessMatch(participants, "ObliVion", "W"); 

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

            }*/





            bool running = true;

            while (running)

            {

                ShowMenu();

                string choice = Console.ReadLine();



                switch (choice)

                {

                    case "1":

                        ViewAllPlayers(participants);

                        break;

                    case "2":

                        AddPlayer(participants);

                        break;

                    case "3":

                        TriggerMatch(participants);

                        break;

                    case "4":

                        ToggleActive(participants);

                        break;

                    case "5":

                        Console.Write("File path to save to (e.g. roster.json): ");

                        SaveRoster(participants, Console.ReadLine());

                        break;

                    case "6":

                        Console.Write("File path to load from: ");

                        LoadRoster(participants, Console.ReadLine());

                        break;

                    case "0":

                        running = false;

                        break;

                    default:

                        Console.WriteLine("Invalid option, try again.");

                        break;

                }

            }

            // Stop the background monitor once the user exits the menu 

            _monitorRunning = false;

            monitorThread.Join();



        }



        static void ViewAllPlayers(Dictionary<string, Participant> registry)

        {

            lock (RegistryLock)

            {

                if (registry.Count == 0)

                {

                    Console.WriteLine("No players registered yet.");

                    return;

                }



                foreach (var p in registry.Values)

                {

                    string status = p.Isactive ? "Active" : "Suspended";

                    Console.WriteLine($"{p.Username} | {p.GetType().Name} | Elo: {p.Elo} | Streak: {p.Winstreak} | {status} | {p.Nationality}");

                }

            }

        }



        static void AddPlayer(Dictionary<string, Participant> registry)

        {

            Console.Write("Username: ");

            string username = Console.ReadLine();



            lock (RegistryLock)

            {

                if (registry.ContainsKey(username))

                {

                    Console.WriteLine($"A player named '{username}' already exists.");

                    return;

                }

            }



            Console.Write("Nationality: ");

            string nationality = Console.ReadLine();



            Console.Write("Starting Elo: ");

            if (!int.TryParse(Console.ReadLine(), out int elo))

            {

                Console.WriteLine("Invalid Elo — must be a number. Player not added.");

                return;

            }



            var newPlayer = new Player(username, nationality, elo, 0, true);



            lock (RegistryLock)

            {

                registry[username] = newPlayer;

            }



            Console.WriteLine($"{username} added as a new Player.");

        }



        static void TriggerMatch(Dictionary<string, Participant> registry)

        {

            Console.Write("Challenger username: ");

            string username = Console.ReadLine();



            Console.Write("Result for this player (W/L/D): ");

            string result = Console.ReadLine();



            try

            {

                ProcessMatch(registry, username, result);

            }

            catch (PlayerNotFoundException ex) { Console.WriteLine($"[ERROR] {ex.Message}"); }

            catch (InvalidMatchResultException ex) { Console.WriteLine($"[ERROR] {ex.Message}"); }

            catch (NoActiveOpponentException ex) { Console.WriteLine($"[ERROR] {ex.Message}"); }

            catch (Exception ex) { Console.WriteLine($"[UNEXPECTED ERROR] {ex.Message}"); }

        }



        static void ToggleActive(Dictionary<string, Participant> registry)

        {

            Console.Write("Username: ");

            string username = Console.ReadLine();



            lock (RegistryLock)

            {

                if (!registry.ContainsKey(username))

                {

                    Console.WriteLine($"Player '{username}' was not found.");

                    return;

                }



                var player = registry[username];

                bool newStatus = !player.Isactive;

                player.SetActive(newStatus);



                Console.WriteLine($"{username} is now {(newStatus ? "Active" : "Suspended")}.");

            }

        }



        static void SaveRoster(Dictionary<string, Participant> registry, string filePath)

        {

            List<PlayerRecord> records = new List<PlayerRecord>();



            lock (RegistryLock)

            {

                foreach (var p in registry.Values)

                {

                    records.Add(new PlayerRecord

                    {

                        Type = p.GetType().Name,

                        Username = p.Username,

                        Nationality = p.Nationality,

                        Elo = p.Elo,

                        Winstreak = p.Winstreak,

                        Isactive = p.Isactive

                    });

                }

            }



            try

            {

                string json = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(filePath, json);

                Console.WriteLine($"Roster saved to {filePath} ({records.Count} players).");

            }

            catch (Exception ex)

            {

                Console.WriteLine($"[SAVE ERROR] {ex.Message}");

            }

        }



        static void LoadRoster(Dictionary<string, Participant> registry, string filePath)

        {

            if (!File.Exists(filePath))

            {

                Console.WriteLine($"File not found: {filePath}");

                return;

            }



            try

            {

                string json = File.ReadAllText(filePath);

                List<PlayerRecord> records = JsonSerializer.Deserialize<List<PlayerRecord>>(json);



                lock (RegistryLock)

                {

                    registry.Clear();



                    foreach (var r in records)

                    {

                        Participant p;



                        if (r.Type == "GauntletPlayer")

                        {

                            p = new GauntletPlayer(r.Username, r.Nationality, r.Elo, r.Winstreak, r.Isactive);

                        }

                        else

                        {

                            p = new Player(r.Username, r.Nationality, r.Elo, r.Winstreak, r.Isactive);

                        }



                        registry[r.Username] = p;

                    }

                }



                Console.WriteLine($"Roster loaded from {filePath} ({records.Count} players).");

            }

            catch (Exception ex)

            {

                Console.WriteLine($"[LOAD ERROR] {ex.Message}");

            }

        }



        static void ShowMenu()

        {

            Console.WriteLine("=========================");

            Console.WriteLine("     GAUNTLET SYSTEM     ");

            Console.WriteLine("=========================");

            Console.WriteLine("1. View all players");

            Console.WriteLine("2. Add a player");

            Console.WriteLine("3. Trigger a match");

            Console.WriteLine("4. Suspend / reactivate a player");

            Console.WriteLine("5. Save roster to file");

            Console.WriteLine("6. Load roster from file");

            Console.WriteLine("0. Exit");

            Console.Write("Choose an option: ");

        }



        static void ProcessMatch(Dictionary<string, Participant> registry, string challengerKey, string result)

        {

            lock (RegistryLock) // Locking the registry to ensure thread safety 

            {

                if (!registry.ContainsKey(challengerKey))

                {

                    throw new PlayerNotFoundException(

                $"Player '{challengerKey}' was not found.");//Added throw for when the player is not found in the registry 

                }/*return*/
                ;



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

                    throw new NoActiveOpponentException(

                        "No active opponents are currently available.");

                }



                // Validate result input first, so everything downstream uses a clean value 

                result = result.ToUpper();



                if (result != "W" && result != "L" && result != "D")

                {

                    throw new InvalidMatchResultException(

                      "Invalid match result. Please enter W, L, or D.");

                }



                Console.WriteLine($"[MATCH START] {p1.Username} ({p1.GetType().Name}, Elo {p1.Elo}) vs {p2.Username} ({p2.GetType().Name}, Elo {p2.Elo})");



                string p2Result = result == "W" ? "L" : (result == "L" ? "W" : "D");



                p1.CalculateNewElo(p2.Elo, result);

                p2.CalculateNewElo(p1.Elo, p2Result);



                //Upgrade/downgrades 

                CheckAndSwapTier(registry, p1.Username);

                CheckAndSwapTier(registry, p2.Username);



                // Match has now completed 

                MatchCompleted?.Invoke(p1, p2, result);

            }

        }



        static void CheckAndSwapTier(Dictionary<string, Participant> registry, string username)

        {

            if (!registry.ContainsKey(username)) return;



            // Check for Upgrade  

            if (registry[username].Winstreak >= 3 && registry[username] is IUpgradePlayer upgradable)

            {

                registry[username] = upgradable.UpgradePlayer();

                Console.WriteLine($"{username} reached 3 wins and UPGRADED to GauntletPlayer");

            }

            // Check for Downgrade  

            else if (registry[username].Winstreak <= -3 && registry[username] is IDowngradePlayer downgradable)

            {

                registry[username] = downgradable.DowngradePlayer();

                Console.WriteLine($"{username} dropped to -3 lossstreak and DEMOTED to Player");

            }

        }



        static void MatchmakingMonitor(Dictionary<string, Participant> registry)

        {

            while (_monitorRunning)

            {

                try

                {

                    string eligibleUsername = null;



                    lock (RegistryLock)

                    {

                        var eligible = registry.Values

                            .FirstOrDefault(p => p.Isactive && p.Winstreak >= StreakThreshold);



                        if (eligible != null)

                            eligibleUsername = eligible.Username;

                    }



                    if (eligibleUsername != null)

                    {

                        Participant eligiblePlayer;

                        lock (RegistryLock)

                        {

                            eligiblePlayer = registry[eligibleUsername];

                        }



                        StreakThresholdReached?.Invoke(eligiblePlayer);



                        string simulatedResult = SimulateGauntletResult(registry, eligibleUsername);

                        if (simulatedResult != null)

                        {

                            ProcessMatch(registry, eligibleUsername, simulatedResult);

                        }

                    }

                }

                catch (NoActiveOpponentException ex)

                {

                    Console.WriteLine($"[MONITOR] {ex.Message}");

                }

                catch (Exception ex)

                {

                    Console.WriteLine($"[MONITOR ERROR] {ex.Message}");

                }



                Thread.Sleep(4000);

            }

        }



        //Method for W, L, D simulation based on Elo ratings 

        static string SimulateGauntletResult(Dictionary<string, Participant> registry, string challengerKey)

        {

            lock (RegistryLock)

            {

                if (!registry.ContainsKey(challengerKey)) return null;

                var p1 = registry[challengerKey];



                int targetElo = p1.Elo;

                if (p1 is GauntletPlayer && p1.Winstreak > 0)

                    targetElo += (p1.Winstreak / 3) * 100;



                var p2 = registry.Values

                    .Where(p => p.Isactive && p.Username != p1.Username)

                    .OrderBy(p => Math.Abs(p.Elo - targetElo))

                    .FirstOrDefault();



                if (p2 == null) return null;



                double expected = 1.0 / (1.0 + Math.Pow(10.0, (p2.Elo - p1.Elo) / 400.0));

                double roll = Rng.NextDouble();



                if (roll < expected - 0.05) return "W";

                if (roll > expected + 0.05) return "L";

                return "D";

            }

        }

    }

}