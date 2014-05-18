namespace FotM.Data

open Microsoft.FSharp.Reflection 
open System.Reflection
open System.Runtime.Serialization
open System.Xml

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

[<KnownType("GetKnownTypes")>]
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
    static member GetKnownTypes() =
        typeof<Class>.GetNestedTypes(BindingFlags.Public ||| BindingFlags.NonPublic)
        |> Array.filter FSharpType.IsUnion

module Specs = 

    let toSpecEnum<'a when 'a: enum<int>>(value: int): 'a option =
        if not (System.Enum.IsDefined(typeof<'a>, value)) then
            None
        else
            Some(LanguagePrimitives.EnumOfValue<int, 'a> value)

    let toClass classId specId = 

        if (classId < 1 || classId > 11) then 
            invalidArg "classId" (classId.ToString())

        match classId with
        | 1 -> Warrior( toSpecEnum specId )
        | 2 -> Paladin( toSpecEnum specId )
        | 3 -> Hunter( toSpecEnum specId )
        | 4 -> Rogue( toSpecEnum specId )
        | 5 -> Priest( toSpecEnum specId )
        | 6 -> ``Death Knight``( toSpecEnum specId )
        | 7 -> Shaman( toSpecEnum specId )
        | 8 -> Mage( toSpecEnum specId )
        | 9 -> Warlock( toSpecEnum specId )
        | 10 -> Monk( toSpecEnum specId )
        | 11 -> Druid( toSpecEnum specId )