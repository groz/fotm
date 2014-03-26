using System;
using System.Collections.Generic;
using System.Linq;
using FotM.Domain;
using FotM.Portal.ViewModels;

namespace FotM.Portal.Infrastructure
{
    public class TeamStatsRepository
    {
        private readonly TeamInfo[] _verifiedTeams;
        private readonly IGrouping<TeamSetup, TeamInfo>[] _setupGroups;
        private readonly TeamStatsViewModel[] _allTimeViewModels;
        private TeamStatsViewModel[] _playingNowViewModels;
        private TeamSetupViewModel[] _teamSetupsViewModels;

        public TeamStatsRepository(TeamStats[] teamStats)
        {
            _verifiedTeams = teamStats
                .Where(t => t.IsVerified)
                .Select(t => new TeamInfo()
                {
                    Team = t.Team,
                    Setup = new TeamSetup(t.Team),
                    Stats = t
                })
                .ToArray();

            _setupGroups = _verifiedTeams.GroupBy(t => t.Setup).ToArray();

            _allTimeViewModels = _verifiedTeams
                .OrderByDescending(t => t.Stats.Rating)
                .Select((ts, i) => new TeamStatsViewModel(i + 1, ts.Stats))
                .ToArray();
            
            _playingNowViewModels = _verifiedTeams
                .OrderByDescending(t => t.Stats.UpdatedUtc)
                .Select((ts, i) => new TeamStatsViewModel(i + 1, ts.Stats))
                .ToArray();

            int totalSetups = _setupGroups.Sum(sg => sg.Count());

            var teamSetups = _setupGroups
                .Select(setupGroup => new
                {
                    Setup = setupGroup.Key,
                    Percent = setupGroup.Count() / (double)totalSetups,
                })
                .OrderByDescending(ts => ts.Percent)
                .ToArray();

            _teamSetupsViewModels = teamSetups
                .Select((ts, i) => new TeamSetupViewModel(i + 1, ts.Setup, ts.Percent))
                .ToArray();
        }

        public TeamStatsViewModel[] QueryTopN(int nMax)
        {
            return _allTimeViewModels.Take(nMax).ToArray();
        }

        public TeamStatsViewModel[] QueryPlayingNow(int nMax, TimeSpan playingNowPeriod)
        {
            var utcNow = DateTime.UtcNow;

            return _playingNowViewModels
                .TakeWhile(t => utcNow - t.UpdatedUtc < playingNowPeriod)
                .Take(nMax)
                .OrderByDescending(t => t.Rating)
                .ToArray();
        }

        public TeamSetupViewModel[] QueryTopSetups(int nMax)
        {
            return _teamSetupsViewModels.Take(nMax).ToArray();
        }

        public TeamSetupViewModel[] QueryFilteredSetups(TeamFilter[] teamFilters, int nMax)
        {
            var filters = teamFilters
                .Where(filter => filter != null)
                .GroupBy(tf => tf.SpecId)
                .Select(g => new {specId = g.Key, count = g.Count()});

            return (from vm in _teamSetupsViewModels
                let vmSpecs = vm.Specs.Select(s => s.SpecId)
                where filters
                    .All(specFilter => vmSpecs
                        .Count(s => specFilter.specId == s) == specFilter.count)
                select vm)
                .OrderBy(vm => vm.Rank)
                .Take(nMax)
                .ToArray();
        }

        public TeamStatsViewModel[] QueryTeamsForSetup(TeamSetupViewModel teamSetupViewModel, int nMaxTeams)
        {
            var specs = teamSetupViewModel.Specs.Select(svm => svm.CharSpec).ToArray();

            return _verifiedTeams
                .Where(t => t.Team.HasSetup(specs))
                .OrderByDescending(sg => sg.Stats.Rating)
                .Take(nMaxTeams)
                .Select((sg, i) => new TeamStatsViewModel(i + 1, sg.Stats))
                .ToArray();
        }
    }
}