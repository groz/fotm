app.factory('media', function() {

    var factions = {
        0: { text: 'Alliance', url: "http://media.blizzard.com/wow/icons/18/faction_0.jpg" },
        1: { text: 'Horde', url: "http://media.blizzard.com/wow/icons/18/faction_1.jpg" },
    };

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
        'Warrior': { id: 1, specs: [71, 72, 73] },
        'Paladin': { id: 2, specs: [65, 66, 70] },
        'Hunter': { id: 3, specs: [253, 254, 255] },
        'Rogue': { id: 4, specs: [259, 260, 261] },
        'Priest': { id: 5, specs: [256, 257, 258] },
        'Death Knight': { id: 6, specs: [250, 251, 252] },
        'Shaman': { id: 7, specs: [262, 263, 264] },
        'Mage': { id: 8, specs: [62, 63, 64] },
        'Warlock': { id: 9, specs: [265, 266, 267] },
        'Monk': { id: 10, specs: [268, 269, 270] },
        'Druid': { id: 11, specs: [102, 103, 104, 105] }
    };

    var imagesRoot = "http://media.blizzard.com/wow/icons/18/";

    var specs = {
        '62': { text: 'Arcane Mage', url: imagesRoot+"spell_holy_magicalsentry.jpg" },
        '63': { text: 'Fire Mage', url: imagesRoot+"spell_fire_firebolt02.jpg" },
        '64': { text: 'Frost Mage', url: imagesRoot+"spell_frost_frostbolt02.jpg" },
        '65': { text: 'Holy Paladin', url: imagesRoot+"spell_holy_holybolt.jpg" },
        '66': { text: 'Protection Paladin', url: imagesRoot+"ability_paladin_shieldofthetemplar.jpg" },
        '70': { text: 'Retribution Paladin', url: imagesRoot+"spell_holy_auraoflight.jpg" },
        '71': { text: 'Arms Warrior', url: imagesRoot+"ability_warrior_savageblow.jpg" },
        '72': { text: 'Fury Warrior', url: imagesRoot+"ability_warrior_innerrage.jpg" },
        '73': { text: 'Protection Warrior', url: imagesRoot+"ability_warrior_defensivestance.jpg" },
        '102': { text: 'Balance Druid', url: imagesRoot+"spell_nature_starfall.jpg" },
        '103': { text: 'Feral Druid', url: imagesRoot+"ability_druid_catform.jpg" },
        '104': { text: 'Guardian Druid', url: imagesRoot+"ability_racial_bearform.jpg" },
        '105': { text: 'Restoration Druid', url: imagesRoot+"spell_nature_healingtouch.jpg" },
        '250': { text: 'Blood Death Knight', url: imagesRoot+"spell_deathknight_bloodpresence.jpg" },
        '251': { text: 'Frost Death Knight', url: imagesRoot+"spell_deathknight_frostpresence.jpg" },
        '252': { text: 'Unholy Death Knight', url: imagesRoot+"spell_deathknight_unholypresence.jpg" },
        '253': { text: 'Beast Mastery Hunter', url: imagesRoot+"ability_hunter_bestialdiscipline.jpg" },
        '254': { text: 'Marksmanship Hunter', url: imagesRoot+"ability_hunter_focusedaim.jpg" },
        '255': { text: 'Survival Hunter', url: imagesRoot+"ability_hunter_camouflage.jpg" },
        '256': { text: 'Discipline Priest', url: imagesRoot+"spell_holy_powerwordshield.jpg" },
        '257': { text: 'Holy Priest', url: imagesRoot+"spell_holy_guardianspirit.jpg" },
        '258': { text: 'Shadow Priest', url: imagesRoot+"spell_shadow_shadowwordpain.jpg" },
        '259': { text: 'Assasination Rogue', url: imagesRoot+"ability_rogue_eviscerate.jpg" },
        '260': { text: 'Combat Rogue', url: imagesRoot+"ability_backstab.jpg" },
        '261': { text: 'Subtlety Rogue', url: imagesRoot+"ability_stealth.jpg" },
        '262': { text: 'Elemental Shaman', url: imagesRoot+"spell_nature_lightning.jpg" },
        '263': { text: 'Enhancement Shaman', url: imagesRoot+"spell_shaman_improvedstormstrike.jpg" },
        '264': { text: 'Restoration Shaman', url: imagesRoot+"spell_nature_magicimmunity.jpg" },
        '265': { text: 'Affliction Warlock', url: imagesRoot+"spell_shadow_deathcoil.jpg" },
        '266': { text: 'Demonology Warlock', url: imagesRoot+"spell_shadow_metamorphosis.jpg" },
        '267': { text: 'Destruction Warlock', url: imagesRoot+"spell_shadow_rainoffire.jpg" },
        '268': { text: 'Brewmaster Monk', url: imagesRoot+"spell_monk_brewmaster_spec.jpg" },
        '269': { text: 'Windwalker Monk', url: imagesRoot+"spell_monk_windwalker_spec.jpg" },
        '270': { text: 'Mistweaver Monk', url: imagesRoot+"spell_monk_mistweaver_spec.jpg" }
    }

    var armoryEndPoints = {
        "eu": "http://eu.battle.net",
        "us": "http://us.battle.net",
        "kr": "http://kr.battle.net",
        "tw": "http://tw.battle.net",
        "cn": "http://www.battlenet.com.cn"
    };
    
    var mediaService = {
        raceText: function(race) {
            return races[race];
        },

        raceImage: function(race, gender) {
            return imagesRoot+'race_' + race + '_' + gender + '.jpg';
        },

        classImageForSpec: function(classSpec) {
            return this.classImage(classSpec.Case);
        },

        classImage: function (className) {
            var classId = classes[className].id;
            return imagesRoot+'class_' + classId + '.jpg';
        },

        classText: function(classSpec) {
            return classSpec.Case;
        },

        getSpecId: function (classSpec) {
            if (classSpec.Fields[0] == null) return -1;
            return classSpec.Fields[0].Fields[0];
        },

        specText: function (classSpec) {            
            var specId = this.getSpecId(classSpec);
            if (specId == -1) return "None";
            return specs[specId].text;
        },

        specImage: function (classSpec) {
            if (classSpec.Fields[0] == null)
                return "http://icons.iconarchive.com/icons/aha-soft/software/16/cancel-icon.png";
            var specId = this.getSpecId(classSpec);
            return specs[specId].url;
        },

        armoryLink: function (region, player) {
            var regionRoot = armoryEndPoints[region];
            return regionRoot + "/wow/en/character/" + player.realm.realmSlug + "/" + player.name + "/simple";
        },

        getSpecInfo: function (specId) {
            return specs[specId];
        },

        getSpecsFor: function(className) {
            if (className !== null) {
                var specIds = classes[className].specs;
                return specIds;
            }
            return [];
        },

        factionText: function(factionId) {
            return factions[factionId].text;
        },

        factionImage: function (factionId) {
            return factions[factionId].url;
        },

        classes: classes,
        specs: specs,
        races: races
    }

    return mediaService;
});