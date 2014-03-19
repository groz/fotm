using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotM.Domain
{
    public static class Healers
    {
        public static readonly Dictionary<CharacterSpec, CharacterClass> Specs =
            new Dictionary<CharacterSpec, CharacterClass>
            {
                {CharacterSpec.Druid_Restoration, CharacterClass.Druid},
                {CharacterSpec.Priest_Holy, CharacterClass.Priest},
                {CharacterSpec.Priest_Discipline, CharacterClass.Priest},
                {CharacterSpec.Shaman_Restoration, CharacterClass.Shaman},
                {CharacterSpec.Monk_Mistweaver, CharacterClass.Monk},
                {CharacterSpec.Paladin_Holy, CharacterClass.Paladin},
            };

        public static bool IsHealer(Player player)
        {
            return Specs.ContainsKey(player.Spec);
        }

        public static bool IsHealer(LeaderboardEntry player)
        {
            return IsHealer(player.Player());
        }
    }
}
