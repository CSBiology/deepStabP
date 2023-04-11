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

let removeFromStorage guid =
    let d_rem = Storage.data.Remove(guid) 
    let md_rem = Storage.metaData.Remove(guid)
    d_rem && md_rem

/// This function checks DateCreation of all metadata and removes all older than two days
let private removeAbandonedData() =
    Storage.metaData |> Seq.filter (fun (KeyValue (key,v)) ->
        let t_now = System.DateTime.UtcNow
        let t_creation = v.DateCreation
        let t_diff = t_now.Subtract(t_creation)
        t_diff > System.TimeSpan(2,0,0,0)
    )
    |> Seq.map (fun (KeyValue (key,v)) -> removeFromStorage key)

/// Returns number of chunks into which data was grouped.
let addToStorage (md: ProcessMetadata) (data: FastaRecord list) =
    // clean storage from any abandoned tasks
    removeAbandonedData() |> ignore
    let data' = data |> Seq.chunkBySize 5
    let chunkCount = data' |> Seq.length
    let md' = {md with ChunkCount = chunkCount}
    Storage.data.Add(md.Session, data')
    Storage.metaData.Add(md.Session, md')
    chunkCount

type ProcessMetadata with
    // Increases ChunkIndex by 1
    member this.increaseChunkIndex() =
        let next = {this with ChunkIndex = this.ChunkIndex + 1}
        Storage.metaData.[this.Session] <- next

