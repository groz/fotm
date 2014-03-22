using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using FotM.Domain;
using FotM.Utilities;

namespace FotM.Portal.ViewModels
{
    public class MediaViewModel
    {
        public Dictionary<string, string> FactionImages { get; private set; }
        public Dictionary<string, string> ClassImages { get; private set; }
        public Dictionary<string, string> SpecImages { get; private set; }

        public Dictionary<string, string> RaceImages { get; private set; }

        public MediaViewModel()
        {
            SpecImages = CollectionExtensions.GetValues<CharacterSpec>()
                .ToDictionary(s => ((int)s).ToString(), MediaLinks.SpecImageLink);

            ClassImages = CollectionExtensions.GetValues<CharacterClass>()
                .ToDictionary(s => ((int)s).ToString(), MediaLinks.ClassImageLink);

            FactionImages = CollectionExtensions.GetValues<Faction>()
                .ToDictionary(s => ((int)s).ToString(), MediaLinks.FactionImageLink);

            RaceImages = (
                from race in CollectionExtensions.GetValues<Race>()
                from gender in CollectionExtensions.GetValues<Gender>()
                let key = string.Format("{0}_{1}", (int) race, (int) gender)
                let link = MediaLinks.RaceImageLink(race, gender)
                select new {key, link})
                .ToDictionary(o => o.key, o => o.link);
        }
    }
}