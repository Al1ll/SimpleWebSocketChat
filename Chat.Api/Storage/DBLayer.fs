namespace Chat.Storage 

[<RequireQualifiedAccess>]
module DbLayer =
  open System.IO
  open MongoDB.Driver

  type private Msg=
    | SetDb of string
    | GetConnection of AsyncReplyChannel<IMongoDatabase>

  let private dbCache = MailboxProcessor.Start(fun inbox->
    let rec loop (db,conn:IMongoDatabase option) =  async {
      match! inbox.Receive() with

      | SetDb newdb ->
        return! loop (newdb,conn)

      | GetConnection ch ->
         match conn with
         | Some x->
           ch.Reply x
           return! loop (db,conn)

         | None -> //mongodb://{User}:{Password}@{Host}:{Port}
                   //mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass%20Community&ssl=false
                   //mongodb://{User}:{Password}@{Host}:{Port}
                   let client =new MongoClient("mongodb://root:123456@192.168.99.100:27017")//mongodb://root:123456@192.168.99.100:27017
                   let conn =  client.GetDatabase(db)

                   ch.Reply conn
                   return! loop (db,Some conn)
                   
      return! loop (db,None)
    }
    loop ("db",None))

  let getConnectionAsync () =
    dbCache.PostAndAsyncReply(fun ch -> GetConnection ch)

  let setDb = Msg.SetDb >> dbCache.Post


//[<RequireQualifiedAccess>]
//module DbLayerEvents =
//  open MongoDB.Driver

//  type private Msg=
//    | SetDb of string
//    | GetConnection of AsyncReplyChannel<IMongoDatabase>

//  let private dbCache = MailboxProcessor.Start(fun inbox->
//    let rec loop (db,conn:IMongoDatabase option) =  async {
//      match! inbox.Receive() with

//      | SetDb newdb ->
//        return! loop (newdb,conn)

//      | GetConnection ch ->
//         match conn with
//         | Some x->
//           ch.Reply x
//           return! loop (db,conn)

//         | None ->
//                   let client =new MongoClient("mongodb://localhost:27017")
//                   let conn =  client.GetDatabase(db)

//                   ch.Reply conn
//                   return! loop (db,Some conn)
                   
//      return! loop (db,None)
//    }
//    loop ("db",None))

//  let getConnectionAsync () =
//    dbCache.PostAndAsyncReply(fun ch -> GetConnection ch)

//  let setDb = Msg.SetDb >> dbCache.Post

