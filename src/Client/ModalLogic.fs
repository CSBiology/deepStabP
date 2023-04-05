module ModalLogic

open Fable.React
open Fable.React.Props
open Fulma
open State

[<Literal>]
let private ModalContainerId_inner = "modal_inner_"

let private createId(name:string) = ModalContainerId_inner + name

let removeModal(name:string) =
    let id = createId name
    let ele = Browser.Dom.document.getElementById(id)
    if not <| isNull ele then ele.remove()

///<summary>Function to add a modal to the html body of the active document. If an object with the same name exists, it is removed first.</summary>
///<param name="name">The name of the modal, this is used for generate an Id for the modal by which it is later identified.</param>
///<param name="reactElement">The modal itself with a open parameter which will be the correct remove function for the modal.</param>
let renderModal(name: string, reactElement: (_ -> unit) -> Fable.React.ReactElement) =
    let body = Browser.Dom.document.body
    let id = createId name
    /// check if existing and if so remove
    let _ =
        let ele = Browser.Dom.document.getElementById(id)
        if not <| isNull ele then ele.remove()
    let child = Browser.Dom.document.createElement "div"
    child.id <- id
    body.appendChild(child) |> ignore
    let rmv = fun _ -> removeModal(name)
    Feliz.ReactDOM.render(reactElement rmv, Browser.Dom.document.getElementById id)
    ()