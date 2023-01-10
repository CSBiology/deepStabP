module Update

open Elmish
open State

let init () : Model * Cmd<Msg> =
    let model = Model.init

    let cmd = Cmd.ofMsg GetVersionRequest

    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | GetVersionRequest ->
        let cmd =
            Cmd.OfAsync.perform
                Api.serviceApi.getVersion
                ()
                GetVersionResponse
        model, cmd
    | GetVersionResponse version ->
        let nextModel = {model with AppVersion = version}
        nextModel, Cmd.none
    | SingleSequenceRequest tst -> model, Cmd.none
    | FastaUploadRequest tst -> model, Cmd.none