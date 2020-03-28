namespace Chat.Storage 

open MongoDB.Bson.Serialization.Attributes
open MongoDB.Driver

module Storage =
  open MongoDB.Bson
  open MongoDB

  //let createCommandAsync query =
  //    async {
  //      let! connection = DbLayer.getConnectionAsync()
  //      return new SqliteCommand(query, connection)
  //    }

  let insertRecord (table:string) (record: 'T)= 
    async {
      let! db = DbLayer.getConnectionAsync()
      let collection = db.GetCollection<'T>(table)
      collection.InsertOne(record)
    }

  //let insertRecord
  
  [<RequireQualifiedAccess>]
  module Users=
    let table = "Users"
    type User={
      Id:System.Guid;
      Name: string;
    }

    let isExists(nick:string)=
      async {
        let! db = DbLayer.getConnectionAsync()
        let collection = db.GetCollection<User>(table)
 
        return 
          collection.Find(fun x -> x.Name=nick).ToList().Count>0
      } 

    let getRecordByName (nick:string) = 
      async {
        let! db = DbLayer.getConnectionAsync()
        let collection = db.GetCollection<User>(table)
 
        return collection.Find(fun x -> x.Name=nick).ToList()
      }

    let getAllUser() = 
      async {
        let! db = DbLayer.getConnectionAsync()
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


  

