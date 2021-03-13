namespace Microsoft.eShopOnContainers.Services.Basket.Infrastructure

open StackExchange.Redis
open System

module DataAccess =
    let getRedisConfiguration (hostName:string) (port:int) (password:string option) =
        let redisConfiguration = ConfigurationOptions()
        redisConfiguration.EndPoints.Add(hostName, port)
        if password.IsSome then
            redisConfiguration.Password <- password.Value
        redisConfiguration

    let getDefaultConfiguration =
        getRedisConfiguration "localhost" 6379 None

    let runWithRedis (sample:(ConnectionMultiplexer) -> unit) =
        let redisConfiguration = getDefaultConfiguration;

        try
            let redis = ConnectionMultiplexer.Connect(redisConfiguration);
            sample redis
        with
            | :? Exception as ex -> printf "%s" ex.Message

    let setValue (redis:ConnectionMultiplexer) key value =
        let db = redis.GetDatabase()
        let redisKey = new RedisKey(key)
        let redisValue = new RedisValue(value)
        db.StringSet(redisKey, redisValue) |> ignore

    let getValue (redis:ConnectionMultiplexer) key =
        let db = redis.GetDatabase()
        let redisKey = new RedisKey(key)
        db.StringGet(redisKey)
