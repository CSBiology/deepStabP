[<AutoOpen>]
[<RequireQualifiedAccess>]
module Storage

open System.Collections.Generic
open DeepStabP.Types

module Storage =

    let data: Dictionary<System.Guid, seq<FastaRecord []>> = Dictionary()

    let metaData: Dictionary<System.Guid, ProcessMetadata> = Dictionary()

let getStorage guid =
    Storage.metaData.[guid],
    Storage.data.[guid]

/// Returns number of chunks into which data was grouped.
let addToStorage (md: ProcessMetadata) (data: FastaRecord list) =
    let data' = data |> Seq.chunkBySize 10
    let chunkCount = data' |> Seq.length
    let md' = {md with ChunkCount = chunkCount}
    Storage.data.Add(md.Session, data')
    Storage.metaData.Add(md.Session, md')
    chunkCount

let removeFromStorage guid =
    let d_rem = Storage.data.Remove(guid) 
    let md_rem = Storage.metaData.Remove(guid)
    d_rem && md_rem


type ProcessMetadata with
    // Increases ChunkIndex by 1
    member this.increaseChunkIndex() =
        let next = {this with ChunkIndex = this.ChunkIndex + 1}
        Storage.metaData.[this.Session] <- next

