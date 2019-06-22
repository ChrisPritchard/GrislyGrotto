open System.IO
open Newtonsoft.Json
open GrislyGrotto.Data
open Microsoft.EntityFrameworkCore

[<EntryPoint>]
let main argv =
    
    match argv with
    | [|databasePath;jsonDirectory|] ->

        let options = DbContextOptionsBuilder().UseSqlite("Data Source=" + databasePath).Options
        use dbContext = new GrislyData(options)

        if not (dbContext.Database.CanConnect ()) then
            printfn "could not connect to db :("
            ()
        else
            let posts = 
                dbContext.Posts.Include(fun o -> o.Author)
                    .Include(fun o -> o.Comments)
                    .ToArrayAsync().GetAwaiter().GetResult();
            printfn "found %i posts" posts.Length

            let jsonSettings = JsonSerializerSettings(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)

            for i in [0..posts.Length - 1] do
                let post = posts.[i]
                printfn "processing %i of %i: '%s'" (i + 1) posts.Length post.Title
                let json = JsonConvert.SerializeObject(post, jsonSettings)
                let fileName = sprintf "%s-%s.json" (post.Date.ToString("yyyy-MM-dd-hh-mm-ss-tt")) post.Key
                let path = Path.Combine(jsonDirectory, fileName);
                File.WriteAllText(path, json)
            ()

    | _ -> 
        printfn "usage: [path to source db] [json extract dir]"

    0
