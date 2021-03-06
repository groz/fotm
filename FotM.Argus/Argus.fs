﻿namespace FotM.Argus

open NodaTime
open FotM.Aether
open FotM.Data
open FotM.Hephaestus.TraceLogging
open Microsoft.ServiceBus.Messaging

module Argus =
    open FotM.Hephaestus

    let armoryPollTimeout = Duration.FromSeconds(30L)
    let armoryPollTimeoutInMilliseconds = int (armoryPollTimeout.ToTimeSpan().TotalMilliseconds)
    let duplicateCheckPeriod = Duration.FromMinutes(30L)

    let logEventToAnalytics = GoogleAnalytics.sendEvent "UA-49247455-4" "Argus"

    let shouldRetain snapshot =
         (SystemClock.Instance.Now - snapshot.timeTaken) < duplicateCheckPeriod

    let armoryUpdates region bracket = 
        Seq.unfold (fun oldHistory ->
            let history = oldHistory |> List.filter shouldRetain

            try
                let currentSnapshot = ArmoryLoader.load region bracket

                if history |> List.exists (fun entry -> entry.ladder = currentSnapshot.ladder) then
                    Some(None, history)
                else
                    Some(Some(currentSnapshot), currentSnapshot :: history)
            with
            | ex -> 
                logError "%s, %s armory update check generated exception: %A" region.code bracket.url ex

                logEventToAnalytics {
                        category = sprintf "Argus_%s_Exception" region.code
                        action = bracket.url + "_exception"
                        label = ""
                        value = ""
                    } |> ignore

                Some(None, history)
        ) [] // initial state is empty history

    let processArmory (region: RegionalSettings) (bracket: Bracket) (publisher: TopicWrapper) = async {
        let armoryInfo = sprintf "[%A, %A]" region.code bracket.url

        logInfo "%s started processing armory..." armoryInfo
                
        let pathPrefix = sprintf "%s/%s" region.code bracket.url
        let repository = Storage(GlobalSettings.playerLaddersContainer, pathPrefix = pathPrefix)
        let updatesStream = armoryUpdates region bracket

        try
            for update in updatesStream do
                match update with
                | None -> logInfo "%s duplicate or outdated update" armoryInfo
                | Some(snapshot) ->
                    let uri = repository.upload (snapshot)
                    logInfo "%s publishing update message" armoryInfo

                    logEventToAnalytics {
                        category = sprintf "Argus_%s" snapshot.region
                        action = sprintf "%s_bracket_update" snapshot.bracket.url
                        label = ""
                        value = ""
                    } |> ignore

                    publisher.send {
                        storageLocation = uri
                        region = region.code
                        bracket = bracket
                    }

                    logInfo "%s message published" armoryInfo

                logInfo "%s processArmory waiting for %A" armoryInfo armoryPollTimeout
                do! Async.Sleep armoryPollTimeoutInMilliseconds
        with
            | ex -> logError "%s scan generated exception: %A" armoryInfo ex
    }

    let watch publisher = async {
        logInfo "FotM.Argus entry point called, starting listening to armory updates..."

        [
            for region in Regions.all do
            for bracket in Brackets.all do
            yield (processArmory region bracket publisher)
        ]
        |> List.map Async.StartChild
        |> Async.Parallel
        |> Async.RunSynchronously
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore

        logInfo "Listening to armory updates stopped."
    }