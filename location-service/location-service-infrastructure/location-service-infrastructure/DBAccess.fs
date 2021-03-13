namespace  Microsoft.eShopOnContainers.Services.Location.Infrastructure

open MongoDB.Bson
open MongoDB.Driver

open Microsoft.eShopOnContainers.Services.Location.Core.LocationTypes

module DBAccess =
    [<Literal>]
    let ConnectionString = "mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass%20Community&ssl=false"

    [<Literal>]
    let DbName = "LocationsDb"

    [<Literal>]
    let CollectionName = "Locations"

    let client = MongoClient(ConnectionString)
    let db = client.GetDatabase(DbName)
    let testCollection = db.GetCollection<Locations>(CollectionName)

    let create (location:Locations) =
        testCollection.InsertOne(location)

    let createMany (locations:Locations list) =
        testCollection.InsertMany (locations)

    let readOnId (id:BsonObjectId) =
        testCollection.Find(fun x -> x.Id = id).ToEnumerable()

    let readOnCodeAndId (id:BsonObjectId) (code:string) =
        testCollection.Find(fun x -> x.Id = id && x.Code = code).ToEnumerable()

    let readAll () =
        testCollection.Find(Builders.Filter.Empty).ToEnumerable()

    let updateOneOnCode (codeToUpdateFrom:string) (codeToUpdateTo:string) =
        let filter = 
            Builders<Locations>.Filter.Eq((fun x -> x.Code), codeToUpdateFrom)
        let updateDefinition = 
            Builders<Locations>.Update.Set((fun x -> x.Code), codeToUpdateTo)

        testCollection.UpdateOne(filter, updateDefinition)

    let deletedOnId (id:BsonObjectId) =
        testCollection.DeleteOne(fun x -> x.Id = id)

    let deleteManyOnId (id:BsonObjectId) =
        testCollection.DeleteMany(fun x -> x.Id = id)

    let delateAll () =
        testCollection.DeleteMany(Builders.Filter.Empty)