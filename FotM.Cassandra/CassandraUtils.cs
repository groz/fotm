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
