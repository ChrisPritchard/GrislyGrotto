open System
open System.IO
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Hosting
open Microsoft.EntityFrameworkCore
open Giraffe
open Models
open Microsoft.AspNetCore.Http

let latestHandler (data: GrislyData) = 
    let posts = data.Posts |> Seq.sortByDescending (fun o -> o.Date) |> Seq.take 5 |> Seq.toList
    posts |> List.map Views.post |> Views.layout [] |> htmlView

let webApp =
    choose [
        route "/" >=> warbler (fun (_, (ctx:HttpContext)) -> latestHandler (ctx.GetService<GrislyData>()) ) ]

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
    (services.AddDbContext<GrislyData> : Action<DbContextOptions> * ServiceLifetime -> ServiceCollection)
        ((fun o -> o.UseSqlServer("")), ServiceLifetime.Scoped) |> ignore
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
