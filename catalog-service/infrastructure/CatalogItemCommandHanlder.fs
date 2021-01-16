namespace Microsoft.eShopOnContainers.Services.Catalog.CommandHandler

open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.SqlServer
open Microsoft.eShopOnContainers.Services.Catalog.API.CatalogItemAggregate

module CatalogItemCommandHanlder = 
    module private Helpers =
        let load = Events.loadAggregateEvents catelogItemCategory 0
        let commit = Events.commitEvents catelogItemCategory
        let dequeue () = Commands.dequeueCommands catalogItemQueueName

    let handler = makeCatalogItemCommandHandler Helpers.load Helpers.commit
    let processCommandQueue () = Seq.map handler <!> Helpers.dequeue ()