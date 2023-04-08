module FastaReader

open System
open System.IO
open DeepStabP.Types

// if you run this locally change this value to allow parsing more values at once
let maxCount = 1000

let private replaceLetters (str:string) = 
    str.ToUpper()
    |> String.collect (fun c -> 
        match c with
        |'O'|'U'|'J'|'Z'|'B' -> "X"
        | _ -> string c
    )

let addWhiteSpaceInBetween (str: string) =
    let mutable isFirst = true
    let sb = System.Text.StringBuilder()
    for c in str do
        if isFirst then 
            isFirst <- false
        else
            sb.Append(" ") |> ignore
        sb.Append(c) |> ignore
    sb.ToString()

let private read (reader:TextReader) =
    let mutable noNameIterator = 0
    let mutable iterator = 0
    let rec readRecords (acc: FastaRecord list) (currentHeader: string) (currentSeq: string) =
        let nextLine = reader.ReadLine()
        match nextLine with
        | null | _ when (iterator >= maxCount) || (isNull nextLine) -> List.rev ({ header = currentHeader; sequence = addWhiteSpaceInBetween currentSeq } :: acc)
        | line when line.StartsWith(">!") ->
            iterator <- iterator + 1
            noNameIterator <- noNameIterator + 1
            let newHeader = sprintf "Unknown%i" noNameIterator
            let line' = line.Substring(2).Trim() |> replaceLetters
            if currentHeader <> "" && currentSeq <> "" then
                readRecords ({ header = currentHeader; sequence = addWhiteSpaceInBetween currentSeq } :: acc) newHeader line'
            else
                readRecords acc newHeader line'
        | line when line.StartsWith(">") ->
            let newHeader = line.Substring(1)
            iterator <- iterator + 1
            if currentHeader <> "" && currentSeq <> "" then
                readRecords ({ header = currentHeader; sequence = addWhiteSpaceInBetween currentSeq } :: acc) newHeader ""
            else
                readRecords acc newHeader ""
        | line ->
            let line' = line.Trim() |> replaceLetters
            readRecords acc currentHeader (currentSeq + line')
    let res = readRecords [] "" ""
    reader.Dispose()
    res

let private readOfBytes (data:byte []) =
    use ms = new System.IO.MemoryStream(data)
    use reader = new System.IO.StreamReader(ms)
    let res = read reader
    ms.Dispose()
    res

let private readOfString (str:string) =
    use reader = new System.IO.StringReader(str)
    let res = read reader
    res

type FastaRecord with
    static member ofFile (data:byte []) = readOfBytes(data)

    static member ofString (str:string) = readOfString(str)