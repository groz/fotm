namespace FotM.Aether

open System
open System.Net
open Microsoft.WindowsAzure
open Microsoft.ServiceBus
open Microsoft.ServiceBus.Messaging
open FotM.Hephaestus.TraceLogging
open FotM.Hephaestus.Async

type TopicWrapper(client: TopicClient) =
    let retry = RetryBuilder(3, logException "TopicWrapper operation failed with %A")
    
    member this.send (data: obj) =
        try
            retry {
                use brokeredMessage = new BrokeredMessage(data) 
                client.Send brokeredMessage
            }
        with
        | ex -> logError "Error during TopicWrapper.post: %A. Update ignored." ex

    interface IDisposable with
        member this.Dispose() =
            client.Close()

type ServiceBus(?connectionString) =

    let messageTimeToLive = TimeSpan.FromHours 10.0
    
    let serviceBusConnectionString =
        defaultArg connectionString (CloudConfigurationManager.GetSetting "Microsoft.ServiceBus.ConnectionString")
    
    let namespaceManager = NamespaceManager.CreateFromConnectionString serviceBusConnectionString

    do
        logInfo "initializing servicebus..."
        logInfo "Service bus connection string set to %s" serviceBusConnectionString

    let createTopic topicName = 
        if not (namespaceManager.TopicExists topicName) then
            logInfo "Topic %s not found. Creating new one..." topicName
            let description = TopicDescription(topicName)
            description.DefaultMessageTimeToLive <- messageTimeToLive
            namespaceManager.CreateTopic(description) |> ignore

    member this.topic topicName =
        logInfo "Creating publisher for topic %s" topicName
        createTopic topicName
        new TopicWrapper (TopicClient.CreateFromConnectionString(serviceBusConnectionString, topicName))

    member this.subscribe topicName subscriptionName =
        logInfo "Creating subscription %s to topic %s" subscriptionName topicName
        createTopic topicName

        if not (namespaceManager.SubscriptionExists(topicName, subscriptionName)) then
            logInfo "Subscription %s not found. Creating new one..." subscriptionName
            let description = SubscriptionDescription(topicName, subscriptionName)
            description.DefaultMessageTimeToLive <- messageTimeToLive
            namespaceManager.CreateSubscription(description) |> ignore

        SubscriptionClient.CreateFromConnectionString(serviceBusConnectionString, topicName, subscriptionName)