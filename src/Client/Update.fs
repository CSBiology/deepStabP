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

    let resultModal_props (m: Model) = Client.Components.SweetAlertModals.resultModal_success(m)
    let resultModal_closeMsg = fun (res) ->
        match res with
        | SweetAlert.Result.Dismissal _ ->
            Some <| CleanStorage model.SessionId
        | _ -> None
    let resultModal_Cmd (m: Model)=
        let modal = Client.Components.SweetAlertModals.resultModal_success(m)
        let isVisible = Feliz.SweetAlert.Swal.isVisible()
        // If the user closes the modal (not isVisible), it sets `KeepJobRunning` to false,
        if not isVisible && not m.KeepJobRunning then
            Cmd.none
        // if the modal is visible, we expect the user to await more info
        elif isVisible then
            Cmd.Swal.update(modal)
        // This might not be necessary anymore, since we fire modal at `PostDataResponse`
        // if `KeepJobRunning` is true but the modal is not visible the job propably just started.
        else
            Cmd.Swal.fire(modal, resultModal_closeMsg)
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
        let nextModel = { model with ChunkCount = chunks; Results = List.empty; ChunkIndex = -1; KeepJobRunning = true }
        let cmd =
            Cmd.OfAsync.either
                Api.deepStabPApi.getData
                {|session = model.SessionId|}
                (fun x -> GetDataRequest {|ChunkIndex = x.chunkIndex; Results = x.results|})
                GetDataRequestError
        let resultModal = Cmd.Swal.fire(resultModal_props nextModel, resultModal_closeMsg)
        nextModel, Cmd.batch [resultModal; cmd]
    | GetDataRequest prop ->
        let nextModel = { model with ChunkIndex = prop.ChunkIndex; Results = model.Results@prop.Results }
        // Dont call again if all chunks are processed, (nextModel.ChunkCount - 1) because index is always -1 to length
        let handler_stopped() =
            let mutable m = disableJobRunning nextModel
            m <- { m with SessionId = System.Guid.NewGuid() }
            m, resultModal_Cmd m
        let handler_finished() =
            let m = disableJobRunning nextModel
            // if m is used for modalCmd, not modal will be fired if only one chunk exists.
            m, resultModal_Cmd (m)
        let handler_getNext() =
            let cmd =
                Cmd.OfAsync.either
                    Api.deepStabPApi.getData
                    {|session = nextModel.SessionId|}
                    (fun x -> GetDataRequest {|ChunkIndex = x.chunkIndex; Results = x.results|})
                    GetDataRequestError
            let m = nextModel
            m, Cmd.batch [resultModal_Cmd m; cmd]
        if not model.KeepJobRunning then
            handler_stopped()
        elif nextModel.ChunkIndex >= (nextModel.ChunkCount-1) then
            handler_finished()
        else
            handler_getNext()
    | GetDataRequestError e ->
        let nextModel = disableJobRunning model
        let cmd = if Feliz.SweetAlert.Swal.isVisible() then resultModal_Cmd nextModel else Cmd.ofMsg (GenericError e)
        nextModel, cmd
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