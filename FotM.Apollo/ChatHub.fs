module FotM.Apollo.ChatModule

open Microsoft.AspNet.SignalR
open Microsoft.AspNet.SignalR.Hubs
open EkonBenefits.FSharp.Dynamic
open FotM.Hephaestus.Async
open FotM.Data

type Room = Room of string
type User = User of string
type ChatMessage = ChatMessage of string

let getRoomName(Room name) = name

type ChatEvent =
| UserJoined of Room*User
| UserLeft of Room*User
| MessageAdded of Room*User*ChatMessage

type ChatRoomEvent =
| UserJoinedRoom of User
| UserLeftRoom of User
| MessageAddedToRoom of User*ChatMessage

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
    )

and ChatRoom(room, ctx: IHubContext) = 
    let roomName = getRoomName room
    let roomGroup = ctx.Clients.Group roomName

    let connectionId (User id) = id

    member this.agent = startAgent (function
        | UserJoinedRoom(user) ->
            roomGroup ? userJoined(user)
            ctx.Groups.Add(connectionId user, roomName) |> ignore

        | UserLeftRoom(user) -> 
            ctx.Groups.Remove(connectionId user, roomName).Wait()
            roomGroup ? userLeft(user)

        | MessageAddedToRoom(user, text) -> 
            roomGroup ? messageAdded(user, text)
    )

and [<HubName("chatHub")>] ChatHub() =
    inherit Hub()

    member this.joinRoom roomName =
        let callerId = this.Context.ConnectionId
        let chatEvent = UserJoined(Room roomName, User callerId)
        ChatAgent.Instance.Post chatEvent