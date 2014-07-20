namespace FotM.Data

type Realm = {
    realmId: int;
    realmName: string;
    realmSlug: string;
}

type Faction =
| Alliance = 0
| Horde = 1

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
    name: string
    realm: Realm
    faction: Faction
    classSpec: Class 
    race: Race
    gender: Gender
}

type PlayerUpdate = {
    player: Player

    // features
    ranking: int
    rating: int
    weeklyWins: int
    weeklyLosses: int
    seasonWins: int
    seasonLosses: int

    // diff features
    ratingDiff: int
}

type PlayerEntry = {
    player: Player

    ranking: int
    rating: int
    seasonWins: int
    seasonLosses: int
    weeklyWins: int
    weeklyLosses: int
} 
with
    static member (-) (current: PlayerEntry, previous: PlayerEntry) : PlayerUpdate = {
        player = current.player
        ranking = current.ranking
        rating = current.rating
        weeklyWins = current.weeklyWins
        weeklyLosses = current.weeklyLosses
        seasonWins = current.seasonWins
        seasonLosses = current.seasonLosses
        ratingDiff = current.rating - previous.rating
    }
    member this.seasonTotal = this.seasonWins + this.seasonLosses
    member this.weeklyTotal = this.weeklyWins + this.weeklyLosses

type Bracket = {
    url: string
    teamSize: int
}

type TeamEntry = 
    {
        players: Player list
        ratingChange: int
        rating: int
        snapshotTime: NodaTime.Instant
    }
    member this.getClasses() = 
        this.players
        |> List.map(fun p -> p.classSpec)
        |> List.sortBy(fun spec -> spec.isHealer, spec.isRanged, Specs.getClassId spec)
    member this.getPlayers() = 
        this.players
        |> List.sortBy(fun p -> p.classSpec.isHealer, p.classSpec.isRanged, Specs.getClassId p.classSpec, p.name, p.realm)

type TeamInfo = 
    {
        lastEntry: TeamEntry
        totalWins: int
        totalLosses: int
    }
    member this.totalGames = this.totalWins + this.totalLosses
    member this.winRatio = float this.totalWins / float this.totalGames

module Teams =
    let createEntry (snapshotTime: NodaTime.Instant) (playerUpdates: PlayerUpdate list) = {
        TeamEntry.players = 
            playerUpdates 
            |> List.map (fun pu -> pu.player)
            |> List.sortBy (fun p -> (p.classSpec, p.realm, p))
        ratingChange = 
            playerUpdates 
            |> List.map (fun pu -> float pu.ratingDiff)
            |> List.average
            |> int
        rating =
            playerUpdates 
            |> List.map (fun pu -> float pu.rating)
            |> List.average
            |> int
        snapshotTime = snapshotTime
    }

    let createTeamInfo (teamEntries: TeamEntry array) = 
        let teamEntries = teamEntries |> Array.sortBy(fun te -> -te.snapshotTime.Ticks)
        let won, lost = teamEntries |> Array.partition (fun e -> e.ratingChange > 0)

        {
            lastEntry = teamEntries.[0]
            totalWins = won.Length
            totalLosses = lost.Length
        }

    let matchesFilter (filters: Class array) (setup: Class seq) =
        let setupClasses = setup |> Seq.countBy(fun p -> Specs.getClassId p) |> Seq.toList
        let setupSpecs = setup |> Seq.countBy(fun p -> p) |> Seq.toList

        let filterClasses = filters |> Seq.countBy(fun p -> Specs.getClassId p) |> Seq.toList
        let filterSpecs = 
            filters
            |> Seq.filter(fun s -> s.defined)
            |> Seq.countBy(fun p -> p) |> Seq.toList

        let allClasses = (setupClasses @ filterClasses) |> Seq.distinct
        let allSpecs = (setupSpecs @ filterSpecs) |> Seq.distinct

        let getCount id (classes: (_*int) list) =
            let c = classes |> Seq.tryFind(fun (cid, _) -> id = cid)
            match c with
            | None -> 0
            | Some(cid, count) -> count

        let ok all setup filter =
            all
            |> Seq.forall(fun (id, count) -> 
                let setupCount = setup |> getCount id
                let filterCount = filter |> getCount id
                setupCount >= filterCount)

        let classesOk = ok allClasses setupClasses filterClasses
        let specsOk = ok allSpecs setupSpecs filterSpecs

        classesOk && specsOk

    let teamMatchesFilter (classFilters: Class array) teamInfo =
        teamInfo.lastEntry.getClasses() |> matchesFilter classFilters

[<StructuralEquality;NoComparison>]
type LadderSnapshot<'a> = {
    region: string
    bracket: Bracket
    ladder: 'a array
    timeTaken: NodaTime.Instant
}

type PlayerLadderSnapshot = LadderSnapshot<PlayerEntry>

type TeamLadderSnapshot = LadderSnapshot<TeamEntry>

module Brackets =
    let twos = { url = "2v2"; teamSize = 2 }
    let threes = { url = "3v3"; teamSize = 3 }
    let fives = { url = "5v5"; teamSize = 5 }
    let rbg = { url = "rbg"; teamSize = 10 }

    let all = [twos; threes; fives; rbg]
