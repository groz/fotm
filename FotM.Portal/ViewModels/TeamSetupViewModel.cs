using System.Linq;
using FotM.Domain;

namespace FotM.Portal.ViewModels
{
    public class TeamSetupViewModel
    {
        public int Rank { get; set; }
        public string Percent { get; set; }
        public SpecViewModel[] Specs { get; set; }
        
        public TeamSetupViewModel()
        {
        }

        public TeamSetupViewModel(int rank, TeamSetup teamSetup, double percent)
        {
            Rank = rank;
            Percent = string.Format("{0:F1}%", percent*100);

            Specs = Enumerable.Range(0, teamSetup.BracketSize)
                .Select(i => new SpecViewModel(teamSetup.ClassIds[i], teamSetup.SpecIds[i]))
                .OrderBy(svm => svm.CharSpec.IsHealer())
                .ThenBy(svm => svm.CharClass)
                .ToArray();
        }
    }
}