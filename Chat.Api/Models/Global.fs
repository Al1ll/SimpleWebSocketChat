namespace Chat

open System.Collections.Concurrent
open System.Net.WebSockets
open Chat.Api.Models

module Global=
  let rooms = new ConcurrentDictionary<int, ChatMessengeHandler>()
  let channels = new ConcurrentDictionary<string, int>()
  

