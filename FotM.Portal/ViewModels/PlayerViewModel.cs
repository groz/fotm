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

        public string RaceId
        {
            get
            {
                return string.Format("{0}_{1}", _model.RaceId, _model.GenderId);
            }
        }

        public string ClassId
        {
            get
            {
                return _model.ClassId.ToString();
            }
        }

        public string SpecId
        {
            get
            {
                return _model.SpecId.ToString();
            }
        }
    }
}