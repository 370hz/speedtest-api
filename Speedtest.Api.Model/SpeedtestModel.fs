module Speedtest.Api.Model.SpeedtestModel

open System

//[<CLIMutable>]
type Speedtest = {
    Id: Guid
    Download: double
    Timestamp: int
}

type Speedtests = Speedtest list

let now () = System.DateTime.UtcNow

let postSpeedtests (speedtest : Speedtest) : Speedtest =
    printfn "Id: %s Download: %f Timestamp: %i" (string speedtest.Id) speedtest.Download speedtest.Timestamp
    speedtest

let getSpeedtests =
    printfn "Getting all speedtests"
    let s = {
        Id = Guid.NewGuid()
        Download = 20.0
        Timestamp = 1234
    }
    [s; s; s]