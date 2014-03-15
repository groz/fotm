using System;

namespace FotM.Domain
{
    public class Leaderboard
    {
        public LeaderboardEntry[] Rows { get; set; }
        public Bracket Bracket { get; set; }
        public DateTime Time { get; set; }
    }

}