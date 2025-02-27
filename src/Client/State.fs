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
        /// Only used in Sequence mode, as fasta validation for file input is done on backend
        HasValidFasta       : bool
        FastaFileName       : string
        FastaFileData       : byte []
        FastaFileSize       : int
        InvalidFastaChars   : char list
        MT_Mode             : MT_Mode
        GrowthTemperature   : float
    } with
        static member init = {
            SeqMode             = None
            Sequence            = ""
            HasValidFasta       = false
            FastaFileName       = ""
            FastaFileData       = Array.empty
            FastaFileSize       = 0
            InvalidFastaChars   = List.empty
            MT_Mode             = MT_Mode.Lysate
            GrowthTemperature   = 22.
        }

    type InputMsg =
    | Reset
    | RemoveFasta
    | UpdateSeqMode             of SeqMode option
    | UpdateMT_Mode             of MT_Mode
    | UpdateGrowthTemp          of float
    | SequenceInput_Handler     of string
    | FastaUpload_Handler       of data:byte [] * name:string * fileSize: int
    | FastaValidation           of Result<string,char list>

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
    SessionId           : System.Guid
    /// The current chunk index, instead of None it is -1 as default
    ChunkIndex          : int
    /// The count of all chunks
    ChunkCount          : int
    Results             : DeepStabP.Types.PredictorResponse list
    Version             : Versions
    /// This indicates if the active process should proceed running, if set to false, any GetDataRequest loop will be stopped.
    KeepJobRunning      : bool
    HasJobRunning       : bool
    Page                : Page
} with
    static member init() = {
        SessionId           = System.Guid.NewGuid()
        Results             = List.empty
        ChunkIndex          = -1
        ChunkCount          = 0
        KeepJobRunning      = false
        HasJobRunning       = false
        Page                = Page.Main
        Version             = Versions.init
    }

type Msg =
    | GenericError              of exn
    | UpdatePage                of Page
    | GetVersionUIRequest
    | GetVersionUIResponse      of string
    | GetVersionApiRequest
    | GetVersionApiResponse     of string
    | DisableJobRunning
    | CleanStorage              of System.Guid
    | PostDataString            of Shared.PostDataString
    | PostDataBytes             of Shared.PostDataBytes
    | PostDataResponse          of ChunkCount:int
    /// Uses the current chunk index to determine if it should be looped
    | GetDataRequest            of {|ChunkIndex: int; Results: DeepStabP.Types.PredictorResponse list|}
    | GetDataRequestError       of exn
    | DownloadResults