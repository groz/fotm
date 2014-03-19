using System;

namespace FotM.Utilities
{
    public static class RatingUtils
    {
        public static int EstimatedRatingChange(double playerRating, double opponentRating, bool playerWon)
        {
            const double k = 32.0;

            double playerWinChance = 1 / (1 + Math.Pow(10, (opponentRating - playerRating) / 400.0));

            int pw = playerWon ? 1 : 0;

            return (int)(k * (pw - playerWinChance));
        }
    }
}