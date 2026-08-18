# Gauntlet System

A command-line chess matchmaking system built around a dynamic streak system, multithreaded matchmaking, and event-driven notifications. The system pairs players of similar Elo for normal matches, but rewards win streaks by pushing players up against significantly higher-rated opponents—giving skilled players a faster path out of Elo stagnation.

## Why This System?

Chess players can get stuck in an Elo range for reasons that have little to do with their actual skill—a situation often called "Elo Hell." Because rating changes per game are small, a player stuck this way has to grind through many games to move, which leads to frustration and burnout.

Team-based versions of this problem have been studied in games like *League of Legends*, where the cause is usually poor teammates rather than the player's own play. Chess does not have teammates, so its version of the problem is statistical: small Elo swings mean slow progress even for players who deserve to be rated higher.

The Gauntlet System addresses this by giving strong, in-form players a high-stakes route to accelerate their Elo. It changes how the pairing algorithm responds to a streak while keeping the roster safe across concurrent background updates.

## Features

* **Data Persistence (JSON File I/O):** Players are saved to and loaded from a local JSON file (`PlayerRecord` DTOs), maintaining full state across user sessions.
* **Menu-Driven CLI:** Clean, enumerated console navigation wrapped in defensive input parsing to prevent crashes.
* **Player Management:** Supports player creation, status toggles (Active / Inactive), and dynamic player lookup.
* **Multithreaded Matchmaking Monitor:** A dedicated background `Thread` continuously monitors the roster, evaluates player streaks, and executes automated gauntlet matches concurrently.
* **Event-Driven Architecture:** System actions trigger C# `Action` delegates and custom events (`OnStreakThresholdReached`, `OnMatchCompleted`) to keep application logic loosely coupled from output logging.
* **Dynamic Tiering Mechanics:**
* **`Player` (Standard Tier):** Standard Elo pairing and calculation rules.
* **`GauntletPlayer` (High Tier):** Unlocked via `IPromotable` upon reaching a 3-win streak. Introduces accelerated Elo formulas and higher streak targets.
* **Demotion Mechanic:** Reaching a -3 loss streak as a `GauntletPlayer` automatically demotes the player back to standard `Player` status.


* **Thread-Safe Roster Locking:** Shared access to the central player dictionary is guarded with strict `lock` primitives (`RegistryLock`) to prevent race conditions during concurrent updates.

## Architecture & Design Decisions

* **Object-Oriented Design:**
* **Abstraction:** `Participant` base class defines core domain contracts and abstract Elo calculations.
* **Inheritance:** `Player` and `GauntletPlayer` derive from `Participant`.
* **Polymorphism & Interfaces:** `ITier` defines rating calculation contracts, while `IPromotable` standardizes `UpgradePlayer()` and `DowngradePlayer()` tier transitions.
* **Encapsulation** Used getters and setters along with access modifiers to restrict access to sensitive properties like username. 


* **Concurrency & Multithreading:** The background matchmaking monitor runs on a separate thread using thread locks (`lock (RegistryLock)`) to ensure thread-safety against manual menu inputs.
* **Events & Delegates:** Utilizes `Action<T>` delegates within an `EventManager` class to handle real-time notifications for match results and streak thresholds.
* **Custom Exceptions:** Domain-specific exceptions (`PlayerNotFoundException`, `InvalidMatchResultException`, `NoActiveOpponentException`) handle invalid system states cleanly.

## How to Run

1. Clone or download the repository.
2. Open the solution in **Visual Studio** or **VS Code**.
3. Build the solution using the .NET SDK (`dotnet build`).
4. Execute the program (`dotnet run`).
5. Use the console menu options to view standings, manage players, manually trigger matches, or let the background monitor process matches automatically.

---
