module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared

let serviceApi = {
    getVersion = fun () -> async {return "0.1.0"}
}

let webApp_service =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue serviceApi
    |> Remoting.buildHttpHandler



let app =
    application {
        url "http://0.0.0.0:5000"
        use_router webApp_service
        memory_cache
        use_static "public"
        use_gzip
    }

[<EntryPoint>]
let main _ =
    run app
    0