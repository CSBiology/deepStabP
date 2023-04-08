module Client.Components.SweetAlertModals

open Elmish
open Feliz
open Feliz.Bulma
open Feliz.SweetAlert

module private ResultModal_success =

    let body (model:State.Model) =
        Html.div [
            prop.style [
                style.maxHeight (length.vh 40);
                style.overflow.auto
            ]
            prop.children [
                if model.Results.IsEmpty then
                    Bulma.content [
                        Html.p "Sequences parsed successfully! Waiting for the results, while the machine is thinking"
                        Html.p [
                            Bulma.icon [Html.i [prop.className "fa-solid fa-robot fa-lg"]]
                        ]
                    ]
                else
                    Bulma.table [
                        Bulma.table.isFullWidth
                        Bulma.table.isBordered
                        prop.children [
                            Html.thead [Html.tr [
                                Html.th "index"
                                Html.th "Protein"
                                Html.th "Melting temperature [°C]"
                            ]]
                            Html.tbody [
                                let arr = model.Results |> Array.ofList
                                for i in 0 .. model.Results.Length-1 do
                                    yield
                                        Html.tr [
                                            let r = arr.[i]
                                            Html.td i
                                            Html.td r.Protein
                                            Html.td r.MeltingTemp
                                        ]
                            ]
                        ]
                    ]
            ]
        ]

open ResultModal_success

let private isError (model: State.Model) =
    let notFinished = (model.ChunkIndex+1) <> model.ChunkCount
    let notRunning = not model.KeepJobRunning
    notFinished && notRunning

let private title (model:State.Model) =
    let isError = isError model
    let current = model.ChunkIndex+1 
    let max = model.ChunkCount
    let title_main = if isError then $"Error at ({current+1}/{max})" else $"Success ({current}/{max})"
    let title_addition = if current = 0 then " .. calculating first " else " .. calculating next "
    Html.span [
        Html.span title_main
        if model.KeepJobRunning then
            Html.span title_addition
            Bulma.icon [Html.i [prop.className "fa-solid fa-person-running fa-bounce"]]
    ]

/// if more customization is needed one can fallback to ModalLogic
let resultModal_success_fire (model:State.Model) =
    let isError = isError model
    [
        if not isError then swal.icon.success else swal.icon.error
        swal.title (title model)
        swal.html (body model)
        swal.showCloseButton true
        swal.showCancelButton true
        swal.cancelButtonText "Close"
        swal.cancelButtonColor "#E31B4C"
        swal.showConfirmButton true
        swal.confirmButtonText "Download as .csv"
        swal.reverseButtons true
        //swal.width (length.perc 70)
        //swal.preConfirm (fun _ ->
        //    true
        //)
    ]

let resultModal_success_update (model: State.Model) =
    let isError = isError model
    [
        if not isError then swal.icon.success else swal.icon.error
        swal.title (title model)
        swal.html (body model)
        swal.showCloseButton true
        swal.showCancelButton true
        swal.cancelButtonText "Close"
        swal.cancelButtonColor "#E31B4C"
        swal.showConfirmButton true
        swal.confirmButtonText "Download as .csv"
        swal.reverseButtons true
    ]
    