namespace FotM.Athena

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
        
    type AthenaKMeans<'a>(distance: float[] -> float[] -> float) =

        (*
            NB! matrix has m rows (number of training examples) and n columns (number of features)

            1. init k centroids
            2. 
        *)

        let rng = System.Random()

        let ``kmeans++`` matrix k m n =
            
            let rec buildCentroids(centroids: float[] list)(i: int)=
                if i < k then 
                    let distanceToClosestCentroid(v: Vector): float = centroids |> List.map (squaredEuclideanDistance v) |> List.min

                    let dx2 = matrix |> Array.map distanceToClosestCentroid

                    let border = rng.NextDouble() * Array.sum(dx2)

                    let rec nextCentroid runningSum idx =
                        if runningSum < border then nextCentroid (runningSum+dx2.[idx]) (idx+1)
                        else matrix.[idx-1]
                
                    let next = nextCentroid 0.0 0

                    buildCentroids (next::centroids) (i+1)
                else
                    centroids
            
            buildCentroids [matrix.[rng.Next(m)]] 0
                
        let compute(matrix: float[][])(k: int): int[] =

            let m = matrix.Length
            let n = matrix.[0].Length
            let centroids = ``kmeans++`` matrix k m n

            let rec iterate result =
                // I. assignment step
                // II. update step
                0

            [||]

        interface FotM.Utilities.IKMeans<'a> with
            member this.ComputeGroups(dataSet, nGroups) =
                null

        