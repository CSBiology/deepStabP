module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn
open Giraffe
open Shared
open Microsoft.AspNetCore.Http
open System.Collections.Generic
open Microsoft.AspNetCore.Hosting

let private errorHandler (ex:exn) (routeInfo:RouteInfo<HttpContext>) =
    let msg = sprintf "%s." ex.Message 
    Propagate msg

let serviceApi = {
    getVersion = fun () -> async {
        return "1.0.0"
    }
}

let deepStabPApi : IDeepStabPApi = {
    helloWorld = fun () -> DeepStabP.Api.helloWorldHandler()
    getVersion = fun () -> DeepStabP.Api.getVersionHandler()
    postDataBytes = fun prop -> DeepStabP.Api.postDataBytesHandler prop
    postDataString = fun prop -> DeepStabP.Api.postDataStringHandler prop
    getData = fun prop -> DeepStabP.Api.getDataHandler prop.session
    cleanStorage = fun prop -> DeepStabP.Api.cleanStorageHandler prop.session
} 

let webApp_deepStabp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder (Route.builderVersioned "v1")
    |> Remoting.fromValue deepStabPApi
    |> Remoting.withErrorHandler errorHandler
    |> Remoting.withDiagnosticsLogger (printfn "%A")
    |> Remoting.buildHttpHandler

let webApp_service =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue serviceApi
    |> Remoting.withErrorHandler errorHandler
    |> Remoting.withDiagnosticsLogger (printfn "%A")
    |> Remoting.buildHttpHandler

let browserRouter = choose [
    webApp_service
    webApp_deepStabp
]

printfn "[STORAGE] data count: %i" Storage.data.Count
printfn "[STORAGE] metadata count: %i" Storage.metaData.Count


let webhost (config: IWebHostBuilder) : IWebHostBuilder =
    config.UseKestrel(fun options ->
        options.Limits.MaxRequestBodySize <- System.Nullable()
        ()
    )

let app =
    application {
        url "http://0.0.0.0:5000"
        use_router browserRouter
        memory_cache
        use_static "public"
        use_gzip
        webhost_config webhost
    }

[<EntryPoint>]
let main _ =
    run app
    0