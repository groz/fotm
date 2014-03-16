using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotM.Domain
{
    public class Player : IEquatable<Player>
    {
        public Player()
        {   
        }
        
        public Player(LeaderboardEntry entry)
        {
            Name = entry.Name;
            ClassId = entry.ClassId;
            SpecId = entry.SpecId;
            FactionId = entry.FactionId;
            GenderId = entry.GenderId;
            RaceId = entry.RaceId;

            Realm = new Realm()
            {
                RealmId = entry.RealmId,
                RealmName = entry.RealmName,
                RealmSlug = entry.RealmSlug
            };
        }

        public string Name { get; set; }
        public Realm Realm { get; set; }
        public int ClassId { get; set; }
        public int SpecId { get; set; }
        public int FactionId { get; set; }
        public int GenderId { get; set; }
        public int RaceId { get; set; }

        public Race Race
        {
            get { return (Race) RaceId; }
        }

        public Gender Gender
        {
            get { return (Gender)GenderId; }
        }

        public CharacterSpec Spec
        {
            get
            {
                return (CharacterSpec)SpecId;
            }
        }

        public CharacterClass Class
        {
            get
            {
                return (CharacterClass)ClassId;
            }
        }

        public Faction Faction
        {
            get { return (Faction)FactionId; }
        }

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

        public override string ToString()
        {
            return string.Format("{0} - {1}", Name, Realm);
        }
    }
}
