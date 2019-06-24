open System.IO
open Newtonsoft.Json
open GrislyGrotto.Data
open Microsoft.EntityFrameworkCore

let extractPosts databasePath jsonDirectory suppressMessages =
    let options = DbContextOptionsBuilder().UseSqlite("Data Source=" + databasePath).Options
    use dbContext = new GrislyData(options)

    if not (dbContext.Database.CanConnect ()) then
        printfn "could not connect to db :("
    else
        let posts = 
            dbContext.Posts.Include(fun o -> o.Author)
                .Include(fun o -> o.Comments)
                .ToArrayAsync().GetAwaiter().GetResult();
        if not suppressMessages then 
            printfn "found %i posts" posts.Length

        let jsonSettings = JsonSerializerSettings(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)

        for i in [0..posts.Length - 1] do
            let post = posts.[i]
            if not suppressMessages then 
                printfn "processing %i of %i: '%s'" (i + 1) posts.Length post.Title
            
            let json = JsonConvert.SerializeObject(post, jsonSettings)
            let fileName = sprintf "%s-%s.json" (post.Date.ToString("yyyy-MM-dd-hh-mm-ss-tt")) post.Key
            let path = Path.Combine(jsonDirectory, fileName);
            File.WriteAllText(path, json)

[<EntryPoint>]
let main argv =
    
    match argv with
    | [|databasePath;jsonDirectory|] ->
        extractPosts databasePath jsonDirectory false
    | [|databasePath;jsonDirectory;"-quiet"|] ->
        extractPosts databasePath jsonDirectory true
    | _ -> 
        printfn "usage: [path to source db] [json extract dir]"
        printfn "optional third argument: -quiet to suppress messages"

    0
