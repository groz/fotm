using System;
using FotM.Domain;

namespace FotM.Cassandra
{
    public class PlayerDiff
    {
        public PlayerDiff(Player player, LeaderboardEntry previous, LeaderboardEntry current)
        {
            this.Player = player;

            if (previous.RealmId != current.RealmId || previous.Name != current.Name)
                throw new InvalidOperationException("PlayerDiff should target single player");

            this.RankingDiff = current.Ranking - previous.Ranking;
            this.WeeklyWinsDiff = current.WeeklyWins - previous.WeeklyWins;
            this.WeeklyLossesDiff = current.WeeklyLosses - previous.WeeklyLosses;
            this.SeasonWinsDiff = current.SeasonWins - previous.SeasonWins;
            this.SeasonLossesDiff = current.SeasonLosses - previous.SeasonLosses;
            this.RatingDiff = current.Rating - previous.Rating;

            this.Ranking = current.Ranking;
            this.Rating = current.Rating;
            this.FactionId = current.FactionId;

            this.WeeklyWins = current.WeeklyWins;
            this.WeeklyLosses = current.WeeklyLosses;
            this.SeasonWins = current.SeasonWins;
            this.SeasonLosses = current.SeasonLosses;

            this.RealmId = this.Player.Realm.RealmId;
        }

        public bool HasChanges
        {
            get
            {
                return !(WeeklyWinsDiff == 0 && WeeklyLossesDiff == 0 &&
                         SeasonLossesDiff == 0 && SeasonWinsDiff == 0 &&
                         RatingDiff == 0);
            }
        }

        public Player Player { get; private set; }
        public int FactionId { get; private set; }

        // FEATURES
        [Feature]
        public int RealmId { get; private set; }

        [Feature(Weight = 0.1)]
        public int Ranking { get; private set; }

        [Feature(Weight = 0.05)]
        public int Rating { get; private set; }

        [Feature(Weight = 0.5)]
        public int WeeklyWins { get; private set; }

        [Feature(Weight = 0.5)]
        public int WeeklyLosses { get; private set; }

        [Feature(Weight = 0.5)]
        public int SeasonWins { get; private set; }

        [Feature(Weight = 0.5)]
        public int SeasonLosses { get; private set; }

        // DIFF FEATURES
        [Feature(Weight = 5)]
        public int RankingDiff { get; private set; }

        [Feature]
        public int WeeklyWinsDiff { get; private set; }

        [Feature]
        public int WeeklyLossesDiff { get; private set; }

        [Feature]
        public int SeasonWinsDiff { get; private set; }

        [Feature]
        public int SeasonLossesDiff { get; private set; }

        [Feature(Weight = 10)]
        public int RatingDiff { get; private set; }
    }
}