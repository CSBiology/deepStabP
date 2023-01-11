module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn
open Giraffe
open Shared

let serviceApi = {
    getVersion = fun () -> async {return "0.1.0"}
}

let testFasta = """>sp|A0A178WF56|CSTM3_ARATH Protein CYSTEINE-RICH TRANSMEMBRANE MODULE 3 OS=Arabidopsis thaliana OX=3702 GN=CYSTM3 PE=1 SV=1
MAQYHQQHEMKQTMAETQYVTAPPPMGYPVMMKDSPQTVQPPHEGQSKGSGGFLRGCLAA
MCCCCVLDCVF"""

module DeepStabP =

    open System.Net.Http
    open Newtonsoft.Json

    let mutable DeepStabP_url = "http://localhost:8000"

    let httpClient = new HttpClient()
    httpClient.BaseAddress <- System.Uri(DeepStabP_url)

    // https://www.newtonsoft.com/json
    // https://stackoverflow.com/questions/42000362/creating-a-proxy-to-another-web-api-with-asp-net-core

    let testHandler  =
        task {
            let! world = httpClient.GetAsync("/")
            let! content = world.Content.ReadAsStringAsync()
            let r = JsonConvert.DeserializeObject<HelloWorld>(content)
            return r
        }
        |> Async.AwaitTask

    open DeepStabP.Types

    let example = {
        growth_temp = 22
    }

    let postPredictHandler (info:PredictorInfo) =
        task {
            let requestJson = JsonConvert.SerializeObject(info)
            let content = new StringContent(requestJson,System.Text.Encoding.UTF8, "application/json")
            let! world = httpClient.PostAsync("/predict", content)
            let! content = world.Content.ReadAsStringAsync()
            let response = JsonConvert.DeserializeObject<{| Prediction: seq<seq<obj>> |}>(content)
            let responseParsed =
                response.Prediction
                |> Seq.map (fun x ->
                    PredictorResponse.create
                        (Seq.item 0 x |> string)
                        (Seq.item 1 x |> string |> float)
                )
                |> Array.ofSeq
            return responseParsed
        }
        |> Async.AwaitTask


let deepStabPApi : IDeepStabPApi = {
    helloWorld = fun () -> DeepStabP.testHandler
    predict = fun () -> DeepStabP.postPredictHandler DeepStabP.example
}


let webApp_deepStabp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
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