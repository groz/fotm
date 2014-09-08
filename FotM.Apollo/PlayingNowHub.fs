module FotM.Apollo.PlayingNowUpdateManager

open Microsoft.AspNet.SignalR
open Microsoft.AspNet.SignalR.Hubs
open EkonBenefits.FSharp.Dynamic

[<HubName("playingNowHub")>]
type PlayingNowHub() =
    inherit Hub()

let clients = lazy GlobalHost.ConnectionManager.GetHubContext<PlayingNowHub>().Clients

let notifyUpdateReady(region: string, bracket: string, time: NodaTime.Instant): unit =
    let t = time.ToDateTimeUtc().ToString()
    clients.Value.All ? updateReady(region, bracket, t)