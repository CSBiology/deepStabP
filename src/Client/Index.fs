module Index

open Elmish
open State
open Update

open Feliz
open Feliz.Bulma

let view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        Client.View.Navbar.navbar model dispatch
    ]