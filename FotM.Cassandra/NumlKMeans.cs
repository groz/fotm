using System.Collections.Generic;
using numl.Math.Metrics;
using numl.Model;
using numl.Unsupervised;

namespace FotM.Cassandra
{
    public class NumlKMeans: IKMeans<PlayerDiff>
    {
        private readonly IDistance _distanceMetric;

        private static readonly Descriptor DiffDescriptor = Descriptor
            .For<PlayerDiff>()
            //.With(d => d.RealmId)
            .With(d => d.Ranking)
            .With(d => d.Rating)
            .With(d => d.WeeklyWins)
            .With(d => d.WeeklyLosses)
            .With(d => d.SeasonWins)
            .With(d => d.SeasonLosses)
            .With(d => d.RankingDiff)
            .With(d => d.WeeklyWinsDiff)
            .With(d => d.WeeklyLossesDiff)
            .With(d => d.SeasonWinsDiff)
            .With(d => d.SeasonLossesDiff)
            .With(d => d.RatingDiff);

        public NumlKMeans(IDistance distanceMetric = null)
        {
            _distanceMetric = distanceMetric ?? new HammingDistance();
        }

        public int[] Compute(PlayerDiff[] diffs, int nGroups)
        {
            var kMeans = new KMeans();
            kMeans.Descriptor = DiffDescriptor;

            // I have no idea why, but HammingDistance brings prediction accuracy from 33% to whooping 90%
            return kMeans.Generate(diffs, nGroups, _distanceMetric);
        }
    }
}