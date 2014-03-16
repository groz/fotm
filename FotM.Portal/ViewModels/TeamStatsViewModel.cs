using System;
using System.Linq;
using FotM.Domain;

namespace FotM.Portal.ViewModels
{
    public class TeamStatsViewModel
    {
        private readonly TeamStats _model;
        private readonly int _factionId;

        public PlayerViewModel[] Players;

        public TeamStatsViewModel(TeamStats model)
        {
            _model = model;
            _factionId = model.Team.Players.First().FactionId;
            Players = model.Team.Players.Select(p => new PlayerViewModel(p)).ToArray();
        }

        public string FactionImageLink
        {
            get
            {
                return string.Format("http://media.blizzard.com/wow/icons/18/faction_{0}.jpg", _factionId);
            }
        }

        public int RatingChange
        {
            get { return _model.RatingChange; }
        }

        public int Rating
        {
            get { return _model.Rating; }
        }

        public DateTime Updated
        {
            get { return _model.UpdatedUtc.ToLocalTime(); }
        }
    }
}