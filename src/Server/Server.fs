module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn
open Giraffe
open Shared
open Api

let serviceApi = {
    getVersion = fun () -> async {return "0.1.0"}
}

let deepStabPApi : IDeepStabPApi = {
    helloWorld = fun () -> DeepStabP.helloWorldHandler()
    getVersion = fun () -> DeepStabP.getVersionHandler()
    predict = fun info -> DeepStabP.postPredictHandler info
}

let webApp_deepStabp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder (Route.builderVersioned "v1")
    |> Remoting.fromValue deepStabPApi
    |> Remoting.buildHttpHandler

let webApp_service =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue serviceApi
    |> Remoting.buildHttpHandler

let browserRouter = choose [
    webApp_service
    webApp_deepStabp
]

let app =
    application {
        url "http://0.0.0.0:5000"
        use_router browserRouter
        memory_cache
        use_static "public"
        use_gzip
    }

[<EntryPoint>]
let main _ =
    run app
    0