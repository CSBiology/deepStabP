module Index

open Elmish
open State
open Update

open Feliz

let view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.style [
            style.minHeight(length.vh 100)
            style.display.flex
            style.flexDirection.column
        ]
        prop.children [
            Client.Components.Navbar.Navbar model dispatch
            Client.View.MainView.hero model dispatch
            Client.View.InputView.View model.Version model.HasJobRunning model dispatch
            Client.Components.Footer.footer model.Version
        ]
    ]