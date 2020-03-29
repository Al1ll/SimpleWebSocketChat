namespace Chat.Storage 

[<RequireQualifiedAccess>]
module DbLayer =
  open System.IO
  open MongoDB.Driver

  type private Msg=
    | SetDb of string
    | GetDb of AsyncReplyChannel<string>
    | GetConnection of AsyncReplyChannel<IMongoDatabase>

  let private dbCache = MailboxProcessor.Start(fun inbox->
    let rec loop (db,conn:IMongoDatabase option) =  async {
      match! inbox.Receive() with

      | SetDb newdb ->
        return! loop (newdb,conn)

      | GetDb ch ->
        ch.Reply db

      | GetConnection ch ->
         match conn with
         | Some x->
           ch.Reply x
           return! loop (db,conn)

         | None ->
                   let client =new MongoClient()
                   let conn =  client.GetDatabase(db)

                   ch.Reply conn
                   return! loop (db,Some conn)
                   
      return! loop (db,None)
    }
    loop ("db",None))

  let getDatabase() =
    dbCache.PostAndReply(fun ch->GetDb ch)

  let getConnectionAsync () =
    dbCache.PostAndAsyncReply(fun ch -> GetConnection ch)

  let setDb = Msg.SetDb >> dbCache.Post

