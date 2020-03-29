namespace Chat.Api.Models

open System.Collections.Concurrent
open System.Net.WebSockets
open System.Text
open System
open System.Threading
open Chat.Storage
type ChatMessengeHandler()=
  let sockets = new ConcurrentDictionary<string, WebSocket>()  

  member this.SendMessageToAll(msg:string)= async {
    sockets.Values
    |> Seq.iter(fun ws -> 
      match ws.State with
      | WebSocketState.Open -> 
          ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg),0,msg.Length),WebSocketMessageType.Text,true,CancellationToken.None)|>Async.AwaitTask|> Async.RunSynchronously
      | _ ->()

    )
  }

  member this.Recieve (nickname:string) (wsResult:WebSocketReceiveResult) (buffer:byte array)= async {
    do! this.SendMessageToAll((sprintf "%s said: %s" nickname (Encoding.UTF8.GetString(buffer,0,wsResult.Count))))
  }

  member this.OnConnected(nickname:string) (ws:WebSocket)=
    sockets.TryAdd(nickname,ws)|>ignore
    //do! Storage.Event.addOneEvent(sprintf "%s on connected" nickname)
    this.SendMessageToAll(sprintf "%s is connected" nickname)

  member this.OnDisconnet(nickname:string)= async {
    try
      let _,ws = sockets.TryRemove nickname
      do! ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None)|>Async.AwaitTask
      do! this.SendMessageToAll (sprintf "%s is disconnected" nickname)
      do! Storage.Event.addOneEvent(sprintf "%s is disconnected" nickname)
    with ex->
      do! Storage.Event.addOneEvent(sprintf "Error: %A" ex)
  }

  member this.IsEmpty()= sockets.IsEmpty

  