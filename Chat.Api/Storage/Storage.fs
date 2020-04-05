namespace Chat

open MongoDB.Bson.Serialization.Attributes
open MongoDB.Driver
open ClickHouse.Ado
open System.Data
open Chat.Db

[<RequireQualifiedAccess>]
module Storage =
  open MongoDB.Bson
  open MongoDB

  let private insertRecord (table:string) (record: 'T)= 
    async {
      let! db = DbLayer.MongoDB.getConnectionAsync()
      let collection = db.GetCollection<'T>(table)
      collection.InsertOne(record)
    }
  
  [<RequireQualifiedAccess>]
  module Users=
    let private table = "Users"
    type User={
      Id:System.Guid;
      Name: string;
    }

    let isExists(nick:string)=
      async {
        let! db = DbLayer.MongoDB.getConnectionAsync()
        let collection = db.GetCollection<User>(table)
 
        return 
          collection.Find(fun x -> x.Name=nick).ToList().Count>0
      } 

    let getRecordByName (nick:string) = 
      async {
        let! db = DbLayer.MongoDB.getConnectionAsync()
        let collection = db.GetCollection<User>(table)
 
        return collection.Find(fun x -> x.Name=nick).ToList()
      }

    let getAllUser() = 
      async {
        let! db = DbLayer.MongoDB.getConnectionAsync()
        let collection = db.GetCollection<User>(table)
        return collection.Find(fun _->true).ToList()
      }

    let addUser (nickname:string) = async{
      let record = {Id=System.Guid.NewGuid(); Name=nickname}
      do! insertRecord table record
    }

  [<RequireQualifiedAccess>]
  module Message=
    let table = "Messages"
    type Message={
      [<BsonId>]UserId:int;
      Text: string;
    } 
    
    let addMsg (userId:int, text:string) = async{
      let record = {UserId=userId; Text=text}
      do! insertRecord table record
    }

  [<RequireQualifiedAccess>]
  module Event =
    let private table  =
      "CREATE TABLE IF NOT EXISTS Events (timestamp DateTime, message String) ENGINE = StripeLog"
      |> DbLayer.ClickHouseDB.tableQueries.Add

    type Event ={
      Timestamp:System.DateTime
      Event: string
    }

    let addEvent = DbLayer.ClickHouseDB.addEvent 

    let inline private readOrdinal (reader:IDataReader) ord : 'a=
      if (typeof<'a> = typeof<string>) then
        reader.GetString ord |> unbox<'a>
      else
        reader.GetValue ord |> unbox<'a>

    let inline read (reader:IDataReader)  name : 'a=
        reader.GetOrdinal name |> readOrdinal reader

    let inline readValues (f:IDataReader -> 'a) (reader:IDataReader)  =
      [  
        while reader.NextResult() do
          while (reader.Read()) do yield f reader
      ]

    let getAllEvents() = 
      async {
        let read(reader: IDataReader) =
          let time = read reader "timestamp"
          let text = read reader "message"
          {Timestamp=time; Event=text}

        try
          let! conn = DbLayer.ClickHouseDB.getConnectionAsync()
          return
            conn.CreateCommand("SELECT * FROM Events ORDER BY timestamp DESC").ExecuteReader()
            |> readValues read
        with ex ->
          System.Console.WriteLine(sprintf "Error add event. Error:%A" ex)
          return List.empty
      }
