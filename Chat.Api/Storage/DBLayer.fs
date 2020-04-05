namespace Chat.Db

[<RequireQualifiedAccess>]
module DbLayer =

  module MongoDB =
    open MongoDB.Driver

    type private Msg=
      | SetDb of string
      | GetConnection of AsyncReplyChannel<IMongoDatabase>

    let private dbCache = MailboxProcessor.Start(fun inbox->
      let rec loop (db,conn) =  async {
        match! inbox.Receive() with

        | SetDb newdb ->
          return! loop (newdb,conn)

        | GetConnection ch ->
           match conn with
           | Some x->
             ch.Reply x
             return! loop (db,conn)

           | None ->
                     //mongodb://{User}:{Password}@{Host}:{Port}
                     //mongodb://root:123456@192.168.99.100:27017
                     let client =new MongoClient("mongodb://root:123456@192.168.99.100:27017")
                     let conn =  client.GetDatabase(db)

                     ch.Reply conn
                     return! loop (db,Some conn)
                     
        return! loop (db,None)
      }
      loop ("db",None))

    let getConnectionAsync () =
      dbCache.PostAndAsyncReply(fun ch -> GetConnection ch)

    let setDb = Msg.SetDb >> dbCache.Post
  
  module ClickHouseDB = 
    open ClickHouse.Ado

    module DateTime=
      let Now() = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")

    let tableQueries = ResizeArray<string>()
    //let addTableQueries = tableQueries.Add

    type private Msg=
    | GetConnection of AsyncReplyChannel<ClickHouseConnection>
    | AddEvent of string

    let private fWrite text (cnn:ClickHouseConnection) = 
        try 
          let query = sprintf "INSERT INTO Events (timestamp, message) VALUES ('%s','%s')" (DateTime.Now()) text
          let cmd = cnn.CreateCommand(query)
          cmd.ExecuteNonQuery()|>ignore
        with ex ->
          System.Console.WriteLine(sprintf "Error add event. Error:%A" ex)

    let private getConnection tableQueries=  async {
      let cstr = "Compress=False;BufferSize=32768;SocketTimeout=10000;CheckCompressedHash=False;Compressor=lz4;Host=192.168.99.100;Port=9000;Database=default;User=default;Password="
      let settings = new ClickHouseConnectionSettings(cstr)
      let conn = new ClickHouseConnection(settings)
      conn.Open()
  
      tableQueries
      |> Seq.iter (fun query ->
        try
          conn.CreateCommand(query).ExecuteNonQuery()|>ignore
        with exn ->
          System.Diagnostics.Debug.Fail(sprintf "[ClickHouse] Exception %A" exn)
      )

      return conn
    }

    let private db = MailboxProcessor.Start(fun inbox->
      let rec loop (conn) =  async {
          match! inbox.Receive() with
          | GetConnection ch ->
              match conn with
              | Some x->
                  ch.Reply x
                  return! loop (conn)
              | None ->
                  
                  let! conn = getConnection tableQueries
                  ch.Reply conn
                  return! loop (Some conn)

          | AddEvent text ->  
              match conn with
              | Some cnn ->
                  fWrite text cnn
                  return! loop (conn) 
              | None -> 
                  let! cnn = getConnection tableQueries
                  fWrite text cnn

                  return! loop (Some cnn)
          | _ -> return! loop (conn)
      }
        loop (None))
    
    let getConnectionAsync () = db.PostAndAsyncReply(fun ch -> GetConnection ch)
    
    let addEvent event = db.Post(AddEvent event)

