using System;

namespace FotM.Domain
{
    public class TeamStats
    {
        public TeamStats()
        {
        }

        public TeamStats(Team team, int rating, int ratingChange)
        {
            RatingChange = ratingChange;
            TimesSeen = 1;
            Team = team;
            UpdatedUtc = DateTime.UtcNow;
            Rating = rating;
        }

        public int Rating { get; set; }
        public int RatingChange { get; set; }
        public int TimesSeen { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public Team Team { get; set; }

        public bool IsVerified
        {
            get
            {
                return TimesSeen >= 1; // same setup seen twice or more
            }
        }

        public void Update(int rating, int ratingChange)
        {
            RatingChange += ratingChange;
            UpdatedUtc = DateTime.UtcNow;
            ++TimesSeen;
        }
    }
}