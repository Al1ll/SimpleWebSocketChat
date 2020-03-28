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

  [<HttpGet>]
  [<Route("{nickname}")>]
  member this.Register(nickname:string) = async {
    match! Storage.Storage.Users.isExists nickname with
    | true -> return "this nickname already exists"
    | false -> 
        do! Storage.Storage.Users.addUser nickname
        return "Account created"
  }
    
  [<HttpGet>]
  [<Route("{nickname}/{roomId}")>]
  member this.Login(nickname:string, roomId:int)=
    let channelId=
      match Chat.Global.rooms.ContainsKey roomId with
      | true -> 
          Chat.Global.channels
          |> Seq.pick(fun p -> if p.Value=roomId then Some p.Key else None)
      | false ->
          Chat.Global.rooms.TryAdd(roomId, new Chat.Api.Models.ChatMessengeHandler())|>ignore
          Guid.NewGuid().ToString("N")

    Chat.Global.channels.TryAdd(channelId, roomId)|>ignore
    let port = 
      match this.Request.Host.Port.HasValue with
      | true -> sprintf ":%i" this.Request.Host.Port.Value
      | false -> String.Empty

    let url = sprintf "ws://%s%s/ws/%s" this.Request.Host.Host port channelId
    JsonResult(url)

  [<HttpGet>]
  member this.UserAll()= async {
    let! res = Storage.Storage.Users.getAllUser()
    return JsonResult(res)
  }