module Content.Contact

open Feliz
open Feliz.Bulma

let view =
    Bulma.content [
        Bulma.color.hasTextWhite
        prop.children [
            Html.h1 [
                prop.className Main.HeaderTextColor
                prop.text "Contact"
            ]

            Html.p """This service is developed and maintained by the Computational Systems Biology, RPTU University of Kaiserslautern, 67663 Kaiserslautern, Germany."""

            Html.p [
                Html.text """Contact us via """
                Html.a [
                    Bulma.color.hasTextLinkLight
                    prop.href Shared.Emails.MainContact
                    prop.text "email"
                ]
                Html.text """ or visit the open source """
                Html.a [
                    Bulma.color.hasTextLinkLight
                    prop.href Shared.Urls.GitHubRepo
                    prop.text "GitHub repository"
                ]
                Html.text """ for this service!"""
            ]
        ]
    ]