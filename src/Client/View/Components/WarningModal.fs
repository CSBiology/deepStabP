module Client.Components.WarningModal

open Feliz
open Feliz.Bulma
open Shared

let fileSizeIsLarge_msgBody (size: int) = [
    Bulma.field.div [prop.innerHtml $"You are uploading a large file ({size/1000000} MB)."]
    Bulma.field.div [prop.innerHtml $"This can take some time depending on your internet connection. Please be patient after starting the calculation."]
]

let Main (msgBody: seq<ReactElement>) (rmv: _ -> unit) =
    Bulma.modal [
        Bulma.modal.isActive
        prop.children [
            Bulma.modalBackground [ prop.onClick rmv ]
            Bulma.modalCard [
                Bulma.modalCardHead [
                    Bulma.modalCardTitle "Attention!"
                    Bulma.delete [ prop.onClick rmv ]
                ]
                Bulma.modalCardBody msgBody
                Bulma.modalCardFoot [
                    Bulma.button.a [
                        prop.onClick rmv
                        Bulma.color.isInfo
                        prop.text "Ok"
                    ]
                ]
            ]
        ]
    ]