module FotM.Apollo.ChatModule

open Microsoft.AspNet.SignalR
open Microsoft.AspNet.SignalR.Hubs
open EkonBenefits.FSharp.Dynamic
open FotM.Hephaestus.Async
open FotM.Data
open FotM.Hephaestus.TraceLogging

type ChatAvatar = ChatAvatar of Race * Gender * Class

let allAvatars = 
    [
        for race in Races.all do 
        for c in Classes.all do // TODO: restrict classes to those playable for race
        for gender in Genders.all do
        yield ChatAvatar(race, gender, c)
    ] |> Set.ofList

let maxMessageLength = 140
let maxMessages = 5

type Room = Room of string
type ChatMessage = ChatMessage of string
type User = User of string
type UserAvatar = UserAvatar of User * ChatAvatar

let getRoomName(Room name) = name

type ChatEvent =
| UserJoined of Room*User
| UserLeft of Room*User
| UserDisconnected of User
| MessageAdded of Room*User*ChatMessage

type ChatRoomEvent =
| UserJoinedRoom of User
| UserLeftRoom of User
| MessageAddedToRoom of User*ChatMessage
| UserDisconnectedFromRoom of User

let startAgent<'Message> processMessage = Agent<'Message>.Start(fun mailbox ->
    let rec loop() = async {
        let! msg = mailbox.Receive()
        processMessage msg
        return! loop()
    }
    loop()
)

type ChatAgent() =
    static let ctx = lazy GlobalHost.ConnectionManager.GetHubContext<ChatHub>()

    static let rooms = 
        [ for r in Regions.all do
              let room = Room(r.code)
              yield room, ChatRoom(room, ctx.Value)
        ] 
        |> Map.ofSeq

    static let getAgent room : Agent<ChatRoomEvent> =
        rooms.[room].agent

    static member Instance = startAgent (function
        | UserJoined(room, user) -> (getAgent room).Post(UserJoinedRoom user)
        | UserLeft(room, user) -> (getAgent room).Post(UserLeftRoom user)
        | MessageAdded(room, user, text)  -> (getAgent room).Post(MessageAddedToRoom(user, text))
        | UserDisconnected(user) ->
            rooms |> Map.iter(fun room chatRoom -> (getAgent room).Post(UserDisconnectedFromRoom user))
    )

and ChatRoom(room, ctx: IHubContext) = 
    let roomName = getRoomName room
    let roomGroup = ctx.Clients.Group roomName

    let connectionId (User id) = id
    let client (User id) = ctx.Clients.Client id

    let selectAvatar currentUserAvatars =
        let takenAvatars = currentUserAvatars |> Set.map(function UserAvatar(u,a) -> a)
        let availableAvatars = Set.difference allAvatars takenAvatars

        // TODO: recolor avatars when nothing's available?

        if availableAvatars.Count > 0 then
            let randomNumber = System.Random().Next(availableAvatars.Count)
            availableAvatars |> Seq.nth randomNumber
        else
            let randomNumber = System.Random().Next(allAvatars.Count)
            allAvatars |> Seq.nth randomNumber

    let getAvatar (User id) currentUserAvatars =
        currentUserAvatars |> Seq.tryFind(function UserAvatar (User userId, avatar) -> userId = id)

    let messageProcessor = Agent.Start(fun mailbox ->
        logInfo "********* CREATED AGENT FOR %A ***************" room

        let rec loop userAvatars messages = async {
            logInfo "%A waiting for message, state: %A, %A" room userAvatars messages
            let! msg = mailbox.Receive()
            logInfo "%A started processing message %A, current state: %A, %A" room msg userAvatars messages

            let userLeft user isDisconnected =
                let userId = connectionId user
                        
                match userAvatars |> getAvatar user with
                | Some userAvatar ->
                    if not isDisconnected then
                        ctx.Groups.Remove(userId, roomName).Wait() |> ignore
                    roomGroup ? userLeft(userId)
                    userAvatars |> Set.remove userAvatar, messages
                | None ->
                    userAvatars, messages

            let newUsers, newMessages = 
                try
                    match msg with
                    | UserJoinedRoom(user) ->
                        let userId = connectionId user

                        let chatAvatar = selectAvatar userAvatars
                        let userAvatar = UserAvatar(user, chatAvatar)

                        ctx.Groups.Add(userId, roomName) |> ignore

                        (client user) ? setChatInfo(chatAvatar, messages |> List.rev)
                        roomGroup ? userJoined(userId, chatAvatar)

                        let nextAvatars = userAvatars |> Set.add userAvatar
                        nextAvatars, messages
                    | UserLeftRoom(user) -> 
                        userLeft user false
                    | UserDisconnectedFromRoom user ->
                        userLeft user true
                    | MessageAddedToRoom(user, text) -> 
                        let userId = connectionId user

                        match userAvatars |> getAvatar user with
                        | Some userAvatar ->
                            let roomWithoutSender = ctx.Clients.Group(roomName, userId)
                            roomWithoutSender ? messageAdded(userId, userAvatar, text)
                            userAvatars, (userAvatar, text) :: messages |> Seq.truncate maxMessages |> List.ofSeq
                        | None ->
                            userAvatars, messages
                with
                | ex -> 
                    logError "Error occured in %A room processor: %A" room ex
                    userAvatars, messages

            logInfo "%A finished processing message, result: %A, %A" room newUsers newMessages
            return! loop newUsers newMessages
        }

        loop Set.empty []
    )

    member this.agent = messageProcessor

and [<HubName("chatHub")>] ChatHub() =
    inherit Hub()

    member this.joinRoom(roomName: string) =
        let callerId = this.Context.ConnectionId
        let roomName = roomName.ToUpper()
        let chatEvent = UserJoined(Room roomName, User callerId)
        ChatAgent.Instance.Post chatEvent

    member this.leaveRoom(roomName: string)=
        let callerId = this.Context.ConnectionId
        let roomName = roomName.ToUpper()
        let chatEvent = UserLeft(Room roomName, User callerId)
        ChatAgent.Instance.Post chatEvent

    override this.OnDisconnected() =
        let callerId = this.Context.ConnectionId
        let chatEvent = UserDisconnected(User callerId)
        ChatAgent.Instance.Post chatEvent
        base.OnDisconnected()

    member this.message(roomName: string, text: string) =
        if (text.Length <> 0) then
            let callerId = this.Context.ConnectionId
            let roomName = roomName.ToUpper()
            let text = text.Substring(0, System.Math.Min(text.Length, maxMessageLength))
            let chatEvent = MessageAdded(Room roomName, User callerId, ChatMessage(text))
            ChatAgent.Instance.Post chatEvent