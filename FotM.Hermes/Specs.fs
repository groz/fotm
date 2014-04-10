module Specs

type WarriorSpec =
| Arms = 71
| Fury = 72
| Protection = 73

type PaladinSpec = 
| Holy = 65
| Protection = 66
| Retribution = 70

type HunterSpec =
| ``Beast Mastery`` = 253
| Marksmanship = 254
| Survival = 255

type RogueSpec =
| Assassination = 259
| Combat = 260
| Subtlety = 261

type PriestSpec = 
| Discipline = 256
| Holy = 257
| Shadow = 258

type DeathKnightSpec =
| Blood = 250
| Frost = 251
| Unholy = 252

type ShamanSpec =
| Elemental = 262
| Enhancement = 263
| Restoration = 264

type MageSpec =
| Arcane = 62
| Fire = 63
| Frost = 64

type WarlockSpec =
| Affliction = 265
| Demonology = 266
| Destruction = 267

type MonkSpec = 
| Brewmaster = 268
| Windwalker = 269
| Mistweaver = 270

type DruidSpec =
| Balance = 102
| ``Feral Combat`` = 103
| Guardian = 104
| Restoration = 105

type Class =
| Warrior of WarriorSpec option
| Paladin of PaladinSpec option
| Hunter of HunterSpec option
| Rogue of RogueSpec option
| Priest of PriestSpec option
| ``Death Knight`` of DeathKnightSpec option
| Shaman of ShamanSpec option
| Mage of MageSpec option
| Warlock of WarlockSpec option
| Monk of MonkSpec option
| Druid of DruidSpec option

let toClass classId specId = 
    match classId with
    | 1 -> Warrior( Some(enum specId) )
    | 2 -> Paladin( Some(enum specId) )
    | 3 -> Hunter( Some(enum specId) )
    | 4 -> Rogue( Some(enum specId) )
    | 5 -> Priest( Some(enum specId) )
    | 6 -> ``Death Knight``( Some(enum specId) )
    | 7 -> Shaman( Some(enum specId) )
    | 8 -> Mage( Some(enum specId) )
    | 9 -> Warlock( Some(enum specId) )
    | 10 -> Monk( Some(enum specId) )
    | 11 -> Druid( Some(enum specId) )