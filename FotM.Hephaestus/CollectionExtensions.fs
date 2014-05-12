namespace FotM.Hephaestus

module CollectionExtensions =

    module Seq =
        let mini s = s |> Seq.mapi (fun i x -> (i, x)) |> Seq.minBy snd
        let miniBy f s = s |> Seq.mapi (fun i x -> (i, f(x))) |> Seq.minBy snd
        let filteri f s = s  |> Seq.mapi (fun i x -> (i, x)) |> Seq.filter f |> Seq.map snd

    module List =
        let mini s = s |> List.mapi (fun i x -> (i, x)) |> List.minBy snd
        let miniBy f s = s |> List.mapi (fun i x -> (i, f(x))) |> List.minBy snd

    module Array =
        let mini s = s |> Array.mapi (fun i x -> (i, x)) |> Array.minBy snd
        let miniBy f s = s |> Array.mapi (fun i x -> (i, f(x))) |> Array.minBy snd
        let randomElement (rng: System.Random) (arr: 'a array) = arr.[rng.Next arr.Length]

    // https://gist.github.com/vasily-kirichenko/6855137
    let toMultiMap xs =
        Seq.fold (fun res (k, v) ->
            match Map.tryFind k res with
            | Some vs -> Map.add k (v :: vs) res
            | None -> Map.add k [v] res) Map.empty xs