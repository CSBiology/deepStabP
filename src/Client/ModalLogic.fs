module ModalLogic

open Fable.React
open Fable.React.Props
open Fulma
open State

[<Literal>]
let ModalContainerId = "modal"

[<Literal>]
let ModalContainerId_inner = "modal_inner"

let renderModal (reactElement: Fable.React.ReactElement) =
    let ele = Browser.Dom.document.getElementById ModalContainerId
    if ele.hasChildNodes() then
        for i in 0 .. ele.children.length do
            let child = ele.children.Item i
            child.remove()
    let child = Browser.Dom.document.createElement "div"
    child.id <- ModalContainerId_inner
    ele.appendChild(child) |> ignore
    Feliz.ReactDOM.render(reactElement, Browser.Dom.document.getElementById ModalContainerId_inner)
    ()

let removeModal() =
    let ele = Browser.Dom.document.getElementById ModalContainerId
    if ele <> null then
        if ele.hasChildNodes() then
            let modalContainer = Browser.Dom.document.getElementById ModalContainerId_inner
            modalContainer.remove()

open Feliz

let modalContainer = Html.div [prop.id ModalContainerId]