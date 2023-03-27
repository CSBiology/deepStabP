module Content.About

open Feliz
open Feliz.Bulma


let view =
    Bulma.content [
        Bulma.color.hasTextWhite
        prop.children [
            Html.h1 [
                prop.className Main.HeaderTextColor
                prop.text "About"
            ]

            Html.p [
                Bulma.size.isSize5
                prop.text """
    Proteins are essential macromolecules that carry out a plethora of biological functions.
    The thermal stability of proteins is an important property that affects their function and determines
    their suitability for various applications. However, current experimental approaches, primarily thermal
    proteome profiling, are expensive, labor-intensive, and have limited proteome and species coverage."""
            ]

            Html.p [
                Bulma.size.isSize5
                prop.text """To close the gap between available experimental data and sequence information, a novel protein thermal
    stability predictor called DeepSTABp has been developed. DeepSTABp uses a transformer-based Protein
    Language model for sequence embedding and state-of-the-art feature extraction in combination with other
    deep learning techniques for end-to-end protein Tm prediction. DeepSTABp can predict the thermal stability
    of a wide range of proteins, making it a powerful and efficient tool for large-scale prediction.
    The model captures the structural and biological properties that impact protein stability, and it allows
    for the identification of the structural features that contribute to protein stability."""
            ]
        ]
    ]