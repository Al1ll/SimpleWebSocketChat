namespace Chat.Api.Controllers

open Microsoft.AspNetCore.Mvc
open Chat

[<ApiController>]
[<Route("api/[controller]/[action]")>]
type EventsController()=
  inherit ControllerBase()

  [<HttpGet>]
  member this.All()= async {
    Storage.Event.addEvent "call host://api/user/all"
    let! res = Storage.Event.getAllEvents()
    return this.Ok(res)
  }