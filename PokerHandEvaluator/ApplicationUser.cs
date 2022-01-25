using System;
using System.Collections.Generic;
using System.Text;

namespace PokerHandEvaluator
{
    public class ApplicationUser
    {
        public string Name { get; set; }
        public int Chips { get; set; }
        public List<string[]> PlayerCards { get; set; }
        public bool IsPlayingThisRound { get; set; } = true;
        public bool IsPlayingThisGame { get; set; } = true;
    }
}
