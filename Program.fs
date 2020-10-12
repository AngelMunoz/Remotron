module Remotron.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open System.Diagnostics
open Giraffe

open Fable.Remoting.Server
open Fable.Remoting.Giraffe

type Payload = { a: list<string>; b: list<int> }

type IService =
    { getTest: Async<string>
      getSample: Payload -> Async<{| message: string; date: DateTime; |}> }

let service: IService =
    { getTest = async { return "This is from Giraffe with Fable.Remoting!" }
      getSample =
          fun (payload: Payload) ->
              async {
                  return {| message = sprintf "Here We go: %A - %A" payload.a payload.b
                            date = DateTime.Now |}
              } }


let webApp: HttpHandler =
    Remoting.createApi ()
    |> Remoting.fromValue service
    |> Remoting.buildHttpHandler

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse
    >=> setStatusCode 500
    >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder: CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080").AllowAnyMethod().AllowAnyHeader()
    |> ignore

let configureApp (app: IApplicationBuilder) =
    let env =
        app.ApplicationServices.GetService<IWebHostEnvironment>()

    (match env.EnvironmentName with
     | "Development" -> app.UseDeveloperExceptionPage()
     | _ -> app.UseGiraffeErrorHandler(errorHandler)).UseCors(configureCors).UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services: IServiceCollection) =
    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot = Path.Combine(contentRoot, "WebRoot")
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
        webHostBuilder.UseContentRoot(contentRoot).UseWebRoot(webRoot)
                      .Configure(Action<IApplicationBuilder> configureApp).ConfigureServices(configureServices)
                      .ConfigureLogging(configureLogging)
        |> ignore).Build().Run()
    0
