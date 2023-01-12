module State

open Shared

type ComputationMode =
| Legacy
| IMLP

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
    HasJobRunning   : bool
} with
    static member init = {
        HasJobRunning   = false
        Version         = Versions.init
    }

type Msg =
    | GetVersionUIRequest
    | GetVersionUIResponse              of string
    | GetVersionApiRequest
    | GetVersionApiResponse             of string
    | SingleSequenceRequest             of ComputationMode
    | FastaUploadRequest                of ComputationMode
