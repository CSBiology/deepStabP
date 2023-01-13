module Client.Components.SweetAlertModals

open Elmish
open Feliz
open Feliz.Bulma
open Feliz.SweetAlert

module private ResultModal_success =

    open Fable.Core.JsInterop

    let resultsToCsv (results: DeepStabP.Types.PredictorResponse []) =
        results
        |> Array.map (fun x ->
            $"{x.Protein},{x.MeltingTemp}{System.Environment.NewLine}"
        )
        |> String.concat ""

    let downloadResults (filename: string) (filedata: string) =
        let element = Browser.Dom.document.createElement("a");
        element.setAttribute("href", "data:text/plain;charset=utf-8," +  Fable.Core.JS.encodeURIComponent(filedata));
        element.setAttribute("download", filename);

        element?style?display <- "None";
        let _ = Browser.Dom.document.body.appendChild(element);

        element.click();

        Browser.Dom.document.body.removeChild(element) |> ignore
        ()

    let body (model:State.Model) =
        Html.div [
            prop.style [
                style.maxHeight (length.vh 40);
                style.overflow.auto
            ]
            prop.children [
                Bulma.table [
                    Bulma.table.isFullWidth
                    Bulma.table.isBordered
                    prop.children [
                        Html.thead [Html.tr [
                            Html.th "index"
                            Html.th "Protein"
                            Html.th "mt"
                        ]]
                        Html.tbody [
                            for i in 0 .. model.Result.Length-1 do
                                yield
                                    Html.tr [
                                        let r = model.Result.[i]
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


/// if more customization is needed one can fallback to ModalLogic
let resultModal_success (model:State.Model)=
    Cmd.Swal.fire([
        swal.icon.success
        swal.title "Success"
        swal.html (body model)
        swal.showCloseButton true
        swal.showCancelButton true
        swal.cancelButtonText "Close"
        swal.cancelButtonColor "#E31B4C"
        swal.showConfirmButton true
        swal.confirmButtonText "Download as .csv"
        swal.reverseButtons true
        swal.width (length.perc 70)
        swal.preConfirm (fun _ ->
            let fileName =
                [
                    System.DateTime.UtcNow.ToString("yyyyMMdd_hhmmss")
                    "DeepStabP"
                ] |> String.concat "_"
            model.Result
            |> resultsToCsv
            |> downloadResults fileName
        )
    ])