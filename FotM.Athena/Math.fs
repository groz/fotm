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
        
    type AthenaKMeans<'a>(distance: float[] -> float[] -> float) =

        (*
            NB! matrix has m rows (number of training examples) and n columns (number of features)

            1. init k centroids
            2. 
        *)

        let rng = System.Random()

        let ``kmeans++`` matrix k =            
            let rec buildCentroids(centroids: float[] list)(i: int)=
                if i < k then 
                    let distanceToClosestCentroid(v: Vector): float = centroids |> List.map (squaredEuclideanDistance v) |> List.min

                    let distances = matrix |> Array.map distanceToClosestCentroid

                    let border = rng.NextDouble() * Array.sum(distances)

                    let rec nextCentroid runningSum i =
                        if runningSum < border then nextCentroid (runningSum + distances.[i]) (i+1)
                        else matrix.[i-1]
                
                    let next = nextCentroid 0.0 0

                    buildCentroids (next::centroids) (i+1)
                else
                    centroids
                                
            buildCentroids [matrix.[rng.Next(matrix.Length)]] 0

        let getPointsForCluster (clustering: int[]) (i: int) (matrix: Vector[])  =
            clustering
            |> Array.mapi (fun i ci -> i, ci)
            |> Array.filter (fun idx -> snd idx = i)
            |> Array.map (fun idx -> matrix.[fst idx])

        let cluster(matrix: float[][])(k: int): int[] =

            let n = matrix.[0].Length
            let centroids = ``kmeans++`` matrix k

            let rec iterate (centroids: Vector list) (currentClustering: int[]) =
                // I. assignment step
                let newClustering = matrix |> Array.map (fun input -> fst( centroids |> List.miniBy(squaredEuclideanDistance input) ) )

                if newClustering <> currentClustering then
                    // II. update step
                    let newCentroids = centroids |> List.mapi (fun i c -> matrix |> getPointsForCluster newClustering i |> VectorOps.mean n)
                    iterate newCentroids newClustering
                else
                    currentClustering

            iterate centroids [||]

        interface FotM.Utilities.IKMeans<'a> with
            member this.ComputeGroups(dataSet, nGroups) =
                null

        