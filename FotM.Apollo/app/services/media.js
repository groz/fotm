app.factory('media', function() {

    var races = {
        '1': 'Human',
        '2': 'Orc',
        '3': 'Dwarf',
        '4': 'Night Elf',
        '5': 'Undead',
        '6': 'Tauren',
        '7': 'Gnome',
        '8': 'Troll',
        '9': 'Goblin',
        '10': 'Blood Elf',
        '11': 'Draenei',
        '22': 'Worgen',
        '24': 'Pandaren',
        '25': 'Pandaren',
        '26': 'Pandaren'
    };

    var classes = {
        'Warrior': 1,
        'Paladin': 2,
        'Hunter': 3,
        'Rogue': 4,
        'Priest': 5,
        'DeathKnight': 6,
        'Shaman': 7,
        'Mage': 8,
        'Warlock': 9,
        'Monk': 10,
        'Druid': 11
    };

    var specs = {
        '62': { text: 'Arcane Mage', url: "http://media.blizzard.com/wow/icons/18/spell_holy_magicalsentry.jpg" },
        '63': { text: 'Fire Mage', url: "http://media.blizzard.com/wow/icons/18/spell_fire_firebolt02.jpg" },
        '64': { text: 'Frost Mage', url: "http://media.blizzard.com/wow/icons/18/spell_frost_frostbolt02.jpg" },
        '65': { text: 'Holy Paladin', url: "http://media.blizzard.com/wow/icons/18/spell_holy_holybolt.jpg" },
        '66': { text: 'Protection Paladin', url: "http://media.blizzard.com/wow/icons/18/ability_paladin_shieldofthetemplar.jpg" },
        '70': { text: 'Retribution Paladin', url: "http://media.blizzard.com/wow/icons/18/spell_holy_auraoflight.jpg" },
        '71': { text: 'Arms Warrior', url: "http://media.blizzard.com/wow/icons/18/ability_warrior_savageblow.jpg" },
        '72': { text: 'Fury Warrior', url: "http://media.blizzard.com/wow/icons/18/ability_warrior_innerrage.jpg" },
        '73': { text: 'Protection Warrior', url: "http://media.blizzard.com/wow/icons/18/ability_warrior_defensivestance.jpg" },
        '102': { text: 'Balance Druid', url: "http://media.blizzard.com/wow/icons/18/spell_nature_starfall.jpg" },
        '103': { text: 'Feral Druid', url: "http://media.blizzard.com/wow/icons/18/ability_druid_catform.jpg" },
        '104': { text: 'Guardian Druid', url: "http://media.blizzard.com/wow/icons/18/ability_racial_bearform.jpg" },
        '105': { text: 'Restoration Druid', url: "http://media.blizzard.com/wow/icons/18/spell_nature_healingtouch.jpg" },
        '250': { text: 'Blood Death Knight', url: "http://media.blizzard.com/wow/icons/18/spell_deathknight_bloodpresence.jpg" },
        '251': { text: 'Frost Death Knight', url: "http://media.blizzard.com/wow/icons/18/spell_deathknight_frostpresence.jpg" },
        '252': { text: 'Unholy Death Knight', url: "http://media.blizzard.com/wow/icons/18/spell_deathknight_unholypresence.jpg" },
        '253': { text: 'Beast Mastery Hunter', url: "http://media.blizzard.com/wow/icons/18/ability_hunter_bestialdiscipline.jpg" },
        '254': { text: 'Marksmanship Hunter', url: "http://media.blizzard.com/wow/icons/18/ability_hunter_focusedaim.jpg" },
        '255': { text: 'Survival Hunter', url: "http://media.blizzard.com/wow/icons/18/ability_hunter_camouflage.jpg" },
        '256': { text: 'Discipline Priest', url: "http://media.blizzard.com/wow/icons/18/spell_holy_powerwordshield.jpg" },
        '257': { text: 'Holy Priest', url: "http://media.blizzard.com/wow/icons/18/spell_holy_guardianspirit.jpg" },
        '258': { text: 'Shadow Priest', url: "http://media.blizzard.com/wow/icons/18/spell_shadow_shadowwordpain.jpg" },
        '259': { text: 'Assasination Rogue', url: "http://media.blizzard.com/wow/icons/18/ability_rogue_eviscerate.jpg" },
        '260': { text: 'Combat Rogue', url: "http://media.blizzard.com/wow/icons/18/ability_backstab.jpg" },
        '261': { text: 'Subtlety Rogue', url: "http://media.blizzard.com/wow/icons/18/ability_stealth.jpg" },
        '262': { text: 'Elemental Shaman', url: "http://media.blizzard.com/wow/icons/18/spell_nature_lightning.jpg" },
        '263': { text: 'Enhancement Shaman', url: "http://media.blizzard.com/wow/icons/18/spell_shaman_improvedstormstrike.jpg" },
        '264': { text: 'Restoration Shaman', url: "http://media.blizzard.com/wow/icons/18/spell_nature_magicimmunity.jpg" },
        '265': { text: 'Affliction Warlock', url: "http://media.blizzard.com/wow/icons/18/spell_shadow_deathcoil.jpg" },
        '266': { text: 'Demonology Warlock', url: "http://media.blizzard.com/wow/icons/18/spell_shadow_metamorphosis.jpg" },
        '267': { text: 'Destruction Warlock', url: "http://media.blizzard.com/wow/icons/18/spell_shadow_rainoffire.jpg" },
        '268': { text: 'Brewmaster Monk', url: "http://media.blizzard.com/wow/icons/18/spell_monk_brewmaster_spec.jpg" },
        '269': { text: 'Windwalker Monk', url: "http://media.blizzard.com/wow/icons/18/spell_monk_windwalker_spec.jpg" },
        '270': { text: 'Mistweaver Monk', url: "http://media.blizzard.com/wow/icons/18/spell_monk_mistweaver_spec.jpg" }
    }

    function getSpecId(classSpec) {

        return classSpec.Fields[0].Fields[0];
    }

    var mediaService = {
        raceText: function(race) {
            return races[race];
        },

        raceImage: function(race, gender) {
            return 'http://media.blizzard.com/wow/icons/18/race_' + race + '_' + gender + '.jpg';
        },

        classImage: function(classSpec) {
            var classId = classes[classSpec.Case];
            return 'http://media.blizzard.com/wow/icons/18/class_' + classId + '.jpg';
        },

        classText: function(classSpec) {
            return classSpec.Case;
        },

        specText: function(classSpec) {
            var specId = getSpecId(classSpec);
            return specs[specId].text;
        },

        specImage: function(classSpec) {
            var specId = getSpecId(classSpec);
            return specs[specId].url;
        },
    }

    return mediaService;
});