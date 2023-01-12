module Index

open Elmish
open State
open Update

open Feliz

let view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.children [
            Client.Components.Navbar.Navbar model dispatch
            Client.View.MainView.hero model dispatch
            Client.View.InputView.View model.HasJobRunning dispatch
            Client.Components.Footer.footer model.Version
        ]
    ]