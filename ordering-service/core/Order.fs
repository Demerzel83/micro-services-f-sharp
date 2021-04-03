namespace Microsoft.eShopOnContainers.Services.Ordering.Core

open System

module OrderTypes =
    type OrderItemDTO = {
        ProductId : int
        ProductName : string
        UnitPrice : decimal
        Discount : decimal
        Units : int
        PictureUrl : string
    }

    type CreateOrderCommand = {
        UserId : string
        UserName : string
        City : string
        Street : string
        State : string
        Country : string
        ZipCode : string
        CardNumber : string
        CardHolderName : string
        CardExpiration : DateTime
        CardSecurityNumber : string
        CardTypeId : int
        OrderItems : OrderItemDTO list
    }

    type CancelOrderCommand = {
        OrderNumber : int
    }

    type BasketItem = {
        Id : string
        ProductId : int
        ProductName : string
        UnitPrice : decimal
        OldUnitPrice : decimal
        Quantity : int
        PictureUrl : string
    }

    type CustomerBasket = {
        BuyerId : string
        Items : BasketItem list
    }

    type CreateOrderDraftCommand = {
        BuyerId : string
        Items : BasketItem list
    }

    type SetAwaitingValidationOrderStatusCommand = {
        OrderNumber : int
    }

    type SetPaidOrderStatusCommand = {
        OrderNumber : int
    }

    type SetStockConfirmedOrderStatusCommand = {
        OrderNumber : int
    }

    type SetStockRejectedOrderStatusCommand = {
        OrderNumber : int
        OrderStockItems : int list
    }

    type ShipOrderCommand = {
        OrderNumber : int
    }

    type Address  = {
        Street : string
        City : string
        State : string
        Country : string
        ZipCode : string
    }

    type OrderStatus =
        | Submitted
        | AwaitingValidation
        | StockConfirmed
        | Paid
        | Shipped
        | Cancelled

    type OrderItem = {
        ProductName : string
        PictureUrl : string
        UnitPrice : decimal
        Discount : decimal
        Units : int
        ProductId : int
    }

    type Order = {
        OrderDate : DateTime
        Address : Address
        BuyerId : int option
        OrderStatus : OrderStatus
        OrderStatusId : int    
        Description : string
        IsDraft : bool
        OrderItems : OrderItem list
        PaymentMethodId : int option
    }