using System.Linq;
using FotM.Domain;

namespace FotM.Portal.ViewModels
{
    public class TeamSetupViewModel
    {
        private readonly int _rank;
        private readonly TeamSetup _teamSetup;
        private readonly double _percent;

        public TeamSetupViewModel(int rank, TeamSetup teamSetup, double percent)
        {
            _rank = rank;
            _teamSetup = teamSetup;
            _percent = percent;
        }

        public string[] SpecImageLinks
        {
            get
            {
                return _teamSetup.SpecIds.Select(MediaLinks.SpecImageLink).ToArray();
            }
        }

        public string[] ClassImageLinks
        {
            get
            {
                return _teamSetup.ClassIds.Select(MediaLinks.ClassImageLink).ToArray();
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
}