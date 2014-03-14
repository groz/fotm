using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FotM.Domain;
using System.Collections.Generic;
using FotM.Domain;

namespace FotM.Cassandra
{
    public class Cassandra
    {
        public IEnumerable<Team> FindTeams(IEnumerable<Leaderboard> history)
        {
            return null;
        }

        public IEnumerable<Team> FindTeams(Leaderboard previous, Leaderboard current)
        {
            return null;
        }
    }
}