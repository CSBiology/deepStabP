module Update

open Elmish
open State

let init () : Model * Cmd<Msg> =
    let model = Model.init

    let cmd = Cmd.none

    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | SingleSequenceRequest tst -> model, Cmd.none
    | FastaUploadRequest tst -> model, Cmd.none