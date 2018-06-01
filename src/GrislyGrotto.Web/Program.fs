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
    //let posts = data.Posts |> Seq.sortByDescending (fun o -> o.Date) |> Seq.take 5 |> Seq.toList
    let posts = 
        query {
            for post in data.Posts.Include(fun p -> p.Comments).Include(fun p -> p.Author) do
                sortByDescending post.Date
                take 5
                select post
        } |> Seq.toList
    posts |> List.map Views.listPost |> Views.layout [] |> htmlView

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

let connString = "Server=tcp:grislygrotto.database.windows.net,1433;Data Source=grislygrotto.database.windows.net;Initial Catalog=grislygrotto;Persist Security Info=False;User ID=grislygrotto_user;Password=***REMOVED***;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

let configureServices (services : IServiceCollection) =
    services.AddDbContext<GrislyData>(fun o -> o.UseSqlServer connString |> ignore) |> ignore
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
