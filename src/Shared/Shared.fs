namespace Shared

open System

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

type IDeepStabPApi = {
    helloWorld: unit -> Async<HelloWorld>
    predict: unit -> Async<PredictorResponse []>
}
