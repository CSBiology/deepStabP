namespace Shared

open System

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type IServiceApi = {
    getVersion: unit -> Async<string>
}