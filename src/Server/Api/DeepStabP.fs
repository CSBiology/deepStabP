module Api.DeepStabP

open System.Net.Http
open Newtonsoft.Json
open Shared

// Example for testing
//let testFasta = """>sp|A0A178WF56|CSTM3_ARATH Protein CYSTEINE-RICH TRANSMEMBRANE MODULE 3 OS=Arabidopsis thaliana OX=3702 GN=CYSTM3 PE=1 SV=1
//MAQYHQQHEMKQTMAETQYVTAPPPMGYPVMMKDSPQTVQPPHEGQSKGSGGFLRGCLAA
//MCCCCVLDCVF"""
//let example = {
//    growth_temp = 22
//    mt_mode = MT_Mode.Lysate
//    fasta = testFasta
//}

let DeepStabP_url = "http://localhost:8000"
let DeepStabP_url_v1 = DeepStabP_url + "/api/v1"

// Useful links:
// https://www.newtonsoft.com/json
// https://stackoverflow.com/questions/42000362/creating-a-proxy-to-another-web-api-with-asp-net-core

let httpClient = new HttpClient()
httpClient.BaseAddress <- System.Uri(DeepStabP_url_v1)

// Used to serialize Enum Unioncases as strings.
let settings = JsonSerializerSettings()
settings.Converters.Add(Converters.StringEnumConverter())

let helloWorldHandler () =
    task {
        let! world = httpClient.GetAsync("/")
        let! content = world.Content.ReadAsStringAsync()
        let r = JsonConvert.DeserializeObject<HelloWorld>(content)
        return r
    }
    |> Async.AwaitTask

open DeepStabP.Types

let postPredictHandler (info:PredictorInfo) =
    task {
        let url = DeepStabP_url_v1 + "/predict"
        let requestJson = JsonConvert.SerializeObject(info, settings)
        let content = new StringContent(requestJson,System.Text.Encoding.UTF8, "application/json")
        let! world = httpClient.PostAsync(url, content)
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

let getVersionHandler () =
    task {
        let! world = httpClient.GetAsync("/predictor_version")
        let! content = world.Content.ReadAsStringAsync()
        let r = JsonConvert.DeserializeObject<{|Version: string|}>(content)
        return r.Version
    }
    |> Async.AwaitTask