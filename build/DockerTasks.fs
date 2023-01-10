module DockerTasks

open Fake.Extensions.Release
open BlackFox.Fake
open ProjectInfo
open Helpers
open Fake.Core

[<LiteralAttribute>]
let ImageName_api = "deepstabp-api"
[<LiteralAttribute>]
let ImageName_ui = "deepstabp-ui"

let dockerBundle = BuildTask.createFn "DockerBundle" [] (fun config ->
    //let release = release
    Trace.traceImportant $"Start building {ImageName_api} image."
    run docker $"build -t {ImageName_api} -f build/Dockerfile.Api ." ""
    Trace.traceImportant $"Start building {ImageName_ui} image."
    // docker build -t deepstabp-ui -f build/Dockerfile.UI .
    run docker $"build -t {ImageName_ui} -f build/Dockerfile.UI ." ""
)

let dockerTest = BuildTask.createFn "DockerTest" [] (fun config ->
    let apiPort_docker = 80
    let apiPort = 8000
    let uiPort = 5000
    [
        "API", docker $"run -it -p {apiPort}:{apiPort_docker} {ImageName_api}" ""
        // docker run -it -p 5000:5000 deepstabp-ui
        "UI", docker $"run -it -p {uiPort}:{uiPort} {ImageName_ui}" ""
    ]
    |> runParallel
)