module Api

open Fable.Remoting.Client
open Shared

let serviceApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IServiceApi>

let deepStabPApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder (Route.builderVersioned "v1")
    |> Remoting.buildProxy<IDeepStabPApi>