using System;

namespace FotM.Domain
{
    public class Realm : IEquatable<Realm>
    {
        public int RealmId { get; set; }
        public string RealmName { get; set; }
        public string RealmSlug { get; set; }

        public bool Equals(Realm other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RealmId == other.RealmId;
        }

        public override int GetHashCode()
        {
            return RealmId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Realm) obj);
        }
    }
}