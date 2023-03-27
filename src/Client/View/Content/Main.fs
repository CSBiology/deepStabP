module Content.Main

open Fulma
open Fable.React

[<Literal>]
let HeaderTextColor = "has-text-white-ter" //"has-text-black" //"has-text-white-ter"

let view =
    Content.content [] [
        Heading.h1 [Heading.IsSpaced; Heading.CustomClass HeaderTextColor] [
            str "DeepSTABp: Predict protein thermal stability"
        ]
                
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
    ]