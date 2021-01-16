module Microsoft.eShopOnContainers.Services.Catalog.CataloBrandCommandHandler

open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.SqlServer
open Microsoft.eShopOnContainers.Services.Catalog.API.CatalogBrandAggregate

module CatalogBrandCommandHanlder = 
    module private Helpers =
        let load = Events.loadAggregateEvents catelogBrandCategory 0
        let commit = Events.commitEvents catelogBrandCategory
        let dequeue () = Commands.dequeueCommands catalogBrandQueueName

    let handler = makeCatalogBrandCommandHandler Helpers.load Helpers.commit
    let processCommandQueue () = Seq.map handler <!> Helpers.dequeue ()