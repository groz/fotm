using System;
using System.Collections.Generic;
using System.Linq;
using FotM.Domain;
using FotM.Utilities;

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

            int sizePenalty = 0;

            if (team.Length < _bracketSize)
            {
                sizePenalty = 5;
            } 
            else if (team.Length > _bracketSize)
            {
                sizePenalty = 200;
            }

            int healersPenalty = 0;

            if (_bracketSize == 2)
            {
                if (nHealers == 2)
                    healersPenalty = 1;
            }
            else if (_bracketSize == 3)
            {
                healersPenalty = Math.Abs(nHealers - 1); // 0 for 1 healer
            }
            else if (_bracketSize == 5)
            {
                if (nHealers > 2)
                    healersPenalty = Math.Abs(nHealers - 2); // 1 for 3 healers, 2 for 4 healers
            }

            return healersPenalty + sizePenalty;
        }

        public int[] ComputeGroups(PlayerChange[] dataSet, int nGroups)
        {
            return _myKMeans.ComputeGroups(dataSet, nGroups);
        }
    }
}