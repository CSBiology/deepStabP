module Client.View.MainView

open Fable.React
open Fable.React.Props
open Fulma
open State

[<Literal>]
let HeaderTextColor = "has-text-white-ter" //"has-text-black" //"has-text-white-ter"

let hero (model : Model) (dispatch : Msg -> unit) =
    Hero.hero [Hero.CustomClass "csbHero"] [
        Hero.body [] [
            Container.container [ ] [
                Heading.h1 [Heading.IsSpaced; Heading.CustomClass HeaderTextColor] [
                    str "deepStabP"
                ]
                br []
                Heading.h4 [Heading.IsSubtitle;Heading.IsSpaced; Heading.CustomClass HeaderTextColor] [
                    str "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                ]
                Heading.h4 [Heading.IsSubtitle;Heading.IsSpaced; Heading.CustomClass HeaderTextColor] [
                    str "At vero eos et accusam et justo duo dolores et ea rebum."
                ]
                Heading.h4 [Heading.IsSubtitle;Heading.IsSpaced; Heading.CustomClass HeaderTextColor] [
                    str "Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet: "
                    a [
                        Class "has-text-info"
                        //OnClick (fun _ -> ChangeHelpDisplay TechnicalScientificDetails |> dispatch)
                        Style [
                            TextDecoration "none"
                            Color "white"
                    ]] [
                        str "'Details'"
                    ]
                    str " section." 
                    ]
            ]
        ]
    ]