namespace FotM.Data

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

    member c.defined = 
        match c with
        | Warrior None
        | Paladin None
        | Hunter None
        | Rogue None
        | Priest None
        | ``Death Knight`` None
        | Shaman None
        | Mage None
        | Warlock None
        | Monk None
        | Druid None -> false
        | _  -> true

    member c.isHealer =
        let healers = [
            Druid(Some DruidSpec.Restoration)
            Shaman(Some ShamanSpec.Restoration)
            Paladin(Some PaladinSpec.Holy)
            Priest(Some PriestSpec.Holy)
            Priest(Some PriestSpec.Discipline)
            Monk(Some MonkSpec.Mistweaver)
        ]
        healers |> Seq.exists ((=) c)

    member c.isRanged =
        let ranged = [
            Mage(Some MageSpec.Arcane)
            Mage(Some MageSpec.Fire)
            Mage(Some MageSpec.Frost)
            Druid(Some DruidSpec.Balance)
            Druid(Some DruidSpec.Guardian)
            Hunter(Some HunterSpec.``Beast Mastery``)
            Hunter(Some HunterSpec.Marksmanship)
            Hunter(Some HunterSpec.Survival)
            Priest(Some PriestSpec.Shadow)
            Shaman(Some ShamanSpec.Elemental)
            Warlock(Some WarlockSpec.Affliction)
            Warlock(Some WarlockSpec.Demonology)
            Warlock(Some WarlockSpec.Destruction)
        ]
        ranged |> Seq.exists ((=) c)


module Specs = 

    let toSpecEnum<'a when 'a: enum<int>>(value: int): 'a option =
        if not (System.Enum.IsDefined(typeof<'a>, value)) then
            None
        else
            Some(LanguagePrimitives.EnumOfValue<int, 'a> value)

    let toClassOption classId specId = 

        if (classId < 1 || classId > 11) then 
            None
        else
            Some ( 
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
            )

    let toClass classId specId = 
        match toClassOption classId specId with
        | None -> invalidArg "classId" (classId.ToString())
        | Some(c) -> c

    let fromString className (specIdStr: string) =

        let specId = if specIdStr = null then -1 else (int specIdStr)

        let classId = 
            match className with
                | "Warrior" -> 1
                | "Paladin" -> 2
                | "Hunter" -> 3
                | "Rogue" -> 4
                | "Priest" -> 5
                | "Death Knight" -> 6
                | "Shaman" -> 7
                | "Mage" -> 8
                | "Warlock" -> 9
                | "Monk" -> 10
                | "Druid" -> 11
                | _ -> -1

        toClassOption classId specId

    let getClassId = 
        function
        | Warrior _ -> 1
        | Paladin _ -> 2
        | Hunter _ -> 3
        | Rogue _ -> 4
        | Priest _ -> 5
        | ``Death Knight`` _ -> 6
        | Shaman _ -> 7
        | Mage _ -> 8
        | Warlock _ -> 9
        | Monk _ -> 10
        | Druid _ -> 11
