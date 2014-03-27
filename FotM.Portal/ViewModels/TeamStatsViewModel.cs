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
                .OrderBy(p => p.Spec.IsHealer())
                .ThenBy(p => p.Spec.IsRanged())
                .ThenBy(p => p.ClassId)
                .Select(p => new PlayerViewModel(p)).ToArray();
        }

        public int Rank { get; set; }

        public int Rating
        {
            get { return _model.Rating; }
        }

        public string FactionId
        {
            get
            {
                return _factionId.ToString();
            }
        }

        public string Updated
        {
            get { return _model.UpdatedUtc.ToString(); }
        }

        internal DateTime UpdatedUtc
        {
            get { return _model.UpdatedUtc; }
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
                if (DateTime.Now - UpdateTimeout < _model.UpdatedUtc.ToLocalTime())
                {
                    // color recent changes
                    if (_model.RatingChange > 0)
                        return "success";
                    else if (_model.RatingChange < 0)
                        return "danger";
                }
                return "";
            }
        }
    }
}