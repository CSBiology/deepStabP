module Api

open Fable.Remoting.Client
open Shared

let serviceApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IServiceApi>