module State

open Shared

type ComputationMode =
| Legacy
| IMLP

type OrganismModel =
| Plant
| NonPlant

type Model = {
    HasJobRunning: bool
} with
    static member init = {
        HasJobRunning = false
    }

type Msg =
    | SingleSequenceRequest             of ComputationMode
    | FastaUploadRequest                of ComputationMode
