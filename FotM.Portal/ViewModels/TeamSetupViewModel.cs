using System.Linq;
using FotM.Domain;

namespace FotM.Portal.ViewModels
{
    public class TeamSetupViewModel
    {
        private readonly int _rank;
        private readonly TeamSetup _teamSetup;
        private readonly double _percent;
        private readonly SpecViewModel[] _specs;
        private readonly TeamStatsViewModel[] _teamViewModels;

        public TeamSetupViewModel(int rank, TeamSetup teamSetup, double percent, TeamStatsViewModel[] teamViewModels)
        {
            _rank = rank;
            _teamSetup = teamSetup;
            _percent = percent;
            _teamViewModels = teamViewModels;

            _specs = Enumerable.Range(0, _teamSetup.BracketSize)
                .Select(i => new SpecViewModel(_teamSetup.ClassIds[i], _teamSetup.SpecIds[i]))
                .OrderBy(svm => svm.CharSpec.IsHealer())
                .ThenBy(svm => svm.CharClass)
                .ToArray();
        }

        public TeamStatsViewModel[] Teams
        {
            get { return _teamViewModels; }
        }
        
        public SpecViewModel[] Specs
        {
            get { return _specs; }
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
}