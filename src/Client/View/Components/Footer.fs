module Client.Components.Footer

open Fable.React
open Fable.React.Props
open Fulma

let footer =
    Footer.footer [
        Props [Style [Position PositionOptions.Fixed; Bottom 0; Width "100%"]]
    ] [
        Content.content [] [
            str "This service is developed and maintained by the "
            a [Props.Href "https://csb.bio.uni-kl.de/"] [str "Computational Systems Biology department "]
            str "of the TU Kaiserslautern, Germany."
        ]
    ]

