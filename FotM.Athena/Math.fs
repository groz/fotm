namespace FotM.Athena

module Math =

    let (./.) x y = 
        (x |> float) / (y |> float)

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

    let normalize(matrix: float[][]): float[][] =
        let columnStats (col: int) =
            let column = matrix |> Array.map (fun row -> row.[col])
            let min = column |> Array.min
            let max = column |> Array.max
            let mean = column |> Array.average
            let scale  = if (max - min = 0.0) then 1.0 else (max - min)
            mean, scale

        let n = matrix.[0].Length
        let m = matrix.Length

        let stats = [0..n-1] |> List.map columnStats

        [|
        for i in 0..m-1 do
        yield
            [|
            for j in 0..n-1 do
            yield (matrix.[i].[j] - fst stats.[j]) / (snd stats.[j])
            |]
        |]

    type Vector = float[]

    module VectorOps = 
        let (+) (left: Vector) (right: Vector) : Vector =
            Array.zip left right |> Array.map (fun (l, r) -> l + r)

        let (*) (v: Vector) (scale: float) : Vector = v |> Array.map ((*) scale)

        let (/) (v: Vector) (scale: float) : Vector = v * (1.0/scale)

        let mean (vectors: Vector array) =
            ( vectors |> Array.reduce (fun acc v -> acc + v) ) / float(vectors.Length)

    module Combinatorics =
        let rec splits = function
            | [] -> Seq.singleton([],[])
            | x::xs -> seq { 
                for l1,l2 in splits xs do
                    yield x::l1,l2
                    yield l1,x::l2
            }

        let parts l =
            let rec parts' = function
                | 0,[] -> Seq.singleton []
                | _,[] -> Seq.empty
                | 1,l -> Seq.singleton [l]
                | n,x::xs ->
                    seq {
                        for l1,l2 in splits xs do
                        for p in parts'(n-1, l2) do
                            yield (x::l1)::p
                    }
        
            seq { 
                for k = 1 to List.length l do
                    yield! parts'(k,l)
            }

        let partition (groupSize: int) (lst: 'a list) =
            let nGroups = lst.Length / groupSize
            let nMax = nGroups + lst.Length % groupSize

            printfn "Min groups: %A, max groups: %A" nGroups nMax

            parts lst 
            |> Seq.skipWhile (fun l -> l.Length < nGroups) // TODO: Length is O(n) on lists, use arrays?
            |> Seq.takeWhile (fun l -> l.Length <= nMax)
            |> Seq.filter(fun partition -> // filter out all overbooked groups
                partition |> Seq.filter (fun group -> group.Length > groupSize) |> Seq.length = 0)
