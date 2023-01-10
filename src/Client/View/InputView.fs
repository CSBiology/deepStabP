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
    EULAAccepted        : bool
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
        EULAAccepted        = false
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
            | _ ->  if state.EULAAccepted then
                        true, "Start legacy computation"
                    else
                        true, "Start computation"
    | Some Fasta -> 
        printfn "Fasta" 
        match state.FastaFileInput with
        | [||] -> false, "No data provided"
        | x when x.Length > 1000 ->
            false, "Too many sequences (>1000)."
        | _ ->
            match state.HasValidFasta with
            | false -> false, "Fasta is invalid"
            | _ ->  if state.EULAAccepted then
                        true, "Start legacy computation"
                    else
                        true, "Start computation"
    | None ->
        printfn "None"
        false, "No data provided"

let private modeSelection (state : InputState) (setState : InputMsg -> unit) =
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
                File.cta [Props [Class "file-cta fastaFileUploadBtn"]] [
                    Heading.h4 [] [str "Click to choose a file"]
                    Icon.icon [] [Html.i [ prop.className "fa fa-upload"]]
                ]
                File.name [] [str state.FastaFileInputName]
            ]
        ]

open Feliz.UseElmish

[<ReactComponent>]
let View (hasJobRunning: bool) (dispatch : Msg -> unit) =
    let state, setState = React.useElmish(init, update, [||])

    let isValidState, buttonMsg = validateInputState state

    let leftHeader,leftAlternative =
        match state.SeqMode with 
        | Some Single | None -> "Or upload a ", "file"
        | Some Fasta -> "Or insert a single amino acid ", "sequence"
    
    div [] [
        Columns.columns [Columns.CustomClass "ProcessDecision"] [
            Column.column [Column.Width (Screen.Desktop, Column.Is7);Column.CustomClass "leftSelector"] [
                Columns.columns [] [
                    Column.column [Column.Width (Screen.Desktop, Column.Is3)] []
                    Column.column [Column.Width (Screen.Desktop, Column.Is9)] [
                        yield br []
                        yield Heading.h3 [] [str "Input"]
                        yield hr []
                        if (isValidState && not state.HasValidFasta) then
                            yield p [Class "is-danger"] [str "Your fasta contained invalid characters:"]
                            yield p [Class "is-danger"] [str (sprintf "%A" state.InvalidFastaChars)   ]
                            yield Button.button [Button.CustomClass "is-danger";Button.OnClick (fun _ -> Reset |> setState)] [str "Click to reset Input"]
                        yield modeSelection state setState
                        yield br []
                        yield
                            Heading.h5 [Heading.IsSubtitle]
                                [
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
                        yield br []
                    ]
                ]
            ]
            Column.column [Column.Width (Screen.Desktop, Column.Is5);Column.CustomClass "rightSelector"] [
                Columns.columns [] [
                    Column.column [Column.Width (Screen.Desktop, Column.Is8)] [
                        br []
                        Heading.h3 [] [str "Start Prediction"]
                        hr []
                        
                        Button.button [
                            (if isValidState then
                                Button.Disabled false 
                            else 
                                Button.Disabled true)

                            (if isValidState then
                                Button.CustomClass "is-success"
                            else 
                                Button.CustomClass "is-danger" )

                            Button.IsLoading hasJobRunning
                            Button.IsFullWidth
                            Button.CustomClass "startBtn"
                            Button.OnClick (fun _ ->
                                match state.SeqMode with
                                | Some Single   -> SingleSequenceRequest state.ComputationMode |> dispatch
                                | Some Fasta    -> FastaUploadRequest state.ComputationMode |> dispatch
                                | _ -> ())
                        ] [str buttonMsg ]
                        br []
                        Label.label [Label.Size IsMedium; Label.Props [Style [CSSProp.Color "rgb(237, 125, 49)"]]] [str "select iMLP Model:"]
                        Field.div [Field.IsGrouped] [
                            let isNonPlant = state.OrganismModel = OrganismModel.NonPlant
                            Control.div [Control.Props [Style [CSSProp.Color "rgb(237, 125, 49)"]]] [
                                Checkbox.checkbox [Props [Style [CSSProp.Color "rgb(237, 125, 49)"] ]] [
                                    Checkbox.input [Props [OnClick (fun _ -> UpdateOrganismModel OrganismModel.NonPlant |> setState); Checked isNonPlant]]
                                    b [] [ str "NonPlant"]
                                ]
                            ]
                            Control.div [Control.Props [Style [CSSProp.Color "rgb(237, 125, 49)"]]] [
                                Checkbox.checkbox [Props [Style [CSSProp.Color "rgb(237, 125, 49)"]]] [
                                    Checkbox.input [Props[OnClick (fun _ -> UpdateOrganismModel OrganismModel.Plant |> setState); Checked (not isNonPlant)]]
                                    b [] [ str "Plant"]
                                ]
                            ]
                        ]
                        Control.div [Control.Props [Style [CSSProp.Color "rgb(237, 125, 49)"]]] [
                            Checkbox.checkbox [Props [Style [CSSProp.Color "rgb(237, 125, 49)"]]] [
                                //Checkbox.input [Props[OnClick (fun _ -> EULAAcceptedChange |> dispatch)]]
                                b [] [ str" Use legacy computation model"]
                            ]
                            div [Class "block"] [
                                str "in order to use the targetP-based legacy model you have to agree to iMLP's "
                                //a [ OnClick (fun _ -> ShowEulaModal true |> dispatch)
                                //    Style [Color "white";]] [
                                //    str "end user license agreement (EULA)"
                                //]
                            ]
                        ]
                    ]
                    Column.column [Column.Width (Screen.Desktop, Column.Is4)] []
                ]
                
            ]
        ]
    ]
