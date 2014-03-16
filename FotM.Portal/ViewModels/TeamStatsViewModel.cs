using System;
using System.Linq;
using FotM.Domain;

namespace FotM.Portal.ViewModels
{
    public class TeamStatsViewModel
    {
        private static readonly TimeSpan UpdateTimeout = TimeSpan.FromMinutes(5);
        private readonly TeamStats _model;
        private readonly int _factionId;

        public PlayerViewModel[] Players;

        public TeamStatsViewModel(int rank, TeamStats model)
        {
            Rank = rank;
            _model = model;
            _factionId = model.Team.Players.First().FactionId;

            Players = model.Team.Players
                .OrderBy(p => p.ClassId)
                .Select(p => new PlayerViewModel(p)).ToArray();
        }

        public int Rank { get; set; }

        public string FactionImageLink
        {
            get
            {
                return string.Format("http://media.blizzard.com/wow/icons/18/faction_{0}.jpg", _factionId);
            }
        }

        public int Rating
        {
            get { return _model.Rating; }
        }

        public string Updated
        {
            get { return _model.UpdatedUtc.ToString(); }
        }

        public string RatingChange
        {
            get
            {
                var str = _model.RatingChange.ToString();
                return _model.RatingChange > 0 ? "+" + str : str;
            }
        }

        public string DeltaType
        {
            get
            {
                if (DateTime.Now - UpdateTimeout > _model.UpdatedUtc.ToLocalTime())
                    return "";

                // color recent changes
                if (_model.RatingChange > 0)
                    return "success";
                else if (_model.RatingChange < 0)
                    return "danger";

                return "info";
            }
        }
    }
}