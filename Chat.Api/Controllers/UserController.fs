namespace Chat.Api.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Chat.Api
open Chat

[<ApiController>]
[<Route("api/[controller]/[action]")>]
type UserController()=
  inherit ControllerBase()

  [<HttpPost>]
  [<Route("")>]
  member this.Register(nickname:string) = async {
    if not<|(String.IsNullOrEmpty nickname) then
      match! Storage.Storage.Users.isExists nickname with
      | true -> return "this nickname already exists"
      | false -> 
          do! Storage.Storage.Users.addUser nickname
          return "Account created"
    else
      return "Error"
  }
    
  //[<HttpGet>]
  //[<Route("{nickname}/{roomId}")>]
  [<HttpPost>]
  [<Route("")>]
  member this.Login(nickname:string, sessionId:int)=
    if (not<|String.IsNullOrEmpty nickname) && sessionId>=0 then
      let channelId=
          match Chat.Global.rooms.ContainsKey sessionId with
          | true -> 
              Chat.Global.channels
              |> Seq.pick(fun p -> if p.Value=sessionId then Some p.Key else None)
          | false ->
              Chat.Global.rooms.TryAdd(sessionId, new Chat.Api.Models.ChatMessengeHandler())|>ignore
              Guid.NewGuid().ToString("N")

      Chat.Global.channels.TryAdd(channelId, sessionId)|>ignore
      let port = 
        match this.Request.Host.Port.HasValue with
        | true -> sprintf ":%i" this.Request.Host.Port.Value
        | false -> String.Empty

      let url = sprintf "ws://%s%s/ws/%s" this.Request.Host.Host port channelId
      JsonResult(url)
    else
      JsonResult("Please set nickname and roomId>=0")

  [<HttpGet>]
  member this.UserAll()= async {
    let! res = Storage.Storage.Users.getAllUser()
    return JsonResult(res)
  }