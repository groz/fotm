using FotM.Domain;

namespace FotM.Portal.ViewModels
{
    public class SpecViewModel
    {
        public CharacterClass CharClass { get; set; }
        public CharacterSpec CharSpec { get; set; }
        public int SpecId { get; set; }
        public int ClassId { get; set; }

        public SpecViewModel()
        {
        }

        public SpecViewModel(int classId, int specId)
            : this((CharacterClass) classId, (CharacterSpec) specId)
        {
        }

        public SpecViewModel(CharacterClass charClass, CharacterSpec charSpec)
        {
            CharClass = charClass;
            CharSpec = charSpec;
            SpecId = (int)charSpec;
            ClassId = (int)charClass;
        }
    }
}
