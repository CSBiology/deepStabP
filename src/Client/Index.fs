module Index

open Elmish
open State
open Update

open Feliz
open Feliz.Bulma

let view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        Client.Components.Navbar.Navbar model dispatch
        Client.View.MainView.hero model dispatch
        Client.Components.Footer.footer
    ]