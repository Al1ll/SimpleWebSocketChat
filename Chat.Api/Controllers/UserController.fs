namespace Chat.Api.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Chat.Api
open Chat

[<ApiController>]
[<Route("api/[controller]/[action]")>]
type UserController()=
  inherit FSharpControllerBase()

  [<HttpPost>]
  member this.Register nickname = async {
    if not<|(String.IsNullOrEmpty nickname) then
      Storage.Event.addEvent(sprintf "try register - nickname: %s" nickname)
      match! Storage.Users.isExists nickname with
      | true -> return this.BadRequest("this nickname already exists")
      | false -> 
         do! Storage.Users.addUser nickname
         Storage.Event.addEvent(sprintf "%s is registered" nickname)
         return this.Ok("Account created")
    else
      return this.BadRequest()
  }

  [<HttpPost>]
  member this.Login nickname roomId= async {
    if (not<|String.IsNullOrEmpty nickname) && roomId>=0 then
      Storage.Event.addEvent(sprintf "try login: %s in room %i" nickname roomId)
      match! Storage.Users.isExists nickname with
      | true ->
        let channelId=
            match Chat.Global.rooms.ContainsKey roomId with
            | true -> 
                Chat.Global.channels
                |> Seq.pick(fun p -> if p.Value=roomId then Some p.Key else None)
            | false ->
                Chat.Global.rooms.TryAdd(roomId, new Chat.Api.Models.ChatMessengeHandler())|>ignore
                Guid.NewGuid().ToString("N")
        Storage.Event.addEvent (sprintf "%s is logined in room %i" nickname roomId)

        Chat.Global.channels.TryAdd(channelId, roomId)|>ignore
        let port = 
          match this.Request.Host.Port.HasValue with
          | true -> sprintf ":%i" this.Request.Host.Port.Value
          | false -> String.Empty

        //let url = sprintf "ws://%s%s/ws/%s" this.Request.Host.Host port channelId
        //Session.Set Get не работают потому что я не могу извлечь данные в ChannelController т.к. там используется вебсокет
        let url = sprintf "ws://%s%s/%s/ws/%s" this.Request.Host.Host port nickname channelId
        Storage.Event.addEvent (sprintf "return sessionId: %s for %s in room %i" channelId nickname roomId)
        return this.Ok(url)
      | false -> return this.NotFound("User is not registered")
    else
      Storage.Event.addEvent(sprintf "some parameters are missing Nickname:%s roomId:%i" nickname roomId)
      return this.BadRequest("Please set nickname and roomId>=0")
  }

  [<HttpGet>]
  member this.All()= async {
    Storage.Event.addEvent "call host://api/user/all"
    let! res = Storage.Users.getAllUser()
    return this.Ok(res)
  }