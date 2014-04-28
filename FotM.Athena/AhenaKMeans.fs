namespace FotM.Athena

open Math

type NormalizeParams = {
    mean: float
    scale: float
}

type AthenaKMeans<'a>(featureExtractor: 'a -> float array, shouldNormalize: bool, applyDistortionMetric: bool) =

    (*
        NB! matrix has m rows (number of training examples) and n columns (number of features)

        1. init k centroids
        2. 
    *)

    let distance = squaredEuclideanDistance

    let ``kmeans++`` (matrix: float[][]) (k: int) (rng: System.Random) =
        let rec buildCentroids(centroids: float[] list)(i: int)=
            if i < k then 
                let distanceToClosestCentroid(v: Vector): float = centroids |> List.map (distance v) |> List.min

                let distances = matrix |> Array.map distanceToClosestCentroid

                let border = rng.NextDouble() * Array.sum(distances)

                let rec nextCentroid runningSum i =
                    if runningSum < border then nextCentroid (runningSum + distances.[i]) (i+1)
                    else matrix.[i-1]
                
                let next = nextCentroid 0.0 0

                buildCentroids (next::centroids) (i+1)
            else
                centroids
                                
        buildCentroids [matrix.[rng.Next(matrix.Length)]] 1

    let getPointsForCluster (clustering: int[]) (i: int) (matrix: Vector[])  =
        clustering
        |> Array.mapi (fun i ci -> i, ci)
        |> Array.filter (fun idx -> snd idx = i)
        |> Array.map (fun idx -> matrix.[fst idx])

    let cluster(matrix: float[][])(k: int): Vector[] * int[] =

        let n = matrix.[0].Length
        let centroids = ``kmeans++`` matrix k (System.Random())

        let rec iterate (centroids: Vector list) (currentClustering: int[]) =
            // I. assignment step
            let newClustering = matrix |> Array.map (fun input -> fst( centroids |> List.miniBy(distance input) ) )

            if newClustering <> currentClustering then
                // II. update step
                let newCentroids = centroids |> List.mapi (fun i c -> matrix |> getPointsForCluster newClustering i |> VectorOps.mean n)
                iterate newCentroids newClustering
            else
                centroids |> List.toArray, currentClustering

        iterate centroids [||]

    let normalize(matrix: float[][]): float[][] =
        let columnStats (col: int) =
            let column = matrix |> Array.map (fun row -> row.[col])
            let min = column |> Array.min
            let max = column |> Array.max
            {
                mean = column |> Array.average
                scale  = if (max - min = 0.0) then 1.0 else (max - min)
            }

        let n = matrix.[0].Length
        let m = matrix.Length

        let stats = [0..n-1] |> List.map columnStats

        [|
        for i in 0..m-1 do
        yield
            [|
            for j in 0..n-1 do
            yield (matrix.[i].[j] - stats.[j].mean) / (stats.[j].scale)
            |]
        |]

    let distortionMetric (matrix: float[][]) (centroids: Vector[], clustering: int[]) : float = 
        matrix
        |> Array.mapi (fun i row -> distance row centroids.[clustering.[i]])
        |> Array.sum

    interface FotM.Utilities.IKMeans<'a> with
        member this.ComputeGroups(dataSet, nGroups) =
            let input = dataSet |> Array.map featureExtractor
            let matrix = if shouldNormalize then normalize input else input

            if applyDistortionMetric then
                let orderedResults = 
                    [for i in 0..50 do yield cluster matrix nGroups]
                    |> List.sortBy (distortionMetric matrix)

                snd orderedResults.Head
            else
                snd (cluster matrix nGroups)
