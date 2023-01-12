module Client.View.InputView

open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Feliz

//https://zaid-ajaj.github.io/Feliz/#/Hooks/UseElmish

open State

type private SeqMode =
| Single
| Fasta

type private InputState = {
    SeqMode             : SeqMode option
    SingleSequence      : string
    HasValidFasta       : bool
    FastaFileInputName  : string
    FastaFileInput      : string []
    InvalidFastaChars   : char list
    ComputationMode     : State.ComputationMode
    OrganismModel       : OrganismModel
} with
    static member init = {
        SeqMode             = None
        SingleSequence      = ""
        HasValidFasta       = false
        FastaFileInputName  = ""
        FastaFileInput      = Array.empty
        InvalidFastaChars   = List.empty
        ComputationMode     = ComputationMode.IMLP
        OrganismModel       = NonPlant
    }

let private init() = InputState.init, Cmd.none

type private InputMsg =
| Reset
| UpdateSeqMode                 of SeqMode option
| UpdateOrganismModel           of State.OrganismModel
| SingleSequenceInput_Handler   of string
| FastaUpload_Handler           of string * string
| FastaValidation               of Result<string,char list>

let private validateFastaText (fsa:string) =
    let allSeqs =
        fsa
        |> fun x -> x.Replace("\r\n","\n")
        |> fun x -> x.Split('\n')
        |> Array.filter (fun x -> not (x.StartsWith(">")))
    let validSet =
        set [
            'A';'C';'D';'E';'F';'G';'H';'I';'K';'L';'M';'N';'O';'P';'Q';'R';'S';'T';'U';'V';'W';'Y';'X';'J';'Z';'B';'*';'-';'\n';'\r'
            'a';'c';'d';'e';'f';'g';'h';'i';'k';'l';'m';'n';'o';'p';'q';'r';'s';'t';'u';'v';'w';'y';'x';'j';'z';'b'
            ]

    let invalidChars =
        allSeqs
        |> Array.map (String.filter (fun x -> not (validSet.Contains(x))))
        |> Array.filter (fun s -> s.Length > 0)
        |> String.concat ""
        |> fun x -> [for c in x do yield c]
        |> List.distinct

    let isValid = invalidChars.Length = 0

    if isValid then Ok fsa else Error (invalidChars)

let private update (msg: InputMsg) (state:InputState) =
    match msg with
    | Reset -> init()
    | UpdateSeqMode (mode) ->
        { state with SeqMode = mode}, Cmd.none
    | UpdateOrganismModel (org) ->
        { state with OrganismModel = org }, Cmd.none
    | SingleSequenceInput_Handler (str) ->
        let mode = if str <> "" then Some Single else None
        let nextState = {state with SingleSequence = str; SeqMode = mode}
        let validateCmd =
            Cmd.OfFunc.perform
                validateFastaText
                str
                FastaValidation
        nextState, validateCmd
    | FastaUpload_Handler (fileData,fileName) -> 
        let mode = if fileData.Length <> 0 then Some Single else None
        let nextState = {
            state with
                FastaFileInput =
                    fileData.Split('>')
                    |> fun x -> [|yield ([x.[0];x.[1]] |> String.concat ""); yield! x.[2 ..]|]
                    |> Array.map (sprintf ">%s")
                FastaFileInputName = fileName
                SeqMode = mode
            }
        let validateCmd =
            Cmd.OfFunc.perform
                validateFastaText
                fileData
                FastaValidation

        nextState,validateCmd
    | FastaValidation (Ok _) ->
        let updatedModel = {state with HasValidFasta = true; InvalidFastaChars = []}
        updatedModel, Cmd.none

    | FastaValidation (Error invalidChars) ->
        let updatedModel = {state with HasValidFasta = false; InvalidFastaChars = invalidChars}
        updatedModel, Cmd.none

let private validateInputState (state:InputState) =
    match state.SeqMode with
    | Some Single ->
        printfn "Single" 
        match state.SingleSequence with
        | "" -> false, "No data provided"
        | _ ->
            match state.HasValidFasta with
            | false -> false, "Fasta is invalid"
            | _ -> true, "Start computation"
    | Some Fasta -> 
        printfn "Fasta" 
        match state.FastaFileInput with
        | [||] -> false, "No data provided"
        | x when x.Length > 1000 ->
            false, "Too many sequences (>1000)."
        | _ ->
            match state.HasValidFasta with
            | false -> false, "Fasta is invalid"
            | _ -> true, "Start computation"
    | None ->
        printfn "None"
        false, "No data provided"

