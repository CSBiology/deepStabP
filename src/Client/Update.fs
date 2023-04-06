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
        let nextModel = { model with HasJobRunning = true; }
        let cmd =
            Cmd.OfAsync.either
                Api.deepStabPApi.postDataBytes
                prop
                PostDataResponse
                GetDataRequestError
        nextModel, cmd
    | PostDataResponse (chunks) ->
        let nextModel = { model with ChunkCount = chunks; Results = List.empty; ChunkIndex = 0; KeepJobRunning = true }
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
        let closeMsg = fun (res) ->
            match res with
            | SweetAlert.Result.Dismissal _ ->
                Some <| CleanStorage model.SessionId
            | _ -> None
        let modalCmd (m: Model)=
            let modal = Client.Components.SweetAlertModals.resultModal_success(m)
            let isVisible = Feliz.SweetAlert.Swal.isVisible()
            // If the user closes the modal (not isVisible), it sets `KeepJobRunning` to false,
            if not isVisible && not m.KeepJobRunning then
                Cmd.none
            // if the modal is visible, we expect the user to await more info
            elif isVisible then
                Cmd.Swal.update(modal)
            // if `KeepJobRunning` is true but the modal is not visible the job propably just started.
            else
                Cmd.Swal.fire(modal, closeMsg)
        let handler_stopped() =
            let mutable m = disableJobRunning nextModel
            m <- { m with SessionId = System.Guid.NewGuid() }
            m, modalCmd m
        let handler_finished() =
            let m = disableJobRunning nextModel
            m, modalCmd (m)
        let handler_getNext() =
            let cmd =
                Cmd.OfAsync.perform
                    Api.deepStabPApi.getData
                    {|session = nextModel.SessionId|}
                    (fun x -> GetDataRequest {|ChunkIndex = x.chunkIndex; Results = x.results|})
            let m = nextModel
            m, Cmd.batch [modalCmd m; cmd]
        if not model.KeepJobRunning then
            handler_stopped()
        elif nextModel.ChunkIndex >= (nextModel.ChunkCount-1) then
            handler_finished()
        else
            handler_getNext()
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