module Update

open Elmish
open State

let init () : Model * Cmd<Msg> =
    let model = Model.init

    let cmd =
        Cmd.batch [
            Cmd.ofMsg GetVersionUIRequest
            Cmd.ofMsg GetVersionApiRequest
        ]

    model, cmd

open Feliz.SweetAlert

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | GetVersionUIRequest ->
        let cmd =
            Cmd.OfAsync.perform
                Api.serviceApi.getVersion
                ()
                GetVersionUIResponse
        model, cmd
    | GetVersionUIResponse ui_version ->
        let nextModel = {model with Version = {model.Version with UI = ui_version}}
        nextModel, Cmd.none
    | GetVersionApiRequest ->
        let cmd =
            Cmd.OfAsync.perform
                Api.deepStabPApi.getVersion
                ()
                GetVersionApiResponse
        model, cmd
    | GetVersionApiResponse api_version ->
        let nextModel = {model with Version = {model.Version with Api = api_version}}
        nextModel, Cmd.none
    | PredictionRequest info ->
        let nextModel = {
            model with HasJobRunning = true
        }
        let cmd =
            Cmd.OfAsync.either
                Api.deepStabPApi.predict
                info
                (Ok >> PredictionResponse)
                (Error >> PredictionResponse)
        nextModel, cmd
    | PredictionResponse (Ok response) ->
        let nextModel = { model with HasJobRunning = false; Result = response }
        let modal = Client.Components.SweetAlertModals.resultModal_success(nextModel)
        nextModel, modal
    | PredictionResponse (Error e) ->
        let nextModel = { model with HasJobRunning = false }
        nextModel, Cmd.Swal.Simple.error(e.GetPropagatedError())