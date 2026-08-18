using Gauntlet_System.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gauntlet_System.Interfaces
{
    interface IPromotable // Interfaces for upgrading/ downgrading players 
    {
        Participant UpgradePlayer();
        Participant DowngradePlayer();
    }
}
