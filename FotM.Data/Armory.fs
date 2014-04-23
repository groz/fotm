namespace FotM.Data

(* DOMAIN TYPES *)
type Realm = {
    realmId: int;
    realmName: string;
    realmSlug: string;
}

type Faction =
| Horde = 0
| Alliance = 1

type Gender =
| Male = 0
| Female = 1

type Race =
| Human = 1
| Orc = 2
| Dwarf = 3
| ``Night Elf`` = 4
| Undead = 5
| Tauren = 6
| Gnome = 7
| Troll = 8
| Goblin = 9
| ``Blood Elf`` = 10
| Draenei = 11
| Worgen = 22
| ``Pandaren Neutral`` = 24
| ``Pandaren Alliance`` = 25
| ``Pandaren Horde`` = 26

type Player = {
    name: string;
    realm: Realm;
    faction: Faction
    race: Race;
    gender: Gender;
    classSpec: Class;
}

type PlayerEntry = {
    player: Player;

    ranking: int;
    rating: int;
    seasonWins: int;
    seasonLosses: int;
    weeklyWins: int;
    weeklyLosses: int;
}

type Bracket = {
    url: string;
    teamSize: int;
}

type LadderSnapshot = {
    bracket: Bracket;
    ladder: seq<PlayerEntry>;
    timeTaken: NodaTime.Instant;
}

module Brackets =
    let twos = { url = "2v2"; teamSize = 2 }
    let threes = { url = "3v3"; teamSize = 3 }
    let fives = { url = "5v5"; teamSize = 5 }
    let rbg = { url = "rbg"; teamSize = 10 }

(* /DOMAIN TYPES *)
