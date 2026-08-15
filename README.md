# Gauntlet System

A command line chess matchmaking system built around a dynamic streak system. The system pairs players of similar Elo for normal matches, but rewards win streaks by pushing players up against significantly higher rated opponents, giving skilled players a faster path out of Elo stagnation.

## Why this system?

Chess players can get stuck in an Elo range for reasons that have little to do with their actual skill, a situation often called "Elo Hell." Because rating changes per game are small, a player stuck this way has to grind through many games to move, which leads to frustration and burnout.

Team based versions of this problem have been studied in games like League of Legends, where the cause is usually poor teammates rather than the player's own play. Chess does not have teammates, so its version of the problem is more statistical: small Elo swings mean slow progress even for players who deserve to be rated higher.

The Gauntlet System addresses this by giving strong, in form players a high stakes route to accelerate their Elo. It only changes how the pairing algorithm responds to a streak.

## Features

- **Player storage:** Players are stored in a separate file so records persist between sessions. This covers the project's data persistence requirement and the File I/O bonus feature.
- **Menu driven interface:** The entire system runs from the command prompt.
- **Player management:** Players can be created and suspended.
- **Elo based matchmaking:** Players are automatically paired with others of a similar Elo for standard matches.
- **Gauntlet streak mechanic:** Win streaks trigger matches against much higher rated opponents, offering an accelerated path up the ladder.
- **Elo calculation:** Ratings update after each match based on a standard or similar Elo formula.

## Design Decisions

- **Object oriented design:** The system applies core OOP principles (encapsulation, inheritance, polymorphism, abstraction) throughout.
- **Two interfaces:** Meaningful interfaces define what player and match related classes must be able to do, rather than relying on inheritance alone.
- **Exception handling:** All user input is wrapped in try/catch blocks so the presentation layer never crashes on bad input.
- **Events:** The system raises events for meaningful state changes, such as a match completing or a player's streak triggering a gauntlet pairing.
- **Threading:** At least one operation runs concurrently, for example background matchmaking or file writes, so the interface stays responsive.

## How to Run

1. Clone or download the project files.
2. Open the project in your IDE of choice.
3. Build the solution.
4. Run the main program from the command line or your IDE's run command.
5. Follow the on screen menu to create players, run matches, and view standings.





