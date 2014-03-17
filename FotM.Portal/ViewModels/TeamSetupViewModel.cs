using System.Linq;
using FotM.Domain;

namespace FotM.Portal.ViewModels
{
    public class TeamSetupViewModel
    {
        private readonly int _rank;
        private readonly TeamSetup _teamSetup;
        private readonly double _percent;
        private SpecViewModel[] _specs;

        public TeamSetupViewModel(int rank, TeamSetup teamSetup, double percent)
        {
            _rank = rank;
            _teamSetup = teamSetup;
            _percent = percent;

            _specs = Enumerable.Range(0, _teamSetup.BracketSize)
                .Select(i => new SpecViewModel(_teamSetup.ClassIds[i], _teamSetup.SpecIds[i]))
                .OrderBy(svm => svm.CharSpec.IsHealer())
                .ThenBy(svm => svm.CharClass)
                .ToArray();
        }
        
        public SpecViewModel[] Specs
        {
            get
            {
                return _specs;
            }
        }
        
        public int Rank { get { return _rank; } }

        public string Percent
        {
            get
            {
                return string.Format("{0:F1}%", _percent*100);
            }
        }

        public int BracketSize
        {
            get { return _teamSetup.BracketSize; }
        }
    }

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