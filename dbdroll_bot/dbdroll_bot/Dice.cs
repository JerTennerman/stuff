using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbdroll_bot
{
    class Dice
    {
        public double count { get; set; }
        public double max { get; set; }
        public string pointer { get; set; }
        public string previousOperation { get; set; }
        public double mod { get; set; }
        public double totalRoll { get; set; }
    }
}
