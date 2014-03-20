using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FotM.Domain;

namespace FotM.Portal.ViewModels
{
    public class ArmoryViewModel
    {
        private readonly TeamStatsViewModel[] _allTimeViewModels;
        private readonly TeamStatsViewModel[] _playingNowViewModels;
        private readonly TeamSetupViewModel[] _teamSetupsViewModels;

        public ArmoryViewModel(IEnumerable<TeamStats> teamStats, 
            int nTeamsToShow, 
            int nSetupsToShow,
            int nPlayingNowMax,
            TimeSpan playingNowPeriod)
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

            _allTimeViewModels = verifiedTeams
                .OrderByDescending(t => t.Stats.Rating)
                .Take(nTeamsToShow)
                .Select((ts, i) => new TeamStatsViewModel(i + 1, ts.Stats))
                .ToArray();

            var utcNow = DateTime.UtcNow;

            _playingNowViewModels = verifiedTeams
                .OrderByDescending(t => t.Stats.UpdatedUtc)
                .TakeWhile(t => utcNow - t.Stats.UpdatedUtc < playingNowPeriod)
                .Take(nPlayingNowMax)
                .OrderByDescending(t => t.Stats.Rating)
                .Select((ts, i) => new TeamStatsViewModel(i + 1, ts.Stats))
                .ToArray();

            _teamSetupsViewModels = teamSetups
                .Select((ts, i) => new TeamSetupViewModel(i+1, ts.Setup, ts.Percent))
                .ToArray();
        }

        public TeamStatsViewModel[] AllTimeLeaders
        {
            get { return _allTimeViewModels; }
        }

        public TeamSetupViewModel[] TeamSetupsViewModels
        {
            get { return _teamSetupsViewModels; }
        }

        public TeamStatsViewModel[] PlayingNow
        {
            get { return _playingNowViewModels; }
        }
    }
}