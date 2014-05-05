namespace FotM.Argus

open NodaTime
open FotM.Aether
open FotM.Data
open FotM.Hephaestus.TraceLogging

module Argus =
    let armoryPollTimeout = Duration.FromSeconds(20L)
    let armoryPollTimeoutInMilliseconds = int (armoryPollTimeout.ToTimeSpan().TotalMilliseconds)
    let duplicateCheckPeriod = Duration.FromHours(1L)

    let shouldRetain(snapshot: LadderSnapshot) =
         (SystemClock.Instance.Now - snapshot.timeTaken) < duplicateCheckPeriod

    let armoryUpdates region bracket = 
        Seq.unfold (fun oldHistory ->
            let history = oldHistory |> List.filter shouldRetain
            let currentSnapshot = ArmoryLoader.load region bracket

            if history |> List.exists (fun entry -> entry.ladder = currentSnapshot.ladder) then
                Some(None, history)
            else
                // TODO: discard updats with > 1 game diff between them
                Some(Some(currentSnapshot), currentSnapshot :: history)
        ) [] // initial state is empty history

    let processArmory (region: RegionalSettings) (bracket: Bracket) = async {
        let armoryInfo = sprintf "[%A, %A]" region.code bracket.url

        logInfo "%s started processing armory..." armoryInfo

        let repository = SnapshotRepository(region, bracket)
        let updatesStream = armoryUpdates region bracket

        try
            for update in updatesStream do
                match update with
                | None -> logInfo "%s duplicate or outdated update" armoryInfo
                | Some(snapshot) ->
                    let uri = repository.uploadSnapshot snapshot
                    logInfo "%s uploaded update to %A" armoryInfo uri
                    // TODO: send update to appropriate topic

                logInfo "%s processArmory waiting for %A" armoryInfo armoryPollTimeout
                do! Async.Sleep armoryPollTimeoutInMilliseconds
        with
            | ex -> logError "%s scan generated exception: %A" armoryInfo ex
    }

    let watch = async {
        logInfo "FotM.Argus entry point called, starting listening to armory updates..."

        [
            for region in Regions.all do
            for bracket in Brackets.all do
            yield (processArmory region bracket)
        ]
        |> List.map Async.StartChild
        |> Async.Parallel
        |> Async.RunSynchronously
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore

        logInfo "Listening to armory updates stopped."
    }