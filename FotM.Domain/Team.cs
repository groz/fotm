using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FotM.Domain
{
    public class Team: IEquatable<Team>
    {
        public Player[] Players { get; set; }

        public Team()
        {
        }

        public Team(IEnumerable<Player> players)
        {
            Players = players.OrderBy(p => p.Name).ThenBy(p => p.Realm).ToArray();
        }

        public Team(params Player[] players): this(players.AsEnumerable())
        {
        }

        public override int GetHashCode()
        {
            return Players.Aggregate(1, (hashCode, p) => (hashCode * 397) ^ p.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Team)obj);
        }

        public bool Equals(Team other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Players.SequenceEqual(other.Players);
        }

        public override string ToString()
        {
            return "(" + string.Join(",", Players.AsEnumerable()) + ")";
        }
    }
}
