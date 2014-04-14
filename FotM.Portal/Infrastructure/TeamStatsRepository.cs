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
        private readonly TeamInfo[] _orderedByTime;
        private readonly TeamSetupViewModel[] _teamSetupsViewModels;
        private readonly TeamStatsViewModel[] _allTimeViewModels;

        // number of times setup has to be seen to be considered legit for FotM
        private const int SetupSeenNumberFilter = 10;

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

            _allTimeViewModels = _verifiedTeams
                .OrderByDescending(t => t.Stats.Rating)
                .Select((ts, i) => new TeamStatsViewModel(i + 1, ts.Stats))
                .ToArray();
            
            _orderedByTime = _verifiedTeams
                .OrderByDescending(t => t.Stats.UpdatedUtc)
                .ToArray();

            _setupGroups = _verifiedTeams.GroupBy(t => t.Setup).ToArray();

            Dictionary<TeamSetup, int> setupSeen = _setupGroups
                .ToDictionary(sg => sg.Key, sg => sg.Sum(t => t.Stats.TimesSeen));


            int totalSeen = setupSeen.Values.Sum();

            var teamSetups = _setupGroups
                .Select(setupGroup => new
                {
                    Setup = setupGroup.Key,
                    Percent = setupSeen[setupGroup.Key] / (double)totalSeen,
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

            return _orderedByTime
                .TakeWhile(t => utcNow - t.Stats.UpdatedUtc < playingNowPeriod)
                .Take(nMax)
                .OrderByDescending(t => t.Stats.Rating)
                .Select((ts, i) => new TeamStatsViewModel(i + 1, ts.Stats))
                .ToArray();
        }

        public TeamSetupViewModel[] QueryTopSetups(int nMax)
        {
            return _teamSetupsViewModels.Take(nMax).ToArray();
        }

        public TeamSetupViewModel[] QueryFilteredSetups(TeamFilter[] teamFilters, int nMax)
        {
            return _teamSetupsViewModels.Where(tsvm => Passes(tsvm, teamFilters))
                .OrderBy(vm => vm.Rank)
                .Take(nMax)
                .ToArray();
        }

        private static bool Passes(TeamSetupViewModel setup, TeamFilter[] filters)
        {
            IEnumerable<CharacterClass> allClasses = SpecInfo.ClassMappings.Values.Distinct();
            IEnumerable<CharacterSpec> allSpecs = SpecInfo.ClassMappings.Keys;

            Dictionary<CharacterClass, int> setupClasses = allClasses.ToDictionary(c => c, c => 0);
            Dictionary<CharacterSpec, int> setupSpecs = allSpecs.ToDictionary(c => c, c => 0);

            foreach (var specViewModel in setup.Specs)
            {
                ++setupClasses[specViewModel.CharClass];
                ++setupSpecs[specViewModel.CharSpec];
            }

            Dictionary<CharacterClass, int> filterClasses = allClasses.ToDictionary(c => c, c => 0);
            Dictionary<CharacterSpec, int> filterSpecs = allSpecs.ToDictionary(c => c, c => 0);

            foreach (var filter in filters)
            {
                if (filter.ClassId.HasValue)
                    ++filterClasses[filter.Class];

                if (filter.SpecId.HasValue)
                    ++filterSpecs[filter.Spec];
            }

            return setupClasses.All(kv => kv.Value >= filterClasses[kv.Key]) &&
                   setupSpecs.All(kv => kv.Value >= filterSpecs[kv.Key]);
        }

        public TeamStatsViewModel[] QueryTeamsForSetup(TeamSetupViewModel teamSetupViewModel, int nMaxTeams)
        {
            var specs = teamSetupViewModel.Specs.Select(svm => svm.CharSpec).ToArray();

            return _allTimeViewModels
                .Where(t => t.Model.Team.HasSetup(specs))
                .OrderByDescending(teamStatsViewModel => teamStatsViewModel.Rating)
                .Take(nMaxTeams)
                .ToArray();
        }

        public TeamStatsViewModel[] QueryFilteredTeams(TeamFilter[] teamFilters, int nMaxTeams)
        {
            var teamDatas = from teamViewModel in _allTimeViewModels
                let team = teamViewModel.Model.Team
                let setupViewModel = _teamSetupsViewModels
                    .SingleOrDefault(tsvm => team.HasSetup(
                        tsvm.Specs.Select(svm => svm.CharSpec).ToArray())
                    )
                where setupViewModel != null
                select new {teamViewModel, setupViewModel};

            return teamDatas
                .Where(teamData => Passes(teamData.setupViewModel, teamFilters))
                .Select(teamData => teamData.teamViewModel)
                .OrderBy(teamStats => teamStats.Rank)
                .Take(nMaxTeams)
                .ToArray();
        }
    }
}