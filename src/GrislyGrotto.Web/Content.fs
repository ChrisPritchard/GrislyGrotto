module Content

open Giraffe
open System
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

let faviconContent = "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"

let favicon = 
    fun (next : HttpFunc) (ctx : HttpContext) -> 
        task {
            ctx.SetHttpHeader "Content-Type" "image/png"
            faviconContent |> Convert.FromBase64String |> ctx.WriteBytesAsync |> ignore
            return! next ctx
        }