let private modeSelection (state : InputState) (setState : InputMsg -> unit) =
    Field.div [] [
        match state.SeqMode with
        | Some Single | None ->
            Textarea.textarea [
                Textarea.Size Size.IsMedium   
                Textarea.Placeholder "insert a single amino acid sequence in FASTA format (with or without header)"
                Textarea.OnChange (fun e ->
                    let sequence = e.Value//!!e.target?value
                    SingleSequenceInput_Handler sequence |> setState
                )
            ] []
        | Some Fasta ->
            File.file [File.IsBoxed;File.IsFullWidth;File.HasName] [
                File.Label.label [] [
                    JsInterop.FileReader.singleFileInput [
                        Props.Hidden true
                        JsInterop.FileReader.FileInputHelper.React.OnTextReceived(fun x -> FastaUpload_Handler (x.Data,x.Name) |> setState)
                    ] 
                    File.cta [CustomClass "fastaFileUploadBtn"] [
                        Heading.h4 [] [str "Click to choose a file"]
                        File.icon [] [Html.i [ prop.className "fa fa-upload"]]
                    ]
                    if state.FastaFileInputName <> "" then File.name [] [str state.FastaFileInputName]
                ]
            ]
    ]

open Feliz.UseElmish

let private inputLeft (isValidState:bool) (state: InputState) (setState: InputMsg -> unit) =

    let leftHeader,leftAlternative =
        match state.SeqMode with 
        | Some Single | None -> "Or upload a ", "file"
        | Some Fasta -> "Or insert a single amino acid ", "sequence"

    Column.column [Column.Width (Screen.Desktop, Column.Is7);Column.CustomClass "leftSelector"] [
        Heading.h3 [] [str "Input"]
        hr []
        if (isValidState && not state.HasValidFasta) then
            p [Class "is-danger"] [str "Your fasta contained invalid characters:"]
            p [Class "is-danger"] [str (sprintf "%A" state.InvalidFastaChars)   ]
            Button.button [Button.CustomClass "is-danger";Button.OnClick (fun _ -> Reset |> setState)] [str "Click to reset Input"]
        modeSelection state setState
        Heading.h5 [Heading.IsSubtitle] [
            str leftHeader
            a [ Class "leftAlternative"
                Props.OnClick
                    (fun _ ->
                        match state.SeqMode with 
                        | Some Single | None -> UpdateSeqMode (Some Fasta) |> setState
                        | Some Fasta -> UpdateSeqMode (Some Single) |> setState
                    )] [
                str leftAlternative
            ]
        ]
    ]

let private startPredictionRight (hasJobRunning:bool) (isValidState:bool) (buttonMsg:string) (state: InputState) (setState: InputMsg -> unit) (dispatch: Msg -> unit) =
    Column.column [Column.Width (Screen.Desktop, Column.Is5);Column.CustomClass "rightSelector"] [
        Heading.h3 [] [str "Start Prediction"]
        hr []
        Field.div [] [
            Button.button [
                Button.Disabled (not isValidState)
                Button.CustomClass (if isValidState then "is-success" else "is-danger")
                Button.IsLoading hasJobRunning
                Button.IsFullWidth
                Button.CustomClass "startBtn"
                Button.OnClick (fun _ ->
                    match state.SeqMode with
                    | Some Single   -> SingleSequenceRequest state.ComputationMode |> dispatch
                    | Some Fasta    -> FastaUploadRequest state.ComputationMode |> dispatch
                    | _ -> ())
            ] [str buttonMsg ]
        ]
        Label.label [Label.Size IsMedium; Label.Props [Style [CSSProp.Color "$csb-orange"]]] [str "select iMLP Model:"]
        Field.div [Field.IsGrouped] [
            let isNonPlant = state.OrganismModel = OrganismModel.NonPlant
            Control.div [Control.Props [Style [CSSProp.Color "$csb-orange"]]] [
                Checkbox.checkbox [Props [Style [CSSProp.Color "$csb-orange"] ]] [
                    Checkbox.input [Props [OnClick (fun _ -> UpdateOrganismModel OrganismModel.NonPlant |> setState); Checked isNonPlant]]
                    b [] [ str "NonPlant"]
                ]
            ]
            Control.div [Control.Props [Style [CSSProp.Color "$csb-orange"]]] [
                Checkbox.checkbox [Props [Style [CSSProp.Color "$csb-orange"]]] [
                    Checkbox.input [Props[OnClick (fun _ -> UpdateOrganismModel OrganismModel.Plant |> setState); Checked (not isNonPlant)]]
                    b [] [ str "Plant"]
                ]
            ]
        ]
    ]

[<ReactComponent>]
let View (hasJobRunning: bool) (dispatch : Msg -> unit) =
    let state, setState = React.useElmish(init, update, [||])

    let isValidState, buttonMsg = validateInputState state
        
    div [Style [FlexGrow 1; Display DisplayOptions.Flex; FlexDirection "column"]] [
        Columns.columns [Columns.CustomClass "ProcessDecision"; Columns.Props [Style [FlexGrow 1]]] [
            inputLeft isValidState state setState
            startPredictionRight hasJobRunning isValidState buttonMsg state setState dispatch
        ]
    ]
