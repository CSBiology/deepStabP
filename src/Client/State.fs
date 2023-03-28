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

module Input =

    open DeepStabP.Types

    type SeqMode =
    | Sequence
    | File

    type InputState = {
        /// decides between string input and file input
        SeqMode             : SeqMode option
        /// Storage for string input in textarea
        Sequence            : string
        HasValidFasta       : bool
        FastaFileName       : string
        FastaFileData       : string
        InvalidFastaChars   : char list
        MT_Mode             : MT_Mode
        GrowthTemperature   : float
    } with
        static member init = {
            SeqMode             = None
            Sequence            = ""
            HasValidFasta       = false
            FastaFileName       = ""
            FastaFileData       = ""
            InvalidFastaChars   = List.empty
            MT_Mode             = MT_Mode.Lysate
            GrowthTemperature   = 22.
        }

    type InputMsg =
    | Reset
    | RemoveFasta
    | UpdateSeqMode                 of SeqMode option
    | UpdateMT_Mode                 of MT_Mode
    | UpdateGrowthTemp              of float
    | SequenceInput_Handler         of string
    | FastaUpload_Handler           of string * string
    | FastaValidation               of Result<string,char list>

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

[<RequireQualifiedAccess>]
type Page =
| Main
| About
| PrivacyPolicy
| Contact

type Model = {
    Version         : Versions
    Result          : DeepStabP.Types.PredictorResponse []
    HasJobRunning   : bool
    Page            : Page
} with
    static member init = {
        Version         = Versions.init
        Result          = Array.empty
        HasJobRunning   = false
        Page            = Page.Main
    }

type Msg =
    | UpdatePage                        of Page
    | GetVersionUIRequest
    | GetVersionUIResponse              of string
    | GetVersionApiRequest
    | GetVersionApiResponse             of string
    | PredictionRequest                 of DeepStabP.Types.PredictorInfo
    | PredictionResponse                of Result<DeepStabP.Types.PredictorResponse [],exn>