module Client.View.MainView

open Fable.React
open Fable.React.Props
open Fulma
open State
open Feliz

let hero (model : Model) (dispatch : Msg -> unit) =
    Hero.hero [Hero.CustomClass "csbHero"] [
        Hero.body [] [
            Container.container [ ] [
                match model.Page with
                | Page.Main -> Content.Main.view
                | Page.About -> Content.About.view
                | Page.PrivacyPolicy -> Content.PrivacyPolicy.view
            ]
        ]
    ]