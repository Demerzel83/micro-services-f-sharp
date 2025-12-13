namespace Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.CommandHandler

open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.EventsDB
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogItemAggregate
open TaskResult

module CatalogItemCommandHanlder = 
    module private Helpers =
        let load = Events.loadAggregateEvents catelogItemCategory 0
        let commit = Events.commitEvents catelogItemCategory
        let dequeue () = Commands.dequeueCommands catalogItemQueueName

    let handler = makeCatalogItemCommandHandler Helpers.load Helpers.commit
    let processCommandQueue () = taskResult {
        let! commands =  Helpers.dequeue ()
        let! result = Seq.fold(fun acc x -> 
            taskResult {
                let! result = handler x
                let! values = acc
                return (result @ values) }) (TaskResult.rtn List.empty) commands
        return result
    }