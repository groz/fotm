using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FotM.Domain;
using FotM.Utilities;
using MoreLinq;

namespace FotM.Cassandra
{
    public static class CassandraUtils
    {
        public static List<Team[]> GenerateTeamCombinations(this Player[] players, int bracketSize)
        {
            Team[] allTeams = players.Combinations(3).Select(ps => new Team(ps)).ToArray();

            var result = (from t1 in allTeams
                from t2 in allTeams
                where t1 != t2
                from t3 in allTeams
                where t1 != t3 && t2 != t3
                let tc = new[] {t1, t2, t3}
                where IsValidCombination(tc, bracketSize)
                select tc).ToList();

            return result;
        }

        public static bool IsValidCombination(Team[] teams, int bracketSize)
        {
            var playersInCombination = teams.SelectMany(t => t.Players);
            return playersInCombination.Distinct().Count() == teams.Length*bracketSize;
        }
        
        public static IEnumerable<T> Union<T>(this IEnumerable<IEnumerable<T>> sources)
        {
            return sources.Aggregate((acc, s) => acc.Union(s));
        }

        public static int Size(this Bracket bracket)
        {
            int bracketSize;

            switch (bracket)
            {
                case Bracket.Twos:
                    bracketSize = 2;
                    break;
                case Bracket.Threes:
                    bracketSize = 3;
                    break;
                case Bracket.Fives:
                    bracketSize = 5;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return bracketSize;
        }
    }
}
