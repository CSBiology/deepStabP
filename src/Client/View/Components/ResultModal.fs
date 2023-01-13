module Client.Components.ResultModal

open Feliz
open Feliz.Bulma

[<ReactComponent>]
let Results() =
    let (count, setCount) = React.useState(0)
    Bulma.modal [
        Bulma.modal.isActive
        prop.children [
            Bulma.modalBackground []
            Bulma.modalContent [
                Bulma.box [
                    Bulma.content [
                        Html.p "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat,
                        sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus
                        est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore
                        et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea
                        takimata sanctus est Lorem ipsum dolor sit amet."
                    ]
                ]
            ]
            Bulma.modalClose [
                Bulma.modalClose.isLarge
                prop.onClick(fun _ -> ModalLogic.removeModal())
            ]
        ]
    ]

open Browser.Dom