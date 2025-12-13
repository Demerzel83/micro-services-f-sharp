module Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.CatalogTypeCommandHandler

open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.EventsDB
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogTypeAggregate
open TaskResult

module CatalogTypeCommandHanlder = 
    module private Helpers =
        let load = Events.loadAggregateEvents catelogTypeCategory 0
        let commit = Events.commitEvents catelogTypeCategory
        let dequeue () = Commands.dequeueCommands catalogTypeQueueName

    let handler = makeCatalogTypeCommandHandler Helpers.load Helpers.commit
    let processCommandQueue () = taskResult {
        let! commands =  Helpers.dequeue ()
        let! result = Seq.fold(fun acc x -> 
            taskResult {
                let! result = handler x
                let! values = acc
                return (result @ values) }) (TaskResult.rtn List.empty) commands
        return result
    }