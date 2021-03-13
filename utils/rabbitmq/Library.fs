namespace Microsoft.eShopOnContainers.Utils.Rabbitmq

open System
open System.Text
open System.Threading
open RabbitMQ.Client
open RabbitMQ.Client.Events

module Messaging =
    let producer hostname exchange (routingKeys:string list) (token: CancellationTokenSource) = async {
      let factory = ConnectionFactory(HostName = hostname)
      use connection = factory.CreateConnection()
      use channel = connection.CreateModel()
      channel.ExchangeDeclare(exchange = exchange, ``type`` = ExchangeType.Topic, durable = false, autoDelete = false, arguments = null)

      let rand = Random()
  
      while not token.IsCancellationRequested do
        let data = rand.NextDouble()
        let message = sprintf "%f" data
        let body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(message))
        printfn "publish: %s" message
        let routingKey = if int ((data * 10.)) % 2 = 0 
                         then routingKeys.[0] 
                         else routingKeys.[1]
        channel.BasicPublish(exchange = exchange, routingKey = routingKey, basicProperties = null, body = body)
        Thread.Sleep(500)
    }


    let consumer id hostname exchange routingKey (token: CancellationTokenSource) = async {
      let factory = ConnectionFactory(HostName = hostname)
      use connection = factory.CreateConnection()
      use channel = connection.CreateModel()
      channel.ExchangeDeclare(exchange = exchange, ``type`` = ExchangeType.Topic) //, durable = false, autoDelete = false, arguments = null)

      let queueName = channel.QueueDeclare().QueueName
      channel.QueueBind(queue = queueName, exchange = exchange, routingKey = routingKey);

      let consumer = EventingBasicConsumer(channel)
      consumer.Received.AddHandler(new EventHandler<BasicDeliverEventArgs>(fun sender (data:BasicDeliverEventArgs) -> 
        let body = data.Body
        let message = Encoding.UTF8.GetString(body.ToArray())
        printfn "consumed [%s]: %A" id message))

      let consumeResult = channel.BasicConsume(queue = "", autoAck = true, consumer = consumer)

      while not token.IsCancellationRequested do
        Thread.Sleep(500)
    }
