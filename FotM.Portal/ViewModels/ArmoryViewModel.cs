using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FotM.Domain;

namespace FotM.Portal.ViewModels
{
    public class ArmoryViewModel
    {
        private readonly TeamStatsViewModel[] _teamStatsViewModels;
        private readonly TeamSetupViewModel[] _teamSetupsViewModels;

        public ArmoryViewModel(IEnumerable<TeamStats> teamStats, int nTeamsToShow, int nSetupsToShow)
        {
            var verifiedTeams = teamStats
                .Where(t => t.IsVerified)
                .Select(t => new
                {
                    Team = t.Team,
                    Setup = new TeamSetup(t.Team),
                    Stats = t
                })
                .ToArray();

            var setupGroups = verifiedTeams.GroupBy(t => t.Setup).ToArray();

            var teamSetups = setupGroups
                .Select(setupGroup => new
                {
                    Setup = setupGroup.Key,
                    Percent = setupGroup.Count()/(double)setupGroups.Length
                })
                .OrderByDescending(ts => ts.Percent)
                .Take(nSetupsToShow)
                .ToArray();

            _teamStatsViewModels = verifiedTeams
                .Take(nTeamsToShow)
                .OrderByDescending(t => t.Stats.Rating)
                .Select((ts, i) => new TeamStatsViewModel(i + 1, ts.Stats))
                .ToArray();

            _teamSetupsViewModels = teamSetups
                .Select((ts, i) => new TeamSetupViewModel(i+1, ts.Setup, ts.Percent))
                .ToArray();
        }

        public TeamStatsViewModel[] TeamStatsViewModels
        {
            get { return _teamStatsViewModels; }
        }

        public TeamSetupViewModel[] TeamSetupsViewModels
        {
            get { return _teamSetupsViewModels; }
        }
    }
}