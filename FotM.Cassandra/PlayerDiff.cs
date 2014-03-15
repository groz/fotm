using FotM.Domain;

namespace FotM.Cassandra
{
    public class PlayerDiff
    {
        public PlayerDiff(Player player, LeaderboardEntry previous, LeaderboardEntry current)
        {
            this.Player = player;
            this.RankingDiff = current.Ranking - previous.Ranking;
            this.WeeklyWinsDiff = current.WeeklyWins - previous.WeeklyWins;
            this.WeeklyLossesDiff = current.WeeklyLosses - previous.WeeklyLosses;
            this.SeasonWinsDiff = current.SeasonWins - previous.SeasonWins;
            this.SeasonLossesDiff = current.SeasonLosses - previous.SeasonLosses;
            this.RatingDiff = current.Rating - previous.Rating;

            this.Ranking = current.Ranking;
            this.Rating = current.Rating;
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

        [Feature]
        public int Ranking { get; private set; }

        [Feature]
        public int Rating { get; private set; }

        [Feature]
        public int RankingDiff { get; private set; }

        [Feature]
        public int WeeklyWinsDiff { get; private set; }

        [Feature]
        public int WeeklyLossesDiff { get; private set; }

        [Feature]
        public int SeasonWinsDiff { get; private set; }

        [Feature]
        public int SeasonLossesDiff { get; private set; }

        [Feature]
        public int RatingDiff { get; private set; }
    }
}