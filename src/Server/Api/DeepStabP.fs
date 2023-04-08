module DeepStabP.Api

open System.Net.Http
open Newtonsoft.Json
open DeepStabP.Types
open FastaReader
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

let DeepStabP_url_v1 = Environment.deepStabP_url + "/api/v1"

// Useful links:
// https://www.newtonsoft.com/json
// https://stackoverflow.com/questions/42000362/creating-a-proxy-to-another-web-api-with-asp-net-core

let httpClient = new HttpClient()
httpClient.BaseAddress <- System.Uri(DeepStabP_url_v1)
httpClient.Timeout <- System.TimeSpan(0,5,0)

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

let postDataBytesHandler (prop:Shared.PostDataBytes) = async {
    let countChunks =
        FastaRecord.ofFile prop.data // parse data to fasta
        |> addToStorage prop.metadata // add to storage, return n of chunks
    return countChunks
}

let postDataStringHandler (prop:Shared.PostDataString) = async {
    let countChunks =
        FastaRecord.ofString prop.data // parse data to fasta
        |> addToStorage prop.metadata // add to storage, return n of chunks
    return countChunks
}

let private parseToResultType (jsonStr: string) = 
    let response = JsonConvert.DeserializeObject<{| Prediction: {| Protein: Map<int,string>; Tm: Map<int,float> |} |}>(jsonStr)
    let protein = response.Prediction.Protein
    let tm = response.Prediction.Tm
    let min,_ = tm |> Map.minKeyValue
    let max, _ = tm |> Map.maxKeyValue
    [
        for i in min .. max do
            yield PredictorResponse.create(
                protein.[i],
                tm.[i]
            )
    ]

let private processChunk (info: PredictorInfo) =
    task {
        let url = DeepStabP_url_v1 + "/predict"
        let requestJson = JsonConvert.SerializeObject(info, settings)
        let content = new StringContent(requestJson,System.Text.Encoding.UTF8, "application/json")
        let! request = httpClient.PostAsync(url, content)
        let! content = request.Content.ReadAsStringAsync()
        let responseParsed = parseToResultType content
        return responseParsed
    } |> Async.AwaitTask

let getDataHandler (guid:System.Guid) = 
    async {
        let n() = System.DateTime.Now.ToShortTimeString()
        try
            let md, d = getStorage guid
            let chunkIndex = md.ChunkIndex
            printfn $"[GETDATA: {n()}] {chunkIndex}/{(md.ChunkCount - 1)} ({guid})"
            let chunk = d |> Seq.item chunkIndex
            printfn $"[GETDATA: {n()}] get chunk ({guid})"
            let predictorInfo = PredictorInfo.create(md.Growth_Temp, md.MT_Mode, chunk)
            printfn $"[GETDATA: {n()}] sent to api ({guid})"
            let! chunk_processed = processChunk predictorInfo
            printfn $"[GETDATA: {n()}] response from api ({guid})"
            md.increaseChunkIndex() // increases chunk index by 1
            // remove if all data processed, (md.ChunkCount - 1) because index is always -1 to length
            if chunkIndex >= (md.ChunkCount - 1) then removeFromStorage guid |> ignore
            printfn $"[GETDATA: {n()}] prosprocessing done ({guid})"
            return {chunkIndex = chunkIndex; results = chunk_processed}
        with
            | ex ->
                printfn $"[GETDATA: {n()}] Failed with {ex.Message}" 
                removeFromStorage guid |> ignore
                return raise ex
    } 

let getVersionHandler () =
    task {
        let! world = httpClient.GetAsync("/predictor_version")
        let! content = world.Content.ReadAsStringAsync()
        let r = JsonConvert.DeserializeObject<{|Version: string|}>(content)
        return r.Version
    }
    |> Async.AwaitTask

let cleanStorageHandler (guid: System.Guid) =
    async {
        let _ = Storage.removeFromStorage guid
        return ()
    }