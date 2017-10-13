module Speedtest.Api.Host.Storage

open System
open System.IO
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Configuration.Json
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table

open Speedtest.Api.Model.SpeedtestModel.Speedtest

type SpeedtestTableEntity (id : string, download : string, timestamp : string, timestampPartition : string) =
    inherit TableEntity(partitionKey=timestampPartition, rowKey=id)
    new() = SpeedtestTableEntity(null, null, null, null)
    member val Download = download with get, set
    member val Timestamp = timestamp with get, set

let asyncQuery (table : CloudTable) (query : TableQuery<SpeedtestTableEntity>) = 
    let rec loop (cont: TableContinuationToken) (acc : SpeedtestTableEntity list) = async {
        let! ct = Async.CancellationToken
        let! result =
            table.ExecuteQuerySegmentedAsync(query, cont)
            |> Async.AwaitTask

        let tests = result |> List.ofSeq |> List.append acc

        printfn "Querying table storage length: %i" tests.Length
        for customer in result do 
            printfn "Speedtest: %A %A %A %A" customer.RowKey customer.PartitionKey customer.Timestamp customer.Download
        
        match result.ContinuationToken with
        | null -> return tests
        | cont -> return! loop cont tests
    }
    loop null []

let cloudTable =
    let storageConnString = "DefaultEndpointsProtocol=https;AccountName=speedtestdata;AccountKey=Dk0SqzZiji8Svik1zP+ByQCgkq7qdDndJXcTc3UJ3ovpn3zSOq0yj8bmAdmBOF2Yp3K5BjonYVzXn5crp9jGXw==;EndpointSuffix=core.windows.net"
    let storageAccount = CloudStorageAccount.Parse(storageConnString)
    let tableClient = storageAccount.CreateCloudTableClient()
    let table = tableClient.GetTableReference("speedtests")
    table.CreateIfNotExistsAsync() |> Async.AwaitTask |> Async.RunSynchronously |> ignore
    table

let storeSpeedtest (table : CloudTable) (speedtest : Speedtest) : Speedtest =
    let ts = speedtest.Timestamp
    let part = speedtest.Timestamp / int64 86400
    let speedtestTable = SpeedtestTableEntity(string speedtest.Id, string speedtest.Download, string ts, string part)

    printfn "Speedtest: %A %A %A %A" speedtestTable.RowKey speedtestTable.PartitionKey speedtestTable.Timestamp speedtestTable.Download

    let insertOp = TableOperation.InsertOrReplace(speedtestTable)
    table.ExecuteAsync(insertOp) |> Async.AwaitTask |> Async.RunSynchronously |> ignore
    speedtest

let toSpeedtest (entity : SpeedtestTableEntity) : Speedtest =
    printfn "Debug argument null: %A - %A - %A" entity.RowKey entity.Download entity.Timestamp
    {
        Id = Guid.Parse(entity.RowKey)
        Download = float entity.Download
        Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
        //Timestamp = int64 entity.Timestamp
    }

let allSpeedtests (table : CloudTable) _ : Speedtests =
    let query = TableQuery<SpeedtestTableEntity>()
    
    asyncQuery table query
    |> Async.RunSynchronously
    |> List.map toSpeedtest
