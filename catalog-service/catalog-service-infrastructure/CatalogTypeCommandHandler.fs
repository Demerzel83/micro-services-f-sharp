module Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.CatalogTypeCommandHandler

open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogTypeAggregate

module CatalogTypeCommandHanlder = 
    module private Helpers =
        let load = Events.loadAggregateEvents catelogTypeCategory 0
        let commit = Events.commitEvents catelogTypeCategory
        let dequeue () = Commands.dequeueCommands catalogTypeQueueName

    let handler = makeCatalogTypeCommandHandler Helpers.load Helpers.commit
    let processCommandQueue () = Seq.map handler <!> Helpers.dequeue ()