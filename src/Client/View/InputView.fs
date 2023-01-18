module Client.View.InputView

open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Feliz
open DeepStabP.Types

//https://zaid-ajaj.github.io/Feliz/#/Hooks/UseElmish

open State

module private State =

    type SeqMode =
    | Single
    | Fasta

    type InputState = {
        SeqMode             : SeqMode option
        SingleSequence      : string
        HasValidFasta       : bool
        FastaFileName       : string
        FastaFileData       : string
        InvalidFastaChars   : char list
        MT_Mode             : MT_Mode
        GrowthTemperature   : float
    } with
        static member init = {
            SeqMode             = None
            SingleSequence      = ""
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
    | SingleSequenceInput_Handler   of string
    | FastaUpload_Handler           of string * string
    | FastaValidation               of Result<string,char list>

module private Update =

    open State

    let init() = State.InputState.init, Cmd.none

    let validateFastaText (fsa:string) =
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

    let update (msg: InputMsg) (state:InputState) =
        match msg with
        | Reset -> init()
        | RemoveFasta ->
            {state with FastaFileName = ""; FastaFileData = ""; HasValidFasta = false}, Cmd.none
        | UpdateSeqMode (mode) ->
            { state with SeqMode = mode}, Cmd.none
        | UpdateMT_Mode mode ->
            { state with MT_Mode = mode }, Cmd.none
        | UpdateGrowthTemp gt ->
            { state with GrowthTemperature = gt }, Cmd.none
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
            let mode = if fileData.Length <> 0 then Some Fasta else None
            let nextState = {
                state with
                    FastaFileData = fileData
                    FastaFileName = fileName
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

open State

let private validateInputState (versions: State.Versions) (state:InputState) =
    let validateInput() =
        match state.SeqMode with
        | Some Single ->
            match state.SingleSequence with
            | "" -> false, "No data provided"
            | _ ->
                match state.HasValidFasta with
                | false -> false, "Fasta is invalid"
                | _ -> true, "Start computation"
        | Some Fasta -> 
            match state.FastaFileData with
            | "" -> false, "No data provided"
            | x when x.Split([|'>'|],System.StringSplitOptions.RemoveEmptyEntries).Length > 1000 ->
                false, "Too many sequences (>1000)."
            | _ ->
                match state.HasValidFasta with
                | false -> false, "Fasta is invalid"
                | _ -> true, "Start computation"
        | None ->
            false, "No data provided"
    match versions with
    | noServerConnection when versions.UI = "" ->
        false, "No connection to server"
    | noApiConnection when versions.Api = "" ->
        false, "No connection to predictor service"
    | _ -> validateInput()
module private UploadHandler =
    open Fable.Core.JsInterop

    [<Literal>]
    let id = "droparea"
    let updateMsg = FastaUpload_Handler

    let setActive_DropArea (id:string) =
        let ele = Browser.Dom.document.getElementById(id)
        ele?style?boxShadow <- "2px 2px 10px"

    let setInActive_DropArea (id:string) =
        let ele = Browser.Dom.document.getElementById(id)
        ele?style?boxShadow <- "unset"

    let onchange setState =
        fun (e:Browser.Types.Event) ->
            let reader = Browser.Dom.FileReader.Create()
            let files : Browser.Types.File [] = e.target?files
            let file = files[0]
            reader.onload <- (fun _ -> updateMsg (string reader.result, file.name) |> setState )

            reader.readAsText(file)

    let ondrop setState =
        fun (e: Browser.Types.DragEvent) ->
            e.preventDefault()
            if e.dataTransfer.items <> null then
                let item = e.dataTransfer.items.[0]
                if item.kind = "file" then
                    setInActive_DropArea id
                    let file = item.getAsFile()
                    let reader = Browser.Dom.FileReader.Create()
                    reader.onload <- (fun _ -> updateMsg (string reader.result, file.name) |> setState )
                    reader.readAsText(file)

let private modeSelection (state : InputState) (setState : InputMsg -> unit) =
    Field.div [
        Field.Props [
            Id UploadHandler.id
            OnDragEnter(fun e ->
                e.preventDefault()
                if e.dataTransfer.items <> null then
                    let item = e.dataTransfer.items.[0]
                    if item.kind = "file" then
                        UploadHandler.setActive_DropArea UploadHandler.id
            )
            OnDragLeave(fun e ->
                e.preventDefault()
                UploadHandler.setInActive_DropArea UploadHandler.id
            )
            OnDragOver(fun e -> e.preventDefault())
            OnDrop <| UploadHandler.ondrop setState
        ]
    ] [
        match state.SeqMode with
        | Some Single | None ->
            Textarea.textarea [
                Textarea.Color IsInfo
                Textarea.Size Size.IsMedium   
                Textarea.Placeholder "insert a single amino acid sequence in FASTA format (with or without header)"
                Textarea.OnChange (fun e ->
                    let sequence = e.Value//!!e.target?value
                    SingleSequenceInput_Handler sequence |> setState
                )
            ] []
        | Some Fasta ->
            File.file [File.IsBoxed;File.IsFullWidth;File.HasName] [
                File.Label.label [ ] [
                    File.input [ Props [
                        OnChange <| UploadHandler.onchange setState
                    ]]
                    File.cta [CustomClass "fastaFileUploadBtn"] [
                        Heading.h4 [Heading.Props [Style [PointerEvents "none"; Color "white"]]] [str "Click to choose a file"]
                        File.icon [Props [Style [PointerEvents "none"]]] [Html.i [ prop.className "fa fa-upload"]]
                    ]
                    if state.FastaFileName <> "" then
                        File.name [Modifiers [Modifier.TextAlignment (Screen.All, TextAlignment.Centered)]] [
                            str state.FastaFileName
                            Button.span [
                                Button.OnClick (fun e ->
                                    e.preventDefault()
                                    e.stopPropagation()
                                    RemoveFasta |> setState
                                )
                                Button.Size IsSmall
                                Button.IsGhost
                                Button.Props [Style [
                                    Float FloatOptions.Right
                                    TextAlign TextAlignOptions.Center
                                    Cursor "hover"
                                ]]
                            ] [
                                Html.i [ prop.className "fa-solid fa-x"]
                            ]
                        ]
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

let private mtMode_checkbox (state:InputState) (setState: InputMsg -> unit) (mt_mode:MT_Mode) =
    Control.div [] [
        Checkbox.checkbox [Props [Style [CSSProp.Color "$csb-orange";] ]] [
            Checkbox.input [Props [
                Checked (state.MT_Mode = mt_mode)
                OnChange (fun _ -> UpdateMT_Mode mt_mode |> setState)
            ]]
            b [Style [MarginLeft ".5rem"]; Class "has-text-white-ter"] [str <| MT_Mode.toString mt_mode]
        ]
    ]

let private startPredictionRight (hasJobRunning:bool) (isValidState:bool) (buttonMsg:string) (state: InputState) (setState: InputMsg -> unit) (dispatch: Msg -> unit) =
    Column.column [Column.Width (Screen.Desktop, Column.Is5);Column.CustomClass "rightSelector"] [
        Heading.h3 [] [str "Start Prediction"]
        hr []
        Label.label [Label.Size IsSmall; Label.CustomClass "has-text-info-light"] [str "Growth temperature"]
        Field.div [] [
            Input.number [
                Input.Size IsSmall; Input.Color Color.IsBlack
                Input.ValueOrDefault (string state.GrowthTemperature)
                Input.Props [Step 0.1]
                Input.OnChange(fun e ->
                    let f = float e.Value
                    UpdateGrowthTemp f |> setState
                )
            ]
        ]
        Label.label [Label.Size IsSmall; Label.CustomClass "has-text-info-light"] [str "Select environment for melting temperature prediction."]
        Field.div [Field.IsGrouped] [
            mtMode_checkbox state setState MT_Mode.Lysate
            mtMode_checkbox state setState MT_Mode.Cell
        ]
        Field.div [] [
            Button.button [
                Button.Disabled (not isValidState)
                Button.CustomClass (if isValidState then "is-success" else "is-danger")
                Button.IsLoading hasJobRunning
                Button.IsFullWidth
                Button.CustomClass "startBtn"
                Button.OnClick (fun _ ->
                    if isValidState then
                        let fasta = if state.SeqMode = Some SeqMode.Single then state.SingleSequence else state.FastaFileData 
                        let info = DeepStabP.Types.PredictorInfo.create state.GrowthTemperature state.MT_Mode fasta
                        PredictionRequest info |> dispatch
                )
            ] [str buttonMsg]
        ]
    ]

open Update

[<ReactComponent>]
let View (versions: State.Versions) (hasJobRunning: bool) (dispatch : Msg -> unit) =
    let state, setState = React.useElmish(init, update, [||])

    let isValidState, buttonMsg = validateInputState versions state
        
    div [Style [FlexGrow 1; Display DisplayOptions.Flex; FlexDirection "column"]] [
        Columns.columns [Columns.CustomClass "ProcessDecision"; Columns.Props [Style [FlexGrow 1]]] [
            inputLeft isValidState state setState
            startPredictionRight hasJobRunning isValidState buttonMsg state setState dispatch
        ]
    ]
