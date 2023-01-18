module Environment

open System

[<Literal>]
let private url_key = "DEEPSTABP_URL"

let deepStabP_url =
    let def = "http://localhost:8000"
    let nullable = System.Environment.GetEnvironmentVariable(url_key)
    if isNull nullable then def else nullable