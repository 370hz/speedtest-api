module Speedtest.Api.Host.Storage

open System
open System.IO
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Configuration.Json
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table

type Customer(firstName, lastName, email: string, phone: string) =
    inherit TableEntity(partitionKey=lastName, rowKey=firstName)
    new() = Customer(null, null, null, null)
    member val Email = email with get, set
    member val PhoneNumber = phone with get, set

let asyncQuery (table : CloudTable) (query : TableQuery<'a>) = 
    let rec loop (cont: TableContinuationToken) = async {
        let! ct = Async.CancellationToken
        let! result = table.ExecuteQuerySegmentedAsync(query, cont) |> Async.AwaitTask

        for customer in result do 
            printfn "customer: %A %A" customer.RowKey customer.PartitionKey
        
        // Continue to the next segment
        match result.ContinuationToken with
        | null -> ()
        | cont -> return! loop cont 
    }
    loop null

let testStorage =
    let configuration = 
        ConfigurationBuilder()
          .AddJsonFile("appsettings.json")
          .Build()
    
    printfn "Hello World from F#!"

    // fill this in from your storage account
    let storageConnString = configuration.GetConnectionString "azspeedtestdata"
    let huhString = configuration.GetConnectionString "lulwhuut?"

    // Parse the connection string and return a reference to the storage account.
    let storageAccount = CloudStorageAccount.Parse(storageConnString)
    let tableClient = storageAccount.CreateCloudTableClient()
    // Retrieve a reference to the table.
    let table = tableClient.GetTableReference("people")
    // Create the table if it doesn't exist.
    table.CreateIfNotExistsAsync() |> Async.AwaitTask |> Async.RunSynchronously |>  ignore //.CreateIfNotExists()

    let customer = Customer("Yusef", "Harp", "Yusef@contoso.com", "425-555-0101")
    let insertOp = TableOperation.InsertOrReplace(customer)
    table.ExecuteAsync(insertOp) |> Async.AwaitTask |> Async.RunSynchronously |> ignore

    let query =
        TableQuery<Customer>().Where(
            TableQuery.GenerateFilterCondition(
                "PartitionKey", QueryComparisons.Equal, "Harp"))

    asyncQuery table query |> Async.RunSynchronously |> ignore
