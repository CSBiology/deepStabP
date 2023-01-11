module DeepStabP.Types

// mirrored in api main.py
type PredictorInfo = {
    growth_temp: int
}

type PredictorResponse = {
    Protein     : string
    MeltingTemp : float
} with
    static member create prot mt = {
        Protein = prot
        MeltingTemp = mt
    }