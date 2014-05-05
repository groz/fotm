namespace FotM.Hephaestus

module CollectionExtensions =
    module Seq =
        let mini s = s |> Seq.mapi (fun i x -> (i, x)) |> Seq.minBy snd
        let miniBy f s = s |> Seq.mapi (fun i x -> (i, f(x))) |> Seq.minBy snd

    module List =
        let mini s = s |> List.mapi (fun i x -> (i, x)) |> List.minBy snd
        let miniBy f s = s |> List.mapi (fun i x -> (i, f(x))) |> List.minBy snd

    module Array =
        let mini s = s |> Array.mapi (fun i x -> (i, x)) |> Array.minBy snd
        let miniBy f s = s |> Array.mapi (fun i x -> (i, f(x))) |> Array.minBy snd

