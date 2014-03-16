using System;

namespace FotM.Domain
{
    public class TeamStats
    {
        public TeamStats()
        {
        }

        public TeamStats(Team team)
        {
            RatingChange = 0;
            TimesSeen = 1;
            Team = team;
            Seen = DateTime.Now;
        }

        public double RatingChange { get; set; }
        public int TimesSeen { get; set; }
        public DateTime Seen { get; set; }
        public Team Team { get; set; }

        public bool IsVerified
        {
            get
            {
                return TimesSeen >= 2; // same setup seen twice or more
            }
        }

        public void Update(double ratingChange)
        {
            RatingChange += ratingChange;
            Seen = DateTime.Now;
            ++TimesSeen;
        }
    }
}