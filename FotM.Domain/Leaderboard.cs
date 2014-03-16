using System;
using System.Linq;

namespace FotM.Domain
{
    public class Leaderboard
    {
        public LeaderboardEntry[] Rows { get; set; }
        public Bracket Bracket { get; set; }
        public DateTime Time { get; set; }

        public void Order()
        {
            Rows = Rows
                .OrderByDescending(r => r.Rating)
                .ThenBy(r => r.Name)
                .ThenBy(r => r.RealmId)
                .ToArray();

            for (int i = 0; i < Rows.Length; ++i)
            {
                Rows[i].Ranking = i + 1;
            }
        }

        public LeaderboardEntry this[Player player]
        {
            get { return Rows.FirstOrDefault(r => r.CreatePlayer().Equals(player)); }
        }
    }

}