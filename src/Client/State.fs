module State

open Shared

type OrganismModel =
| Plant
| NonPlant

type Versions = {
    UI: string
    Api: string
} with
    static member init = {
        UI  = ""
        Api = ""
    }

type Model = {
    Version         : Versions
    Result          : DeepStabP.Types.PredictorResponse []
    HasJobRunning   : bool
} with
    static member init = {
        Version         = Versions.init
        Result          = Array.empty
        HasJobRunning   = false
    }

type Msg =
    | GetVersionUIRequest
    | GetVersionUIResponse              of string
    | GetVersionApiRequest
    | GetVersionApiResponse             of string
    | PredictionRequest                 of DeepStabP.Types.PredictorInfo
    | PredictionResponse                of Result<DeepStabP.Types.PredictorResponse [],exn>