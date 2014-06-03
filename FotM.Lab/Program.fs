// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open FotM.Data
open FotM.Athena

let agent = MailboxProcessor<string>.Start(fun agent ->

    let maxLength = 1000

    let rec loop (state: string list) i = async {
        let! msg = agent.Receive()

        let newState = 
            try        
                printfn "received message: %s, iteration: %i, length: %i" msg i state.Length
                let truncatedState = state |> Seq.truncate maxLength |> Seq.toList
                msg::truncatedState
            with
            | ex -> 
                printfn "%A" ex
                state

        return! loop newState (i+1)
    }

    loop [] 0
)

let greeting = "hello"

while true do
    agent.Post greeting
    System.Threading.Thread.Sleep(1)

(*    let updates = 
        [
            {
                player = 
                    {
                        name = "Flummy";
                        realm = 
                            {
                                realmId = 11;
                                realmName = "Tichondrius";
                                realmSlug = "tichondrius";
                            };
                        faction = Faction.Horde;
                        classSpec = Druid (Some DruidSpec.Restoration);
                        race = Race.Tauren;
                        gender = Gender.Female;
                    };
                ranking = 177;
                rating = 2638;
                weeklyWins = 62;
                weeklyLosses = 44;
                seasonWins = 80;
                seasonLosses = 54;
                ratingDiff = 13;
            };

            {
                player = 
                    {
                        name = "Magebroqt";
                        realm = 
                            {
                                realmId = 11;
                                realmName = "Tichondrius";
                                realmSlug = "tichondrius";
                            };
                        faction = Faction.Horde;
                        classSpec = Mage (Some MageSpec.Frost);
                        race = Race.Undead;
                        gender = Gender.Male;
                    };
                ranking = 218;
                rating = 2620;
                weeklyWins = 10;
                weeklyLosses = 9;
                seasonWins = 169;
                seasonLosses = 164;
                ratingDiff = 14;
            };
            {
                player = 
                    {
                        name = "Nahjx";
                        realm = 
                            {
                                realmId = 11;
                                realmName = "Tichondrius";
                                realmSlug = "tichondrius";
                            };
                        faction = Faction.Horde;
                        classSpec = Rogue (Some RogueSpec.Subtlety);
                        race = Race.Undead;
                        gender = Gender.Male;
                    };
                ranking = 252;
                rating = 2607;
                weeklyWins = 22;
                weeklyLosses = 17;
                seasonWins = 226;
                seasonLosses = 155;
                ratingDiff = 15;
            };
            {
                player = 
                    {
                        name = "Navajonez";
                        realm = 
                            {
                                realmId = 11;
                                realmName = "Tichondrius";
                                realmSlug = "tichondrius";
                            };
                        faction = Faction.Horde;
                        classSpec = Druid (Some DruidSpec.``Feral Combat``);
                        race = Race.Tauren;
                        gender = Gender.Female;
                    };
                ranking = 168;
                rating = 2640;
                weeklyWins = 67;
                weeklyLosses = 55;
                seasonWins = 535;
                seasonLosses = 552;
                ratingDiff = -14;
            };

            {
                player = 
                    {
                        name = "Renewals";
                        realm = 
                            {
                                realmId = 11;
                                realmName = "Tichondrius";
                                realmSlug = "tichondrius";
                            };
                        faction = Faction.Horde;
                        classSpec = Priest (Some PriestSpec.Holy);
                        race = Race.Undead;
                        gender = Gender.Male;
                    };
                ranking = 183;
                rating = 2635;
                weeklyWins = 14;
                weeklyLosses = 10;
                seasonWins = 363;
                seasonLosses = 285;
                ratingDiff = -13;
            };

            {
                player = 
                    {
                        name = "Gorecke";
                        realm = 
                            {
                                realmId = 11;
                                realmName = "Tichondrius";
                                realmSlug = "tichondrius";
                            };
                        faction = Faction.Horde;
                        classSpec = Hunter (Some HunterSpec.Marksmanship);
                        race = Race.Orc;
                        gender = Gender.Female;};
                ranking = 242;
                rating = 2610;
                weeklyWins = 68;
                weeklyLosses = 65;
                seasonWins = 509;
                seasonLosses = 387;
                ratingDiff = -13;
            }
    ]


    let groups = Athena.split updates

    for g in groups do
        printfn "(------------------ %A --------------------)" g.Length

    let now = NodaTime.SystemClock.Instance.Now

    let teams = groups |> List.fold (fun acc g -> 
        printfn "Group: %A" g
        let teamsFound = Athena.findTeamsInGroup 3 now g
        printfn "%A" teamsFound.Length
        acc @ teamsFound) []

    for t in teams do
        printfn "Team found: %A" t

    0 // return an integer exit code
*)