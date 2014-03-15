using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FotM.Domain
{
    public class Team: IEnumerable<Player>, IEquatable<Team>
    {
        public IReadOnlyCollection<Player> Players { get; private set; }

        public Team(IEnumerable<Player> players)
        {
            Players = players.OrderBy(p => p.Name).ThenBy(p => p.Realm).ToList();
        }

        public Team(params Player[] players): this(players.AsEnumerable())
        {
        }

        public IEnumerator<Player> GetEnumerator()
        {
            return Players.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
            return "(" + string.Join(",", Players) + ")";
        }
    }
}
