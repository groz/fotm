using FotM.Domain;

namespace FotM.Portal.ViewModels
{
    public class SpecViewModel
    {
        private readonly CharacterClass _charClass;
        private readonly CharacterSpec _charSpec;
        private readonly string _specImageLink;
        private readonly string _classImageLink;

        public SpecViewModel(int classId, int specId)
            : this((CharacterClass) classId, (CharacterSpec) specId)
        {
        }

        public SpecViewModel(CharacterClass charClass, CharacterSpec charSpec)
        {
            _charClass = charClass;
            _charSpec = charSpec;
            _specImageLink = MediaLinks.SpecImageLink(charSpec);
            _classImageLink = MediaLinks.ClassImageLink(charClass);
        }

        public string SpecImageLink
        {
            get { return _specImageLink; }
        }

        public string ClassImageLink
        {
            get { return _classImageLink; }
        }

        public CharacterClass CharClass
        {
            get { return _charClass; }
        }

        public CharacterSpec CharSpec
        {
            get { return _charSpec; }
        }
    }
}
