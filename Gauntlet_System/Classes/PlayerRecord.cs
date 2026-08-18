using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gauntlet_System.Classes
{
    public class PlayerRecord
    {
        public string Type { get; set; }
        public string Username { get; set; }
        public string Nationality { get; set; }
        public int Elo { get; set; }
        public int Winstreak { get; set; }
        public bool Isactive { get; set; }
    }
}
