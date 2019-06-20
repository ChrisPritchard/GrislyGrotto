
open GrislyGrotto.Data

[<EntryPoint>]
let main argv =
    
    match argv with
    | [|jsonDirectory;databasePath|] ->
        ()
    | _ -> 
        printfn "usage: [json extract dir] [path to save filled db]"

    0
