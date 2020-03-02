module Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Thoth.Json.Giraffe

type GreetingResponse =
    {
        Greeting : string
    }

let greetingsHandler (name : string) =
    let greetings = sprintf "Hello %s, from Giraffe!" name
    let response = { Greeting = greetings }
    json response

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> greetingsHandler "world"
                routef "/hello/%s" greetingsHandler
                route "/ping"   >=> text "pong"
            ]
        setStatusCode 404 >=> text "Not Found" ]

let configureApp (app : IApplicationBuilder) =
    app.UseDeveloperExceptionPage()
        .UseStaticFiles()
        .UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore
    services.AddSingleton<Giraffe.Serialization.Json.IJsonSerializer>(ThothSerializer()) |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0