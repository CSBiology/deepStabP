module State

open Shared

type ComputationMode =
| Legacy
| IMLP

type OrganismModel =
| Plant
| NonPlant

type Model = {
    AppVersion      : string
    HasJobRunning   : bool
} with
    static member init = {
        HasJobRunning   = false
        AppVersion      = ""
    }

type Msg =
    | GetVersionRequest
    | GetVersionResponse                of string
    | SingleSequenceRequest             of ComputationMode
    | FastaUploadRequest                of ComputationMode
