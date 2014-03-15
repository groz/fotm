using System;
using System.Collections.Generic;
using System.Linq;
using FotM.Domain;

namespace FotM.ArmoryScanner
{
    class TeamSetup : IEquatable<TeamSetup>
    {
        public int[] SpecIds { get; private set; }

        public TeamSetup(Team team)
        {
            this.SpecIds = team.Players.Select(p => p.SpecId).OrderBy(id => id).ToArray();
        }

        public override string ToString()
        {
            return string.Join(", ", SpecIds.Select(id => SpecNames.Names[id]));
        }

        public bool Equals(TeamSetup other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return SpecIds.SequenceEqual(other.SpecIds);
        }

        public override int GetHashCode()
        {
            return SpecIds.Aggregate(1, (hashCode, id) => (hashCode * 397) ^ id.GetHashCode());
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals((TeamSetup)other);
        }
    }

    public static class SpecNames
    {
        public static Dictionary<int, string> Names = new Dictionary<int, string>
        {
            {62, "Mage: Arcane"},
            {63, "Mage: Fire"},
            {64, "Mage: Frost"},
            {65, "Paladin: Holy"},
            {66, "Paladin: Protection"},
            {70, "Paladin: Retribution"},
            {71, "Warrior: Arms"},
            {72, "Warrior: Fury"},
            {73, "Warrior: Protection"},
            {102, "Druid: Balance"},
            {103, "Druid: Feral"},
            {104, "Druid: Guardian"},
            {105, "Druid: Restoration"},
            {250, "Death Knight: Blood"},
            {251, "Death Knight: Frost"},
            {252, "Death Knight: Unholy"},
            {253, "Hunter: Beast Mastery"},
            {254, "Hunter: Marksmanship"},
            {255, "Hunter: Survival"},
            {256, "Priest: Discipline"},
            {257, "Priest: Holy"},
            {258, "Priest: Shadow"},
            {259, "Rogue: Assassination"},
            {260, "Rogue: Combat"},
            {261, "Rogue: Subtlety"},
            {262, "Shaman: Elemental"},
            {263, "Shaman: Enhancement"},
            {264, "Shaman: Restoration"},
            {265, "Warlock: Affliction"},
            {266, "Warlock: Demonology"},
            {267, "Warlock: Destruction"},
            {268, "Monk: Brewmaster"},
            {269, "Monk: Windwalker"},
            {270, "Monk: Mistweaver"},
        };
    }

    /*
     * http://wowprogramming.com/docs/api_types#specID
     * 
     * 
     * Type: specID

Global index of different specializations used by GetSpecializationInfoByID(), GetSpecializationRoleByID(), and returned by GetArenaOpponentSpec().

62 - Mage: Arcane
63 - Mage: Fire
64 - Mage: Frost
65 - Paladin: Holy
66 - Paladin: Protection
70 - Paladin: Retribution
71 - Warrior: Arms
72 - Warrior: Fury
73 - Warrior: Protection
102 - Druid: Balance
103 - Druid: Feral
104 - Druid: Guardian
105 - Druid: Restoration
250 - Death Knight: Blood
251 - Death Knight: Frost
252 - Death Knight: Unholy
253 - Hunter: Beast Mastery
254 - Hunter: Marksmanship
255 - Hunter: Survival
256 - Priest: Discipline
257 - Priest: Holy
258 - Priest: Shadow
259 - Rogue: Assassination
260 - Rogue: Combat
261 - Rogue: Subtlety
262 - Shaman: Elemental
263 - Shaman: Enhancement
264 - Shaman: Restoration
265 - Warlock: Affliction
266 - Warlock: Demonology
267 - Warlock: Destruction
268 - Monk: Brewmaster
269 - Monk: Windwalker
270 - Monk: Mistweaver

     */
}