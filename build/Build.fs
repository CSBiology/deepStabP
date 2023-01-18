open Fake.Core
open Fake.IO
open Farmer
open ProjectInfo

open Helpers

initializeContext()

let _ = ReleaseNotesTasks.updateReleaseNotes
let _ = DockerTasks.dockerBundle
let _ = DockerTasks.dockerTest
let _ = DockerTasks.dockerPublish
let _ = DockerTasks.dockerTestProduction

Target.create "Clean" (fun _ ->
    Shell.cleanDir deployPath
    run dotnet "fable clean --yes" clientPath // Delete *.fs.js files created by Fable
)

Target.create "InstallClient" (fun _ -> run npm "install" ".")

Target.create "InstallApi" (fun _ -> run pip "install --no-cache-dir --upgrade --user -r requirements.txt" "./src/Api")

//Target.create "RunApi" (fun _ ->
//    run uvicorn "src.Api.app.main:app --reload" ""
//)

Target.create "Bundle" (fun _ ->
    [ "server", dotnet $"publish -c Release -o \"{deployPath}\"" serverPath
      "client", dotnet "fable -o output -s --run npm run build" clientPath ]
    |> runParallel
)

Target.create "Run" (fun _ ->
    run dotnet "build" sharedPath
    [ "server", dotnet "watch run" serverPath
      "client", dotnet "fable watch -o output -s --run npm run start" clientPath
      "api", uvicorn "app.main:app --reload" "src/Api"
    ]
    |> runParallel
)

Target.create "RunTests" (fun _ ->
    run dotnet "build" sharedTestsPath
    [ "server", dotnet "watch run" serverTestsPath
      "client", dotnet "fable watch -o output -s --run npm run test:live" clientTestsPath ]
    |> runParallel
)

Target.create "Format" (fun _ ->
    run dotnet "fantomas . -r" "src"
)

open Fake.Core.TargetOperators

let dependencies = [
    "Clean"
        ==> "InstallClient"
        ==> "Bundle"

    "Clean"
        ==> "InstallClient"
        ==> "InstallApi"
        ==> "Run"

    "InstallClient"
        ==> "RunTests"

    "Clean"
        ==> "releaseNotes"

    // Without Bundle before DockerBundle it will not work
    "Clean"
        ==> "InstallClient"
        ==> "Bundle"
        ==> "DockerBundle"
]

[<EntryPoint>]
let main args = runOrDefault args