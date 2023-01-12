module DeepStabP.Types

// this is enum so it is correctly serialized in json
/// <summary>Used to determine mode of execution in deepStabP prediction.</summary>
[<RequireQualifiedAccess>]
type MT_Mode =
| Lysate = 0
| Cell = 1

// mirrored in api main.py
type PredictorInfo = {
    growth_temp: int
    mt_mode: MT_Mode
    fasta: string
}

type PredictorResponse = {
    Protein     : string
    MeltingTemp : float
} with
    static member create prot mt = {
        Protein = prot
        MeltingTemp = mt
    }