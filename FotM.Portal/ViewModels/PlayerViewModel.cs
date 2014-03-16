using System;
using FotM.Domain;

namespace FotM.Portal.ViewModels
{
    public class PlayerViewModel
    {
        private readonly Player _model;

        public PlayerViewModel(Player model)
        {
            _model = model;
        }

        public string Name
        {
            get { return _model.Name; }
        }

        public string Realm
        {
            get { return _model.Realm.RealmName; }
        }

        public string RaceImageLink
        {
            get
            {
                return MediaLinks.RaceImageLink(_model.RaceId, _model.GenderId);
            }
        }

        public string ClassImageLink
        {
            get
            {
                return MediaLinks.ClassImageLink(_model.ClassId);
            }
        }

        public string SpecImageLink
        {
            get
            {
                return MediaLinks.SpecImageLink(_model.Spec);
            }
        }
    }
}