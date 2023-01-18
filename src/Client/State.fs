module State

open Fable.Remoting.Client
open Fable.SimpleJson

type System.Exception with
    member this.GetPropagatedError() =
        match this with
        | :? ProxyRequestException as exn ->
            try
                let response = exn.ResponseText |> Json.parseAs<{| error:string; ignored : bool; handled : bool |}>
                response.error
            with
                | ex -> ex.Message
        | ex ->
            ex.Message

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