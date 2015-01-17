module FotM.Hephaestus.GoogleAnalytics
open HttpClient
 
(*
POST /collect HTTP/1.1
Host: www.google-analytics.com

payload_data

Events:
v=1             // Version.
&tid=UA-XXXX-Y  // Tracking ID / Property ID.
&cid=555        // Anonymous Client ID.

&t=event        // Event hit type
&ec=video       // Event Category. Required.
&ea=play        // Event Action. Required.
&el=holiday     // Event label.
&ev=300         // Event value.
*)

type AnalyticsEvent = {
    category: string
    action: string
    label: string
    value: string
}

let sendEvent (propertyId: string) (clientId: string) (event: AnalyticsEvent) =
    let body = 
        sprintf 
            "v=1&tid=%s&cid=%s&t=event&ec=%s&ea=%s&el=%s&ev=%s"
            propertyId clientId event.category event.action event.label event.value

    createRequest Post "http://www.google-analytics.com/collect"
    |> withHeader(ContentType "application/x-www-form-urlencoded")
    |> withHeader(UserAgent "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36")
    |> withBody body
    |> getResponseCode