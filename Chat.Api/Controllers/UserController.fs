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
  inherit ControllerBase()

  [<HttpPost>]
  member this.Register(nickname:string):Async<IActionResult> = async {
    let nick = this.HttpContext.Session.GetString("1")
    if not<|(String.IsNullOrEmpty nickname) then
        match! Storage.Storage.Users.isExists nickname with
        | true -> return (this.BadRequest("this nickname already exists"):>IActionResult)
        | false -> 
            do! Storage.Storage.Users.addUser nickname
            return (this.Ok("Account created"):>IActionResult)
    else
      return (this.BadRequest():>IActionResult)
  }
    

  [<HttpPost>]
  [<Route("{nickname}/{roomId}")>]
  member this.Login(nickname:string, roomId:int):Async<IActionResult>= async {
    if (not<|String.IsNullOrEmpty nickname) && roomId>=0 then
      do! Storage.Storage.Event.addOneEvent(sprintf "login: %s in room %i" nickname roomId)
      match! Storage.Storage.Users.isExists nickname with
      | true ->
        let channelId=
            match Chat.Global.rooms.ContainsKey roomId with
            | true -> 
                Chat.Global.channels
                |> Seq.pick(fun p -> if p.Value=roomId then Some p.Key else None)
            | false ->
                Chat.Global.rooms.TryAdd(roomId, new Chat.Api.Models.ChatMessengeHandler())|>ignore
                Guid.NewGuid().ToString("N")

        do! Storage.Storage.Event.addOneEvent(sprintf "return sessionId: %s for %s in room %i" channelId nickname roomId)

        Chat.Global.channels.TryAdd(channelId, roomId)|>ignore
        let port = 
          match this.Request.Host.Port.HasValue with
          | true -> sprintf ":%i" this.Request.Host.Port.Value
          | false -> String.Empty

        //let url = sprintf "ws://%s%s/ws/%s" this.Request.Host.Host port channelId
        //Session.Set Get не работают потому что я не могу извлечь данные в ChannelControlle т.к. там используется вебсокет
        let url = sprintf "ws://%s%s/%s/ws/%s" this.Request.Host.Host port nickname channelId
        return (this.Ok(url):> IActionResult)
      | false -> return (this.NotFound("User is not registered"):> IActionResult)
    else
      do! Storage.Storage.Event.addOneEvent(sprintf "some parameters are missing Nickname:%s roomId:%i" nickname roomId)
      return (this.BadRequest("Please set nickname and roomId>=0"):> IActionResult)
  }

  [<HttpGet>]
  member this.All()= async {
    let! res = Storage.Storage.Users.getAllUser()
    return this.Ok(res)
  }