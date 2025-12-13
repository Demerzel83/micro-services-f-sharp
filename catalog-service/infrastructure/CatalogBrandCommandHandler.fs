module Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.CataloBrandCommandHandler

open Microsoft.eShopOnContainers.Services.Catalog.Infrastructure.SqlServer.EventsDB
open Microsoft.eShopOnContainers.Services.Catalog.Core.CatalogBrandAggregate
open TaskResult

module CatalogBrandCommandHanlder = 
    module private Helpers =
        let load = Events.loadAggregateEvents catelogBrandCategory 0
        let commit = Events.commitEvents catelogBrandCategory
        let dequeue () = Commands.dequeueCommands catalogBrandQueueName

    let handler = makeCatalogBrandCommandHandler Helpers.load Helpers.commit
    let processCommandQueue () = taskResult {
        let! commands =  Helpers.dequeue ()
        let! result = Seq.fold(fun acc x -> 
            taskResult {
                let! result = handler x
                let! values = acc
                return (result @ values) }) (TaskResult.rtn List.empty) commands
        return result
    }