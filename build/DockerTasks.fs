module DockerTasks

open Fake.Extensions.Release
open BlackFox.Fake
open ProjectInfo
open Helpers
open Fake.Core

[<LiteralAttribute>]
let dockerfile_api_path = "build/Dockerfile.Api"
[<LiteralAttribute>]
let dockerfile_ui_path = "build/Dockerfile.UI"

[<LiteralAttribute>]
let ImageName_api = "deepstabp-api"
[<LiteralAttribute>]
let ImageName_ui = "deepstabp-ui"

[<LiteralAttribute>]
let ImageName_api_remote = "csbdocker/" + ImageName_api
[<LiteralAttribute>]
let ImageName_ui_remote = "csbdocker/" + ImageName_ui

let uiOnly (config: TargetParameter)=
    let args = config.Context.Arguments
    args
    |> (List.map String.toLower >> List.contains "--uionly")

let dockerBundle = BuildTask.createFn "DockerBundle" [] (fun config ->
    let uiOnly = uiOnly config
    //let release = release
    Trace.traceImportant $"Start building {ImageName_api} image."
    if not uiOnly then run docker $"build -t {ImageName_api} -f {dockerfile_api_path} ." ""
    Trace.traceImportant $"Start building {ImageName_ui} image."
    // docker build -t deepstabp-ui -f build/Dockerfile.UI .
    run docker $"build -t {ImageName_ui} -f {dockerfile_ui_path} ." ""
)

let dockerTest = BuildTask.createFn "DockerTest" [] (fun config ->
    let apiPort_docker = 80
    let apiPort = 8000
    let uiPort = 5000
    let uiOnly = uiOnly config
    [
        if not uiOnly then "API", docker $"run -it -p {apiPort}:{apiPort_docker} {ImageName_api}" ""
        // docker run -it -p 5000:5000 deepstabp-ui
        "UI", docker $"""run -it -e "DEEPSTABP_URL=http://host.docker.internal:{apiPort}" -p {uiPort}:{uiPort} {ImageName_ui}""" ""
    ]
    |> runParallel
)

/// Must login into "csbdocker" docker account
let dockerPublish = BuildTask.createFn "DockerPublish" [] (fun config ->
    let r = ProjectInfo.release
    let dockerTagImage() =
        // tag api
        run docker $"tag {ImageName_api}:latest {ImageName_api_remote}:{r.SemVer.Major}.{r.SemVer.Minor}.{r.SemVer.Patch}" ""
        run docker $"tag {ImageName_api}:latest {ImageName_api_remote}:latest" ""
        // tag ui
        run docker $"tag {ImageName_ui}:latest {ImageName_ui_remote}:{r.SemVer.Major}.{r.SemVer.Minor}.{r.SemVer.Patch}" ""
        run docker $"tag {ImageName_ui}:latest {ImageName_ui_remote}:latest" ""
    let dockerPushImage() =
        // push api
        run docker $"push {ImageName_api_remote}:{r.SemVer.Major}.{r.SemVer.Minor}.{r.SemVer.Patch}" ""
        run docker $"push {ImageName_api_remote}:latest" ""
        // push ui
        run docker $"push {ImageName_ui_remote}:{r.SemVer.Major}.{r.SemVer.Minor}.{r.SemVer.Patch}" ""
        run docker $"push {ImageName_ui_remote}:latest" ""
    Trace.trace $"Tagging image with :latest and :{r.SemVer.Major}.{r.SemVer.Minor}.{r.SemVer.Patch}"
    dockerTagImage()
    Trace.trace $"Pushing image to dockerhub with :latest and :{r.SemVer.Major}.{r.SemVer.Minor}.{r.SemVer.Patch}"
    dockerPushImage()
    ()
)

[<Literal>]
let private dockerCompose_production = "build/deepStabP.yml"

let dockerTestProduction = BuildTask.createFn "DockerTestProduction" [] (fun config ->
    run dockerCompose $"""-f {dockerCompose_production} up --pull "always" """ ""
)