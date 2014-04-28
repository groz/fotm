namespace FotM.Athena

// extensions

module Seq =
    let mini s = s |> Seq.mapi (fun i x -> (i, x)) |> Seq.minBy snd
    let miniBy f s = s |> Seq.mapi (fun i x -> (i, f(x))) |> Seq.minBy snd

module List =
    let mini s = s |> List.mapi (fun i x -> (i, x)) |> List.minBy snd
    let miniBy f s = s |> List.mapi (fun i x -> (i, f(x))) |> List.minBy snd

module Array =
    let mini s = s |> Array.mapi (fun i x -> (i, x)) |> Array.minBy snd
    let miniBy f s = s |> Array.mapi (fun i x -> (i, f(x))) |> Array.minBy snd

module Math =
    let n_choose_k n k = 
        let rec choose lo  =
            function
            |0 -> [[]]
            |i -> [for j in lo .. (Array.length n)-1 do
                   for ks in choose (j+1) (i-1) do
                   yield n.[j] :: ks ] 
                in choose 0  k

    let squaredEuclideanDistance (a: float[]) (b: float[]) : float =
        if a.Length <> b.Length then
            failwith "Dimensions must be equal"

        Array.fold2 (fun acc x y -> acc + (x - y)**2.0) 0.0 a b

    let euclideanDistance(a: float[])(b: float[]) =
        squaredEuclideanDistance a b |> sqrt

    type Vector = float[]

    module VectorOps = 
        let (+) (left: Vector) (right: Vector) : Vector =
            Array.zip left right |> Array.map (fun (l, r) -> l + r)

        let (*) (v: Vector) (scale: float) : Vector = v |> Array.map ((*) scale)

        let (/) (v: Vector) (scale: float) : Vector = v * (1.0/scale)

        let zeroVector n : Vector = [|for i in 1..n do yield 0.0|]

        let mean (n: int) (vectors: Vector array) =
            if vectors.Length <> 0 then
                ( vectors |> Array.reduce (fun acc v -> acc + v) ) / float(vectors.Length)
            else
                zeroVector(n)