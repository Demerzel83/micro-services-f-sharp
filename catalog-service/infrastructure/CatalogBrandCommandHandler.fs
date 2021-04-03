module Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.CataloBrandCommandHandler

open Chessie.ErrorHandling
open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogBrandAggregate

module CatalogBrandCommandHanlder = 
    module private Helpers =
        let load = Events.loadAggregateEvents catelogBrandCategory 0
        let commit = Events.commitEvents catelogBrandCategory
        let dequeue () = Commands.dequeueCommands catalogBrandQueueName

    let handler = makeCatalogBrandCommandHandler Helpers.load Helpers.commit
    let processCommandQueue () = Seq.map handler <!> Helpers.dequeue ()