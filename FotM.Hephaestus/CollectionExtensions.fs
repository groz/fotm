namespace FotM.Hephaestus

module CollectionExtensions =

    module Seq =
        let mini s = s |> Seq.mapi (fun i x -> (i, x)) |> Seq.minBy snd
        let miniBy f s = s |> Seq.mapi (fun i x -> (i, f(x))) |> Seq.minBy snd
        let maxi s = s |> Seq.mapi (fun i x -> (i, x)) |> Seq.maxBy snd
        let maxiBy f s = s |> Seq.mapi (fun i x -> (i, f(x))) |> Seq.maxBy snd
        let filteri f s = s  |> Seq.mapi (fun i x -> (i, x)) |> Seq.filter f |> Seq.map snd

    module Array =
        let randomElement (rng: System.Random) (arr: 'a array) = arr.[rng.Next arr.Length]

    // https://gist.github.com/vasily-kirichenko/6855137
    let toMultiMap xs =
        Seq.fold (fun res (k, v) ->
            match Map.tryFind k res with
            | Some vs -> Map.add k (v :: vs) res
            | None -> Map.add k [v] res) Map.empty xs

    let toOption(x: System.Nullable<_>) =
        if x.HasValue then Some x.Value
        else None

    let inline isNull< ^a when ^a : not struct> (x:^a) =
        obj.ReferenceEquals (x, Unchecked.defaultof<_>)

    let inline asOption< ^a when ^a : not struct> (x:^a) =
        if not (isNull x) then Some x
        else None
