// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open System
open FotM.Data
open FotM.Athena
open System.Net
open System.IO
open System.Drawing
open System.Drawing.Imaging
open FotM.Aether

let downloadImage (path: string) =
    use client = new WebClient()
    let saveTo = System.IO.Path.GetFileName(path)
    client.DownloadFile(path, saveTo)
    saveTo

let GetEncoder(format: ImageFormat) =
    let codecs = ImageCodecInfo.GetImageDecoders()
    codecs |> Seq.find(fun c -> c.FormatID = format.Guid)

let CompressJPEGImage(sourcePath: string, targetPath: string) =
    let encoder: ImageCodecInfo = GetEncoder ImageFormat.Png

    let myEncoder: System.Drawing.Imaging.Encoder = System.Drawing.Imaging.Encoder.Quality
    let myEncoderParameters: EncoderParameters = new EncoderParameters 1
    let myEncoderParameter: EncoderParameter = new EncoderParameter (myEncoder, 1L)
    myEncoderParameters.Param.[0] <- myEncoderParameter

    try
        let bmp = Bitmap.FromFile sourcePath
        bmp.Save(targetPath, encoder, myEncoderParameters)
        true
    with
    | ex -> 
        printfn "Exception: %A" ex
        false


let compressedDir = "compressed"
Directory.CreateDirectory(compressedDir) |> ignore

let prepareImage(url: string) =
    let path = downloadImage url
    let pngPath = Path.ChangeExtension(path, ".png")
    let targetPath = Path.Combine(compressedDir, pngPath)    
    CompressJPEGImage(path, targetPath) |> ignore
    targetPath

let storage = Storage("images", "<insert your azure storage connection string here>")

let upload(path: string) =
    storage.uploadFile(path, TimeSpan.FromDays 1000.0)

