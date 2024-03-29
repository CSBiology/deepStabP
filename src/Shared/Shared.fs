namespace Shared

open System


module Urls =

    [<LiteralAttribute>]
    let Contact = "https://csb.bio.uni-kl.de/#contact"

    [<LiteralAttribute>]
    let GitHubRepo = "https://github.com/CSBiology/deepStabP"

    [<LiteralAttribute>]
    let GitHubFormatInfo = "https://github.com/CSBiology/deepStabP#supported-formats"

module Emails =

    /// "timo.muehlhaus@rptu.de", obfuscated by https://www.email-obfuscator.com
    [<LiteralAttribute>]
    let MainContact = "javascript:location='mailto:\u0074\u0069\u006d\u006f\u002e\u006d\u0075\u0065\u0068\u006c\u0068\u0061\u0075\u0073\u0040\u0072\u0070\u0074\u0075\u002e\u0064\u0065';void 0"

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

    let builderVersioned version typeName methodName =
        sprintf "/api/%s/%s/%s" version typeName methodName

type IServiceApi = {
    getVersion: unit -> Async<string>
}

type HelloWorld = {
    Hello: string
}

open DeepStabP.Types

// Sadly need these use-once-and-forget-about-them types, as anonymous record types cannot be shared between assemblies.
type PostDataBytes = {metadata: ProcessMetadata; data: byte []}
type PostDataString = {metadata: ProcessMetadata; data: string}
type GetData_Response = {chunkIndex: int; results: PredictorResponse list}

type IDeepStabPApi = {
    helloWorld: unit -> Async<HelloWorld>
    getVersion: unit -> Async<string>
    postDataBytes: PostDataBytes -> Async<int>
    postDataString: PostDataString -> Async<int>
    getData: {|session: Guid|} -> Async<GetData_Response>
    cleanStorage: {|session: Guid|} -> Async<unit>
}
