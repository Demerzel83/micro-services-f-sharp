namespace Microsoft.eShopOnContainers.Utils.Rabbitmq

open System
open System.Text
open System.Threading
open RabbitMQ.Client
open RabbitMQ.Client.Events

module Messaging =
    let producer hostname exchange (routingKeys:string list) (token: CancellationTokenSource) = task {
      let factory = ConnectionFactory(HostName = hostname)
      let! connection = factory.CreateConnectionAsync()
      let! channel = connection.CreateChannelAsync()
      do! channel.ExchangeDeclareAsync(exchange = exchange, ``type`` = ExchangeType.Topic, durable = false, autoDelete = false, arguments = null)

      let rand = Random()
  
      while not token.IsCancellationRequested do
        let data = rand.NextDouble()
        let message = sprintf "%f" data
        let body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(message))
        printfn "publish: %s" message
        let routingKey = if int ((data * 10.)) % 2 = 0 
                         then routingKeys.[0] 
                         else routingKeys.[1]
        do! channel.BasicPublishAsync(exchange = exchange, routingKey = routingKey, body = body)
        Thread.Sleep(500)
    }


    let consumer id hostname exchange routingKey (token: CancellationTokenSource) = task {
      let factory = ConnectionFactory(HostName = hostname)
      let! connection = factory.CreateConnectionAsync()
      let! channel = connection.CreateChannelAsync()
      do! channel.ExchangeDeclareAsync(exchange = exchange, ``type`` = ExchangeType.Topic) //, durable = false, autoDelete = false, arguments = null)

      let! queue = channel.QueueDeclareAsync()
      do! channel.QueueBindAsync(queue = queue.QueueName, exchange = exchange, routingKey = routingKey);

      let consumer = AsyncEventingBasicConsumer(channel)
      consumer.add_ReceivedAsync(new AsyncEventHandler<BasicDeliverEventArgs>(fun sender (data:BasicDeliverEventArgs) -> 
        let body = data.Body
        let message = Encoding.UTF8.GetString(body.ToArray())
        printfn "consumed [%s]: %A" id message
        Tasks.Task.FromResult()
      ))
        

      let! consumeResult = channel.BasicConsumeAsync(queue = "", autoAck = true, consumer = consumer)

      while not token.IsCancellationRequested do
        Thread.Sleep(500)
    }
