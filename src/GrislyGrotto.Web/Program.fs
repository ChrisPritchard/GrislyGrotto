open System
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Hosting
open Microsoft.EntityFrameworkCore
open Giraffe
open System.IO
open Microsoft.AspNetCore.Authentication.Cookies

let mustBeUser = requiresAuthentication Handlers.accessDenied

let webApp = 
    choose [
        GET >=>
            choose [
                route "/" >=> Handlers.latest 0
                routef "/page/%i" Handlers.latest
                routef "/post/%s" Handlers.single
                route "/login" >=> Handlers.login
                route "/archives" >=> Handlers.archives
                route "/search" >=> Handlers.search            
                route "/about" >=> Handlers.about
                route "/new" >=> mustBeUser >=> Handlers.editor None
                routef "/edit/%s" (fun key -> mustBeUser >=> (Some key |> Handlers.editor)) 
            ]
        POST >=> 
            choose [
                route "/login" >=> Handlers.tryLogin
                routef "/post/%s" Handlers.createComment
                route "/new" >=> mustBeUser >=> Handlers.createPost
                routef "/edit/%s" (fun key -> mustBeUser >=> Handlers.editPost key)
            ]
    ]

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    (match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseStaticFiles()
        .UseAuthentication()
        .UseGiraffe(webApp)

let connString = "Server=tcp:grislygrotto.database.windows.net,1433;Data Source=grislygrotto.database.windows.net;Initial Catalog=grislygrotto;Persist Security Info=False;User ID=grislygrotto_user;Password=***REMOVED***;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

let cookieAuth (o : CookieAuthenticationOptions) =
    do
        o.SlidingExpiration   <- true
        o.ExpireTimeSpan      <- TimeSpan.FromDays 1.

let configureServices (services : IServiceCollection) =
    services.AddDbContext<Data.GrislyData>(fun o -> o.UseSqlServer connString |> ignore) |> ignore
    services
        .AddGiraffe()
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(cookieAuth) |> ignore

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
