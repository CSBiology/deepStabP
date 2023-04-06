module Update

open Elmish
open State

let init () : Model * Cmd<Msg> =
    let model = Model.init()

    let cmd =
        Cmd.batch [
            Cmd.ofMsg GetVersionUIRequest
            Cmd.ofMsg GetVersionApiRequest
        ]

    model, cmd

open Feliz.SweetAlert

let private disableJobRunning (model: Model) = {model with HasJobRunning = false; KeepJobRunning = false}

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | UpdatePage nextPage ->
        let nextModel = {
            model with Page = nextPage
        }
        nextModel, Cmd.none
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
    | PostDataString prop ->
        let nextModel = { model with HasJobRunning = true }
        let cmd =
            Cmd.OfAsync.either
                Api.deepStabPApi.postDataString
                prop
                PostDataResponse
                GetDataRequestError
        nextModel, cmd
    | PostDataBytes prop ->
        let nextModel = { model with HasJobRunning = true }
        let cmd =
            Cmd.OfAsync.either
                Api.deepStabPApi.postDataBytes
                prop
                PostDataResponse
                GetDataRequestError
        nextModel, cmd
    | PostDataResponse (chunks) ->
        let nextModel = { model with ChunkCount = chunks; Results = List.empty; ChunkIndex = 0 }
        let cmd =
            Cmd.OfAsync.either
                Api.deepStabPApi.getData
                {|session = model.SessionId|}
                (fun x -> GetDataRequest {|ChunkIndex = x.chunkIndex; Results = x.results|})
                GetDataRequestError
        nextModel, cmd
    | GetDataRequest prop ->
        let nextModel = { model with ChunkIndex = prop.ChunkIndex; Results = model.Results@prop.Results }
        // Dont call again if all chunks are processed, (nextModel.ChunkCount - 1) because index is always -1 to length
        let modal = Client.Components.SweetAlertModals.resultModal_success(nextModel)
        let closeMsg = fun res ->
            Browser.Dom.console.log("CLOSED!")
            Some <| CleanStorage model.SessionId
        let modalCmd =
            if Feliz.SweetAlert.Swal.isVisible() then
                Cmd.Swal.update(modal)
            else
                Cmd.Swal.fire(modal,closeMsg)
        if not model.KeepJobRunning then
            let nextModel' = disableJobRunning model
            let nextModel'' = { nextModel' with SessionId = System.Guid.NewGuid() }
            nextModel'', Cmd.none
        elif nextModel.ChunkIndex >= (nextModel.ChunkCount-1) then
            let cmd = Cmd.none
            let nextModel' = disableJobRunning model
            nextModel', Cmd.batch [modalCmd; cmd]
        else
            let cmd =
                Cmd.OfAsync.perform
                    Api.deepStabPApi.getData
                    {|session = model.SessionId|}
                    (fun x -> GetDataRequest {|ChunkIndex = x.chunkIndex; Results = x.results|})
            nextModel, Cmd.batch [modalCmd; cmd]
    | GetDataRequestError e ->
        let nextModel = disableJobRunning model
        nextModel, Cmd.ofMsg (GenericError e)
    | GenericError exn ->
        model, Cmd.Swal.Simple.error(exn.GetPropagatedError())
    | DisableJobRunning ->
        let nextModel = disableJobRunning model
        nextModel, Cmd.none
    | CleanStorage guid ->
        let nextModel = disableJobRunning model
        let cmd =
            Cmd.OfAsync.attempt
                Api.deepStabPApi.cleanStorage {|session = guid|}
                (fun exn -> failwithf "Unable to clear session id: %s" exn.Message)
        nextModel, cmd