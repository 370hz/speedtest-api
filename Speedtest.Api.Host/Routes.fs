module Speedtest.Api.Host.Routes

open System
open Microsoft.AspNetCore.Http
open Giraffe.HttpHandlers
open Giraffe.Tasks
open Giraffe.HttpContextExtensions

open Speedtest.Api.Model

let getSpeedtests (next : HttpFunc) (ctx : HttpContext) =
    task {
        return! json SpeedtestModel.getSpeedtests next ctx
    }

let postSpeedtests (next : HttpFunc) (ctx : HttpContext) =
    task {
        let! speedtest = ctx.BindJson<SpeedtestModel.Speedtest>()
        return! json (SpeedtestModel.postSpeedtests speedtest) next ctx
    }

let routeHandler : HttpHandler =
    choose [
        route "/speedtests" >=>
            choose [
                GET >=> getSpeedtests
                POST >=> postSpeedtests]
        setStatusCode 404 >=> text "Not Found"
    ]
