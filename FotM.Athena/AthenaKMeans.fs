namespace FotM.Athena

type AthenaKMeans<'a> = 
    interface FotM.Utilities.IKMeans<'a> with
        member this.ComputeGroups(dataSet, nGroups) =
            null

    member this.ComputeGroups(matrix, nGroups) =
        null