# ordering_service

A [Giraffe](https://github.com/giraffe-fsharp/Giraffe) web application, which has been created via the `dotnet new giraffe` command.

## Build and test the application

### Windows

Run the `build.bat` script in order to restore, build and test (if you've selected to include tests) the application:

```
> ./build.bat
```

### Linux/macOS

Run the `build.sh` script in order to restore, build and test (if you've selected to include tests) the application:

```
$ ./build.sh
```

## Run the application

After a successful build you can start the web application by executing the following command in your terminal:

```
dotnet run src/ordering_service
```

After the application has started visit [http://localhost:5000](http://localhost:5000) in your preferred browser.

Notes:
cqrs

database: sql server

Notes:
    using IMediator

Grpc:
    - Exposes CreateOrderDraftFromBasketData
 Commands:
    - CancelOrderCommand
    - CreateOrderCommand
    - CreateOrderDraftCommand
    - IdentifiedCommand
    - SetAwaitingValidationOrderStatusCommand
    - SetPaidOrderStatusCommand
    - SetStockConfirmedOrderStatusCommand
    - SetStockRejectedOrderStatusCommand
    - ShipOrderCommand

Domain Events:
    - BuyerAndPaymentMethodVerifiedDomainEvent
    - OrderCancelledDomainEvent
    - OrderStatusChangedToAwaitingValidationDomainEvent
    - OrderStatusChangedToPaidDomainEvent
    - OrderShippedDomainEvent
    - OrderStartedDomainEvent
    - OrderStatusChangedToStockConfirmedDomainEvent
 
 Integration Events:
    GracePeriodConfirmedIntegrationEvent ->
    OrderPaymentFailedIntegrationEvent ->               
    OrderPaymentSucceededIntegrationEvent -> 
    OrderStockConfirmedIntegrationEvent ->
    UserCheckoutAcceptedIntegrationEvent ->
    OrderStockRejectedIntegrationEvent ->
    Publish:

Signalr: sending notifications about the following events
    - OrderStatusChangedToAwaitingValidationIntegrationEvent
    - OrderStatusChangedToCancelledIntegrationEvent
    - OrderStatusChangedToPaidIntegrationEvent
    - OrderStatusChangedToShippedIntegrationEvent
    - OrderStatusChangedToStockConfirmedIntegrationEvent
    - OrderStatusChangedToSubmittedIntegrationEvent