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

        public static bool IsHealingSpec(CharacterSpec spec)
        {
            /* 
             * This method is used in distance calculations, it's impossibly slow if
             * it relies on Dictionary key lookup
             */

            if (!Enum.IsDefined(typeof(CharacterSpec), spec))
                throw new ArgumentException("Spec not found");

            return spec == CharacterSpec.Druid_Restoration ||
                   spec == CharacterSpec.Priest_Holy ||
                   spec == CharacterSpec.Priest_Discipline ||
                   spec == CharacterSpec.Shaman_Restoration ||
                   spec == CharacterSpec.Monk_Mistweaver ||
                   spec == CharacterSpec.Paladin_Holy;
        }

        public static bool IsHealingSpec(int spec)
        {
            return IsHealingSpec((CharacterSpec) spec);
        }

        public static bool IsHealer(Player player)
        {
            return IsHealingSpec(player.Spec);
        }

        public static bool IsHealer(LeaderboardEntry player)
        {
            return IsHealer(player.Player());
        }
    }
}
