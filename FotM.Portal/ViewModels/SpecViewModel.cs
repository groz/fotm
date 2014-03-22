using FotM.Domain;

namespace FotM.Portal.ViewModels
{
    public class SpecViewModel
    {
        private readonly CharacterClass _charClass;
        private readonly CharacterSpec _charSpec;
        private readonly string _specId;
        private readonly string _classId;

        public SpecViewModel(int classId, int specId)
            : this((CharacterClass) classId, (CharacterSpec) specId)
        {
        }

        public SpecViewModel(CharacterClass charClass, CharacterSpec charSpec)
        {
            _charClass = charClass;
            _charSpec = charSpec;
            _specId = ((int)charSpec).ToString();
            _classId = ((int)charClass).ToString();
        }

        public string SpecId
        {
            get { return _specId; }
        }

        public string ClassId
        {
            get { return _classId; }
        }

        internal CharacterClass CharClass
        {
            get { return _charClass; }
        }

        internal CharacterSpec CharSpec
        {
            get { return _charSpec; }
        }
    }
}
