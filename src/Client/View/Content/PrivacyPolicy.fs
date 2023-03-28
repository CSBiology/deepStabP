module Content.PrivacyPolicy

open Feliz
open Feliz.Bulma

let private header2 (str: string) =
    Html.h2 [
        prop.className Main.HeaderTextColor
        prop.text str
    ]

let view =
    Bulma.content [
        Bulma.color.hasTextWhite
        prop.children [
            Html.h1 [
                prop.className Main.HeaderTextColor
                prop.text "Privacy Policy"
            ]

            header2 "1. Introduction"
            
            Html.p """This privacy policy applies to the use of the website DeepSTABp (hereinafter referred to as "Website") and describes how personal data provided by users (hereinafter referred to as "you") on the Website is processed."""

            header2 "2. Data Collection"
            Html.p """The Website does not collect any personal data from you. No cookies or similar technologies are used to collect information about your visit to the Website."""

            header2 "3. Data Processing"
            Html.p """As the Website does not collect any personal data, no personal data is processed."""

            header2 "4. Disclosure of Data"
            Html.p """As the Website does not collect any personal data, no personal data is disclosed to any third parties."""

            header2 "5. Security"
            Html.p """The security of your data is our top priority. We take appropriate technical and organizational measures to ensure the security of your data and to protect it from loss, misuse, or unauthorized access."""

            header2 "6. Changes to this Privacy Policy"
            Html.p """We reserve the right to change this privacy policy from time to time. Changes will be published on the Website. Please check the privacy policy regularly to stay up-to-date."""

            header2 "7. Contact"
            Html.p [
                prop.children [
                    Html.text """If you have any questions or concerns regarding this privacy policy, please contact us via """
                    Html.a [
                        Bulma.color.hasTextLinkLight
                        prop.href Shared.Emails.MainContact
                        prop.text "email"
                    ]
                    Html.text "."
                ]
            ]

            Html.p """Last updated: 28.03.2023"""
        ]
    ]

