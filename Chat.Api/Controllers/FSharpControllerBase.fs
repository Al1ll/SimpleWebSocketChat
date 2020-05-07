namespace Chat.Api.Controllers

open Microsoft.AspNetCore.Mvc
open Chat

type FSharpControllerBase()=
  inherit Controller()

  member this.Ok()=
    base.Ok() :> IActionResult

  member this.Ok(value:obj)=
    base.Ok(value) :> IActionResult

  member this.BadRequest()=
    base.BadRequest():> IActionResult

  member this.BadRequest(modelState:ModelBinding.ModelStateDictionary)=
      base.BadRequest(modelState):> IActionResult

  member this.BadRequest(error:obj)=
      base.BadRequest(error):> IActionResult

  member this.NotFound()=
    base.NotFound() :> IActionResult

  member this.NotFound(value:obj)=
    base.NotFound(value) :> IActionResult
