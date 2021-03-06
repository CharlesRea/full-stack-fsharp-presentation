module Server

open BlackjackApi
open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Thoth.Json.Giraffe
open Fable.Remoting.Giraffe
open Fable.Remoting.Server
open Shared

let remotingWebApp =
    Remoting.createApi()
    |> Remoting.fromValue blackJackApi
    |> Remoting.withRouteBuilder apiRouteBuilder
    |> Remoting.buildHttpHandler

let configureApp (app : IApplicationBuilder) =
    app.UseDeveloperExceptionPage()
        .UseStaticFiles()
        .UseGiraffe remotingWebApp

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