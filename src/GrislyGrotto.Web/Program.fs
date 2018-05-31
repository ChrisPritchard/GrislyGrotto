open System
open System.IO
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Hosting
open Giraffe
open Models

let dummyPosts = [
    { title = "test 1"; author = "daedalus"; content = "test"; date = DateTime.Now; comments = []; isStory = false }
    { title = "test 2"; author = "icarus"; content = "test"; date = DateTime.Now; comments = []; isStory = false }
    { title = "test 3"; author = "helios"; content = "test"; date = DateTime.Now; comments = []; isStory = false }
]

let webApp =
    choose [
        route "/" >=> (dummyPosts |> List.map Views.post |> Views.layout [] |> htmlView) ]

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    (match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main __ =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "wwwroot")
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0
