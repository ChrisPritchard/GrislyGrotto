module Content

open Giraffe

let private css = @"
body {
    font-family: Arial
}
"

let siteCss () = Successful.OK css