let images = 
        [|"http://media.blizzard.com/wow/icons/18/faction_0.jpg"; "http://media.blizzard.com/wow/icons/18/faction_1.jpg"; "http://media.blizzard.com/wow/icons/18/race_1_0.jpg"; "http://media.blizzard.com/wow/icons/18/race_1_1.jpg"; "http://media.blizzard.com/wow/icons/18/race_2_0.jpg"; "http://media.blizzard.com/wow/icons/18/race_2_1.jpg"; "http://media.blizzard.com/wow/icons/18/race_3_0.jpg"; "http://media.blizzard.com/wow/icons/18/race_3_1.jpg"; "http://media.blizzard.com/wow/icons/18/race_4_0.jpg"; "http://media.blizzard.com/wow/icons/18/race_4_1.jpg"; "http://media.blizzard.com/wow/icons/18/race_5_0.jpg"; "http://media.blizzard.com/wow/icons/18/race_5_1.jpg"; "http://media.blizzard.com/wow/icons/18/race_6_0.jpg"; "http://media.blizzard.com/wow/icons/18/race_6_1.jpg"; "http://media.blizzard.com/wow/icons/18/race_7_0.jpg"; "http://media.blizzard.com/wow/icons/18/race_7_1.jpg"; "http://media.blizzard.com/wow/icons/18/race_8_0.jpg"; "http://media.blizzard.com/wow/icons/18/race_8_1.jpg"; "http://media.blizzard.com/wow/icons/18/race_9_0.jpg"; "http://media.blizzard.com/wow/icons/18/race_9_1.jpg"; "http://media.blizzard.com/wow/icons/18/race_10_0.jpg"; "http://media.blizzard.com/wow/icons/18/race_10_1.jpg"; "http://media.blizzard.com/wow/icons/18/race_11_0.jpg"; "http://media.blizzard.com/wow/icons/18/race_11_1.jpg"; "http://media.blizzard.com/wow/icons/18/race_22_0.jpg"; "http://media.blizzard.com/wow/icons/18/race_22_1.jpg"; "http://media.blizzard.com/wow/icons/18/race_24_0.jpg"; "http://media.blizzard.com/wow/icons/18/race_24_1.jpg"; "http://media.blizzard.com/wow/icons/18/race_25_0.jpg"; "http://media.blizzard.com/wow/icons/18/race_25_1.jpg"; "http://media.blizzard.com/wow/icons/18/race_26_0.jpg"; "http://media.blizzard.com/wow/icons/18/race_26_1.jpg"; "http://media.blizzard.com/wow/icons/18/class_1.jpg"; "http://media.blizzard.com/wow/icons/18/class_2.jpg"; "http://media.blizzard.com/wow/icons/18/class_3.jpg"; "http://media.blizzard.com/wow/icons/18/class_4.jpg"; "http://media.blizzard.com/wow/icons/18/class_5.jpg"; "http://media.blizzard.com/wow/icons/18/class_6.jpg"; "http://media.blizzard.com/wow/icons/18/class_7.jpg"; "http://media.blizzard.com/wow/icons/18/class_8.jpg"; "http://media.blizzard.com/wow/icons/18/class_9.jpg"; "http://media.blizzard.com/wow/icons/18/class_10.jpg"; "http://media.blizzard.com/wow/icons/18/class_11.jpg"; "http://media.blizzard.com/wow/icons/18/spell_holy_magicalsentry.jpg"; "http://media.blizzard.com/wow/icons/18/spell_fire_firebolt02.jpg"; "http://media.blizzard.com/wow/icons/18/spell_frost_frostbolt02.jpg"; "http://media.blizzard.com/wow/icons/18/spell_holy_holybolt.jpg"; "http://media.blizzard.com/wow/icons/18/ability_paladin_shieldofthetemplar.jpg"; "http://media.blizzard.com/wow/icons/18/spell_holy_auraoflight.jpg"; "http://media.blizzard.com/wow/icons/18/ability_warrior_savageblow.jpg"; "http://media.blizzard.com/wow/icons/18/ability_warrior_innerrage.jpg"; "http://media.blizzard.com/wow/icons/18/ability_warrior_defensivestance.jpg"; "http://media.blizzard.com/wow/icons/18/spell_nature_starfall.jpg"; "http://media.blizzard.com/wow/icons/18/ability_druid_catform.jpg"; "http://media.blizzard.com/wow/icons/18/ability_racial_bearform.jpg"; "http://media.blizzard.com/wow/icons/18/spell_nature_healingtouch.jpg"; "http://media.blizzard.com/wow/icons/18/spell_deathknight_bloodpresence.jpg"; "http://media.blizzard.com/wow/icons/18/spell_deathknight_frostpresence.jpg"; "http://media.blizzard.com/wow/icons/18/spell_deathknight_unholypresence.jpg"; "http://media.blizzard.com/wow/icons/18/ability_hunter_bestialdiscipline.jpg"; "http://media.blizzard.com/wow/icons/18/ability_hunter_focusedaim.jpg"; "http://media.blizzard.com/wow/icons/18/ability_hunter_camouflage.jpg"; "http://media.blizzard.com/wow/icons/18/spell_holy_powerwordshield.jpg"; "http://media.blizzard.com/wow/icons/18/spell_holy_guardianspirit.jpg"; "http://media.blizzard.com/wow/icons/18/spell_shadow_shadowwordpain.jpg"; "http://media.blizzard.com/wow/icons/18/ability_rogue_eviscerate.jpg"; "http://media.blizzard.com/wow/icons/18/ability_backstab.jpg"; "http://media.blizzard.com/wow/icons/18/ability_stealth.jpg"; "http://media.blizzard.com/wow/icons/18/spell_nature_lightning.jpg"; "http://media.blizzard.com/wow/icons/18/spell_shaman_improvedstormstrike.jpg"; "http://media.blizzard.com/wow/icons/18/spell_nature_magicimmunity.jpg"; "http://media.blizzard.com/wow/icons/18/spell_shadow_deathcoil.jpg"; "http://media.blizzard.com/wow/icons/18/spell_shadow_metamorphosis.jpg"; "http://media.blizzard.com/wow/icons/18/spell_shadow_rainoffire.jpg"; "http://media.blizzard.com/wow/icons/18/spell_monk_brewmaster_spec.jpg"; "http://media.blizzard.com/wow/icons/18/spell_monk_windwalker_spec.jpg"; "http://media.blizzard.com/wow/icons/18/spell_monk_mistweaver_spec.jpg"; "http://icons.iconarchive.com/icons/aha-soft/software/16/cancel-icon.png"|] 

images |> Array.map prepareImage |> Array.map upload |> ignore


(*
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

*)

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