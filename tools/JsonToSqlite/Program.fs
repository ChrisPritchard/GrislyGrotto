open System.IO
open Newtonsoft.Json
open GrislyGrotto.Data
open Microsoft.EntityFrameworkCore

[<EntryPoint>]
let main argv =
    
    match argv with
    | [|jsonDirectory;databasePath|] ->
        
        let json = Directory.GetFiles (jsonDirectory, "*.json")
        printfn "found %i files" json.Length

        File.Copy ("./blank_grislygrotto.db", databasePath, true)
        printfn "copied db template"

        let options = DbContextOptionsBuilder().UseSqlite("Data Source=" + databasePath).Options
        use dbContext = new GrislyData(options)

        if not (dbContext.Database.CanConnect ()) then
            printfn "could not connect to db :("
            ()
        else
            for i in [0..json.Length - 1] do
                let file = json.[i]
                let post = File.ReadAllText file |> JsonConvert.DeserializeObject<Post>
                printfn "processing %i of %i: '%s'" (i + 1) json.Length post.Title
                dbContext.Posts.Add post |> ignore
                dbContext.SaveChanges () |> ignore
            ()

    | _ -> 
        printfn "usage: [json extract dir] [path to save filled db]"

    0
