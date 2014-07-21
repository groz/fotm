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
let maxMessages = 100

let adminKeyConfig = System.Web.Configuration.WebConfigurationManager.AppSettings.["AdminKey"]

type Room = Room of string
type ChatMessage = ChatMessage of string
type UserId = UserId of string

type User = {
    id: UserId
    isAdmin: bool
}

type UserAvatar = {
    user: User
    chatAvatar: ChatAvatar
}

let getRoomName(Room name) = name

type ChatEvent =
| UserJoined of Room*User
| UserLeft of Room*UserId
| UserDisconnected of UserId
| MessageAdded of Room*UserId*ChatMessage

type ChatRoomEvent =
| UserJoinedRoom of User
| UserLeftRoom of UserId
| MessageAddedToRoom of UserId*ChatMessage
| UserDisconnectedFromRoom of UserId

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
        | UserLeft(room, userId) -> (getAgent room).Post(UserLeftRoom userId)
        | MessageAdded(room, userId, text)  -> (getAgent room).Post(MessageAddedToRoom(userId, text))
        | UserDisconnected(userId) ->
            rooms |> Map.iter(fun room chatRoom -> (getAgent room).Post(UserDisconnectedFromRoom userId))
    )

and ChatRoom(room, ctx: IHubContext) = 
    let roomName = getRoomName room
    let roomGroup = ctx.Clients.Group roomName

    let stringId(UserId id) = id
    let client(UserId id) = ctx.Clients.Client id

    let selectAvatar currentUserAvatars =
        let takenAvatars = currentUserAvatars |> Set.map(fun ua -> ua.chatAvatar)
        let availableAvatars = Set.difference allAvatars takenAvatars

        // TODO: recolor avatars when nothing's available?

        if availableAvatars.Count > 0 then
            let randomNumber = System.Random().Next(availableAvatars.Count)
            availableAvatars |> Seq.nth randomNumber
        else
            let randomNumber = System.Random().Next(allAvatars.Count)
            allAvatars |> Seq.nth randomNumber

    let getAvatar userId currentUserAvatars =
        currentUserAvatars |> Seq.tryFind(fun ua -> ua.user.id = userId)

    let messageProcessor = Agent.Start(fun mailbox ->
        logInfo "********* CREATED AGENT FOR %A ***************" room

        let rec loop currentAvatars messages = async {
            logInfo "%A waiting for message, state: %A, %A" room currentAvatars messages
            let! msg = mailbox.Receive()
            logInfo "%A started processing message %A, current state: %A, %A" room msg currentAvatars messages

            let userLeft userId isDisconnected =
                let strId = userId |> stringId

                match currentAvatars |> getAvatar userId with
                | Some userAvatar ->
                    if not isDisconnected then
                        ctx.Groups.Remove(strId, roomName) |> ignore
                    roomGroup ? userLeft(strId)
                    currentAvatars |> Set.remove userAvatar, messages
                | None ->
                    currentAvatars, messages

            let newUsers, newMessages = 
                try
                    match msg with
                    | UserJoinedRoom user ->
                        let chatAvatar = selectAvatar currentAvatars
                        let userAvatar = { user = user; chatAvatar = chatAvatar }
                        let strId = user.id |> stringId

                        let nextAvatars = currentAvatars |> Set.add userAvatar
                        (client user.id) ? setChatInfo(userAvatar, messages |> List.rev, nextAvatars)

                        roomGroup ? userJoined(userAvatar)
                        ctx.Groups.Add(strId, roomName) |> ignore

                        nextAvatars, messages

                    | UserLeftRoom userId -> 
                        userLeft userId false

                    | UserDisconnectedFromRoom userId ->
                        userLeft userId true

                    | MessageAddedToRoom(userId, text) -> 
                        match currentAvatars |> getAvatar userId with
                        | Some userAvatar ->
                            // if this user recently posted same message then discard it (anti-spam)
                            let chatMessage = userAvatar, text
                            let alreadyAdded = messages |> List.tryFind (fun m -> m = chatMessage)

                            match alreadyAdded with
                            | None ->
                                let roomWithoutSender = ctx.Clients.Group(roomName, userId |> stringId)
                                roomWithoutSender ? messageAdded(userAvatar, text)
                                currentAvatars, chatMessage :: messages |> Seq.truncate maxMessages |> List.ofSeq
                            | Some _ -> currentAvatars, messages

                        | None ->
                            currentAvatars, messages

                with
                | ex -> 
                    logError "Error occured in %A room processor: %A" room ex
                    currentAvatars, messages

            logInfo "%A finished processing message, result: %A, %A" room newUsers newMessages
            return! loop newUsers newMessages
        }

        loop Set.empty []
    )

    member this.agent = messageProcessor

and [<HubName("chatHub")>] ChatHub() =
    inherit Hub()

    member this.joinRoom(roomName: string, adminKey: string) =
        let callerId = this.Context.ConnectionId
        let roomName = roomName.ToUpper()
        let isAdmin = (adminKey = adminKeyConfig)
        let user = { User.id = UserId callerId; User.isAdmin = isAdmin }
        let chatEvent = UserJoined(Room roomName, user)
        ChatAgent.Instance.Post chatEvent

    member this.leaveRoom(roomName: string)=
        let callerId = this.Context.ConnectionId
        let roomName = roomName.ToUpper()
        let chatEvent = UserLeft(Room roomName, UserId callerId)
        ChatAgent.Instance.Post chatEvent

    override this.OnDisconnected() =
        let callerId = this.Context.ConnectionId
        let chatEvent = UserDisconnected(UserId callerId)
        ChatAgent.Instance.Post chatEvent
        base.OnDisconnected()

    member this.message(roomName: string, text: string) =
        if (text.Length <> 0) then
            let callerId = this.Context.ConnectionId
            let roomName = roomName.ToUpper()
            let text = text.Substring(0, System.Math.Min(text.Length, maxMessageLength))
            let chatEvent = MessageAdded(Room roomName, UserId callerId, ChatMessage(text))
            ChatAgent.Instance.Post chatEvent