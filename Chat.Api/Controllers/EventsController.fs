namespace Chat.Api.Controllers

open Microsoft.AspNetCore.Mvc
open Chat

[<ApiController>]
[<Route("api/[controller]/[action]")>]
type EventsController()=
  inherit ControllerBase()

  [<HttpGet>]
  member this.All()= async {
    //let! res = Storage.Storage.Event.getAllEvents()
    //return this.Ok(res)
    return this.Ok()
  }