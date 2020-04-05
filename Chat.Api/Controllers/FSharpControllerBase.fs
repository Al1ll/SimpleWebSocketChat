namespace Chat.Api.Controllers

open Microsoft.AspNetCore.Mvc
open Chat

[<AutoOpen>]
module AAAA = 
  let inline GetBodyAsync x = (^a: (member GetBodyAsync: unit -> ^b) x)
  
  type A() =
      member this.GetBodyAsync() = System.Threading.Tasks.Task.FromResult 1
  
  type B() =
      member this.GetBodyAsync() = async { return 2 }

  let s() = 
    let z = A() |> GetBodyAsync |> fun x -> x.Result
    let z2 = B() |> GetBodyAsync |> Async.RunSynchronously

    z
  let inline ZZZZ x = (^a: (member Ok: unit -> OkResult) x)

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