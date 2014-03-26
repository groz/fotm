using System.Collections.Generic;

namespace FotM.Domain
{
    /*
    * http://wowprogramming.com/docs/api_types#specID
    * 
    */

    public enum CharacterSpec
    {
        Mage_Arcane = 62,
        Mage_Fire = 63,
        Mage_Frost = 64,
        Paladin_Holy = 65,
        Paladin_Protection = 66,
        Paladin_Retribution = 70,
        Warrior_Arms = 71,
        Warrior_Fury = 72,
        Warrior_Protection = 73,
        Druid_Balance = 102,
        Druid_Feral = 103,
        Druid_Guardian = 104,
        Druid_Restoration = 105,
        DeathKnight_Blood = 250,
        DeathKnight_Frost = 251,
        DeathKnight_Unholy = 252,
        Hunter_BeastMastery = 253,
        Hunter_Marksmanship = 254,
        Hunter_Survival = 255,
        Priest_Discipline = 256,
        Priest_Holy = 257,
        Priest_Shadow = 258,
        Rogue_Assassination = 259,
        Rogue_Combat = 260,
        Rogue_Subtlety = 261,
        Shaman_Elemental = 262,
        Shaman_Enhancement = 263,
        Shaman_Restoration = 264,
        Warlock_Affliction = 265,
        Warlock_Demonology = 266,
        Warlock_Destruction = 267,
        Monk_Brewmaster = 268,
        Monk_Windwalker = 269,
        Monk_Mistweaver = 270
    }

    public static class SpecExtensions
    {
        public static bool IsHealer(this CharacterSpec spec)
        {
            return spec == CharacterSpec.Druid_Restoration ||
                   spec == CharacterSpec.Shaman_Restoration ||
                   spec == CharacterSpec.Paladin_Holy ||
                   spec == CharacterSpec.Priest_Holy ||
                   spec == CharacterSpec.Priest_Discipline ||
                   spec == CharacterSpec.Monk_Mistweaver;
        }
    }

    public static class SpecInfo
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

        public static Dictionary<CharacterSpec, CharacterClass> ClassMappings =
            new Dictionary<CharacterSpec, CharacterClass>()
            {
                {CharacterSpec.Mage_Arcane, CharacterClass.Mage},
                {CharacterSpec.Mage_Fire, CharacterClass.Mage},
                {CharacterSpec.Mage_Frost, CharacterClass.Mage},
                {CharacterSpec.Paladin_Holy, CharacterClass.Paladin},
                {CharacterSpec.Paladin_Protection, CharacterClass.Paladin},
                {CharacterSpec.Paladin_Retribution, CharacterClass.Paladin},
                {CharacterSpec.Warrior_Arms, CharacterClass.Warrior},
                {CharacterSpec.Warrior_Fury, CharacterClass.Warrior},
                {CharacterSpec.Warrior_Protection, CharacterClass.Warrior},
                {CharacterSpec.Druid_Balance, CharacterClass.Druid},
                {CharacterSpec.Druid_Feral, CharacterClass.Druid},
                {CharacterSpec.Druid_Guardian, CharacterClass.Druid},
                {CharacterSpec.Druid_Restoration, CharacterClass.Druid},
                {CharacterSpec.DeathKnight_Blood, CharacterClass.DeathKnight},
                {CharacterSpec.DeathKnight_Frost, CharacterClass.DeathKnight},
                {CharacterSpec.DeathKnight_Unholy, CharacterClass.DeathKnight},
                {CharacterSpec.Hunter_BeastMastery, CharacterClass.Hunter},
                {CharacterSpec.Hunter_Marksmanship, CharacterClass.Hunter},
                {CharacterSpec.Hunter_Survival, CharacterClass.Hunter},
                {CharacterSpec.Priest_Discipline, CharacterClass.Priest},
                {CharacterSpec.Priest_Holy, CharacterClass.Priest},
                {CharacterSpec.Priest_Shadow, CharacterClass.Priest},
                {CharacterSpec.Rogue_Assassination, CharacterClass.Rogue},
                {CharacterSpec.Rogue_Combat, CharacterClass.Rogue},
                {CharacterSpec.Rogue_Subtlety, CharacterClass.Rogue},
                {CharacterSpec.Shaman_Elemental, CharacterClass.Shaman},
                {CharacterSpec.Shaman_Enhancement, CharacterClass.Shaman},
                {CharacterSpec.Shaman_Restoration, CharacterClass.Shaman},
                {CharacterSpec.Warlock_Affliction, CharacterClass.Warlock},
                {CharacterSpec.Warlock_Demonology, CharacterClass.Warlock},
                {CharacterSpec.Warlock_Destruction, CharacterClass.Warlock},
                {CharacterSpec.Monk_Brewmaster, CharacterClass.Monk},
                {CharacterSpec.Monk_Mistweaver, CharacterClass.Monk},
                {CharacterSpec.Monk_Windwalker, CharacterClass.Monk}
            };
    }
}