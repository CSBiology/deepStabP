module Client.Components.Footer

open Fable.React
open Fable.React.Props
open Fulma
open Feliz

let footer (versions: State.Versions) =
    Footer.footer [
        Props [Style [Width "100%";]]
    ] [
        Content.content [
            Content.Modifiers [Modifier.TextSize (Screen.All, TextSize.Is7)]
        ] [
            div [] [
                str "This service is developed and maintained by the "
                a [Href "https://csb.bio.uni-kl.de/"; Class "has-text-info"] [str "Computational Systems Biology department"]
                str " of the "
                a [Href "https://rptu.de"; Class "has-text-info"] [str "RPTU"]
                str ", Germany."
            ]
            div [] [
                str ("Web UI: " + versions.UI + " ")
                Html.i [prop.className "fa-solid fa-code"]
                str (" deepStabP: " + versions.Api)
            ]
        ]
    ]