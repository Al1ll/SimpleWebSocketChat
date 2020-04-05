namespace Chat.Api.Controllers

open Microsoft.AspNetCore.Mvc
open System
open System.Net.WebSockets
open System.Threading
open Chat

[<ApiController>]
//[<Route("ws/")>]
type ChannelController()=
  inherit ControllerBase()

  let recieve (ws:WebSocket) (f:WebSocketReceiveResult->byte array->Async<unit>) = async {
    let buffer = Array.zeroCreate (1024*4)
    while (ws.State = WebSocketState.Open) do
      let! result = ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None)|> Async.AwaitTask
      do! f result buffer
  }


  [<HttpGet>]
  [<Route("~/{nickname}/ws/{channelId}")>]
  member this.Connect nickname channelId = async {
    if this.HttpContext.WebSockets.IsWebSocketRequest then
      if Global.channels.ContainsKey channelId then
        let roomId = Global.channels.Item channelId
        let sh = Global.rooms.Item roomId
        let! ws = this.HttpContext.WebSockets.AcceptWebSocketAsync()|> Async.AwaitTask
        do! sh.OnConnected nickname ws

        let f (result:WebSocketReceiveResult) buffer = async {
          match result.MessageType with
          | WebSocketMessageType.Text when result.EndOfMessage -> do! sh.Recieve nickname result buffer
          | WebSocketMessageType.Text//if EndOfMessage is False
          | WebSocketMessageType.Close-> 
              do! sh.OnDisconnet nickname 
              if sh.IsEmpty() then
                Global.rooms.TryRemove roomId |> ignore
                Global.channels.TryRemove channelId |> ignore 
          |_ -> ()
        }          

        do! recieve ws f
  }
