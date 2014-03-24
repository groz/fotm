using System;
using System.Collections.Generic;
using System.Linq;
using FotM.Domain;

namespace FotM.Cassandra
{
    public class HealerAndSizeAwareKMeans : IKMeans<PlayerChange>
    {
        private readonly int _bracketSize;
        private readonly MyKMeans<PlayerChange> _myKMeans;

        public HealerAndSizeAwareKMeans(bool normalize, int bracketSize)
        {
            _bracketSize = bracketSize;
            _myKMeans = new MyKMeans<PlayerChange>(normalize, MyMetric);
        }

        private double MyMetric(IEnumerable<PlayerChange> arg)
        {
            Player[] team = arg.Select(a => a.Player).ToArray();

            int nHealers = team.Count(Healers.IsHealer);

            int sizePenalty = team.Length == _bracketSize
                ? 0
                : 5;

            int healersPenalty = Math.Abs(nHealers - 1); // 0 for 1 healer, only valid for 3v3

            return healersPenalty + sizePenalty;
        }

        public int[] ComputeGroups(PlayerChange[] dataSet, int nGroups)
        {
            return _myKMeans.ComputeGroups(dataSet, nGroups);
        }
    }
}