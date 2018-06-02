module Content

open Giraffe
open Microsoft.AspNetCore.Http

let private cssContent = @"
body {
    font-family: Arial
}"

let css = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            ctx.SetHttpHeader "Content-Type" "text/css"
            ctx.WriteStringAsync cssContent |> ignore
            return! next ctx
        }