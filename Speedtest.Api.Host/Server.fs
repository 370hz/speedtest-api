module Speedtest.Api.Host.Server

open System
open System.IO
open System.Collections.Generic
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration;
open Microsoft.Extensions.Configuration.Json
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe.HttpHandlers
open Giraffe.Middleware

open Speedtest.Api.Host.Routes

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

let configureAppConfiguration (config : IConfigurationBuilder) =
    config.AddJsonFile("appsettings.json").Build() |> ignore

    (*Configuration = config.Build()
    Environment = hostingContext.HostingEnvironment |> ignore*)

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffeErrorHandler errorHandler
    app.UseGiraffe routeHandler

let configureServices (services : IServiceCollection) =
    services.BuildServiceProvider() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main argv =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot = Path.Combine(contentRoot, "WebRoot")

    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .ConfigureAppConfiguration(Action<IConfigurationBuilder> configureAppConfiguration)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()

    0