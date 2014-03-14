using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotM.Domain
{
    public class Player : IEquatable<Player>
    {
        public string Name { get; set; }
        public Realm Realm { get; set; }

        public bool Equals(Player other)
        {
            return string.Equals(Name, other.Name) && Equals(Realm, other.Realm);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0)*397) ^ (Realm != null ? Realm.GetHashCode() : 0);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Player) obj);
        }
    }
}
