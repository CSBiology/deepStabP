module DeepStabP.Types

// this is enum so it is correctly serialized in json
/// <summary>Used to determine mode of execution in deepStabP prediction.</summary>
[<RequireQualifiedAccess>]
type MT_Mode =
| Lysate = 0
| Cell = 1

// Use this module to hold functions for MT_Mode as Enumerables cannot have members
module MT_Mode =

    let toString (mode:MT_Mode) =
        match mode with
        | MT_Mode.Lysate    -> "Lysate"
        | MT_Mode.Cell      -> "Cell"
        | anythingElse      -> failwith $"MT_Mode Enumerable {anythingElse} not recognized."
        

// mirrored in api main.py
type PredictorInfo = {
    growth_temp: float
    mt_mode: MT_Mode
    fasta: string
} with
    static member create gtemp mtMode fasta = {
        growth_temp = gtemp
        mt_mode = mtMode
        fasta = fasta
    }

type PredictorResponse = {
    Protein     : string
    MeltingTemp : float
} with
    static member create prot mt = {
        Protein = prot
        MeltingTemp = mt
    }