module Microsoft.eShopOnContainers.Services.Catalog.CatalogTypeCommandHandler

open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.SqlServer
open Microsoft.eShopOnContainers.Services.Catalog.API.CatalogTypeAggregate

module CatalogTypeCommandHanlder = 
    module private Helpers =
        let load = Events.loadAggregateEvents catelogTypeCategory 0
        let commit = Events.commitEvents catelogTypeCategory
        let dequeue () = Commands.dequeueCommands catalogTypeQueueName

    let handler = makeCatalogTypeCommandHandler Helpers.load Helpers.commit
    let processCommandQueue () = Seq.map handler <!> Helpers.dequeue ()