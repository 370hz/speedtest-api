module Speedtest.Api.Model.SpeedtestModel.Speedtest

open System

type Speedtest = {
    Id: Guid
    Download: float
    Timestamp: int64
}

type Speedtests = Speedtest list

let postSpeedtests (storeSpeedtest : Speedtest -> Speedtest) (speedtest : Speedtest) : Speedtest =
    printfn "Id: %s Download: %f Timestamp: %i" (string speedtest.Id) speedtest.Download speedtest.Timestamp
    storeSpeedtest speedtest

let getSpeedtests (allSpeedtests : _ -> Speedtests) : Speedtests =
    printfn "Getting all speedtests"
    allSpeedtests ()