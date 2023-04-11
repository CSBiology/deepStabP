module DeepStabP.Types

// this is enum so it is correctly serialized in json
/// <summary>Used to determine mode of execution in deepStabP prediction.</summary>
[<RequireQualifiedAccess>]
type MT_Mode =
| Lysate = 0
| Cell = 1

// Use this module to hold functions for MT_Mode as Enumerables cannot have members
module MT_Mode =

    let toString (mode:MT_Mode) =
        match mode with
        | MT_Mode.Lysate    -> "Lysate"
        | MT_Mode.Cell      -> "Cell"
        | anythingElse      -> failwith $"MT_Mode Enumerable {anythingElse} not recognized."
        
/// Metadata for storage with all information for session
type ProcessMetadata = {
    DateCreation: System.DateTime
    Session: System.Guid
    MT_Mode: MT_Mode
    Growth_Temp: float
    /// This determines which package bin (e.g. grouped every 10 aa sequences) the process currently is.
    ChunkIndex: int
    ChunkCount: int
} with
    static member init (id: System.Guid, mt_mode, growth_temp) = {
        DateCreation= System.DateTime.UtcNow
        Session     = id
        MT_Mode     = mt_mode
        Growth_Temp = growth_temp
        ChunkIndex  = 0
        ChunkCount  = 0
    }

/// Parsed fasta format
/// Communcation type between ui and python api. mirrored in api main.py
type FastaRecord = {
    /// Fasta Header (not only name)
    header: string
    /// Protein sequence
    sequence: string
} with
    static member init(name, feature) = {
        header      = name
        sequence    = feature
    }

/// Communcation type between ui and python api. mirrored in api main.py
type PredictorInfo = {
    growth_temp: float
    mt_mode: MT_Mode
    fasta: FastaRecord []
} with
    static member create(gtemp, mtMode, fasta) = {
        growth_temp = gtemp
        mt_mode = mtMode
        fasta = fasta
    }

/// Response type with result information
type PredictorResponse = {
    Protein     : string
    MeltingTemp : float
} with
    static member create(prot, mt) = {
        Protein = prot
        MeltingTemp = mt
    }