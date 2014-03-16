using System;
using System.Collections.Generic;
using System.Linq;

namespace FotM.Domain
{
    public class TeamSetup : IEquatable<TeamSetup>
    {
        public int[] SpecIds { get; private set; }

        public TeamSetup(Team team)
        {
            this.SpecIds = team.Players.Select(p => p.SpecId).OrderBy(id => id).ToArray();
        }

        public override string ToString()
        {
            return string.Join(", ", SpecIds.Select(id => SpecNames.Names[id]));
        }

        public bool Equals(TeamSetup other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return SpecIds.SequenceEqual(other.SpecIds);
        }

        public override int GetHashCode()
        {
            return SpecIds.Aggregate(1, (hashCode, id) => (hashCode*397) ^ id.GetHashCode());
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals((TeamSetup) other);
        }
    }
}