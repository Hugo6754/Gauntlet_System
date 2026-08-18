using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gauntlet_System.Interfaces;

namespace Gauntlet_System.Classes
{
    public abstract class Participant : ITier, IPromotable // Abtract class so any type of player inherits these properties 
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
        public abstract Participant UpgradePlayer();
        public abstract Participant DowngradePlayer();
    }
}
