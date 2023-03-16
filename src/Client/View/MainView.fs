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
                    str "DeepSTABp: Predict protein thermal stability"
                ]
                br []
                
                Heading.h4 [Heading.IsSubtitle;Heading.IsSpaced; Heading.CustomClass HeaderTextColor] [
                    str "Protein thermal stability is an important parameter for understanding protein function, designing protein-based therapeutics, and improving the stability of enzymes used in industrial processes. This web interface allows you to utilize the deep learning model DeepSTABp to predict the melting point of a protein of your choice!"
                ]
                Heading.h4 [Heading.IsSubtitle;Heading.IsSpaced; Heading.CustomClass HeaderTextColor] [
                    str "Only three more steps are needed to get going:" 
                ]
                Heading.h4 [Heading.IsSubtitle;Heading.IsSpaced; Heading.CustomClass HeaderTextColor] [
                    str "(i) Provision a protein sequence or protein database in the FASTA format" 
                ]
                Heading.h4 [Heading.IsSubtitle;Heading.IsSpaced; Heading.CustomClass HeaderTextColor] [
                    str "(ii) Specify a growth temperature " 
                ]
                Heading.h4 [Heading.IsSubtitle;Heading.IsSpaced; Heading.CustomClass HeaderTextColor] [
                    str "(iii) Set the ‘lysate’ or ‘cell’ flag. " 
                ]
                // Heading.h4 [Heading.IsSubtitle;Heading.IsSpaced; Heading.CustomClass HeaderTextColor] [
                //     str ""
                //     a [
                //         Class "has-text-info"
                //         //OnClick (fun _ -> ChangeHelpDisplay TechnicalScientificDetails |> dispatch)
                //         Style [
                //             TextDecoration "none"
                //             Color "white"
                //     ]] [
                //         str "'Details'"
                //     ]
                //     str " section." 
                //     ]
            ]
        ]
    ]