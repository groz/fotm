using FotM.Domain;

namespace FotM.Portal.Infrastructure
{
    public class TeamFilter
    {
        public int? SpecId { get; set; }
        public int? ClassId { get; set; }

        public CharacterSpec Spec
        {
            get { return (CharacterSpec)SpecId.Value; }
        }

        public CharacterClass Class
        {
            get { return (CharacterClass)ClassId.Value; }
        }
    }
}