using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FotM.Cassandra.Tests
{
    [TestFixture]
    public class CassandraUtilsTests
    {
        [Test]
        public void TeamCombinationsTest()
        {
            var players = CassandraSuperTests.GeneratePlayers(9).Select(p => p.Player()).ToArray();

            var teams = CassandraUtils.GenerateTeamCombinations(players, 3);
        }
    }
}
