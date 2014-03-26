using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FotM.Domain;
using FotM.Portal.Infrastructure;

namespace FotM.Portal.ViewModels
{
    public class ArmoryViewModel
    {
        private readonly TeamStatsViewModel[] _allTimeViewModels;
        private readonly TeamStatsViewModel[] _playingNowViewModels;
        private readonly TeamSetupViewModel[] _teamSetupsViewModels;

        public ArmoryViewModel(TeamStatsRepository repository, 
            TimeSpan playingNowPeriod,
            int nTeamsToShow, 
            int nSetupsToShow,
            int nPlayingNowMax)
        {
            _allTimeViewModels = repository.QueryTopN(nTeamsToShow);
            _playingNowViewModels = repository.QueryPlayingNow(nPlayingNowMax, playingNowPeriod);
            _teamSetupsViewModels = repository.QueryTopSetups(nSetupsToShow);
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