module Speedtest.Api.WebApp

open System
open Microsoft.AspNetCore.Http
open Giraffe.HttpHandlers
open Giraffe.Tasks
open Giraffe.HttpContextExtensions

type Speedtest = {
    Id: Guid
    Download: double
    Timestamp: int
}

type Speedtests = Speedtest list

let s = {
    Id = Guid.NewGuid()
    Download = 20.0
    Timestamp = 1234
}

let sss = [s; s; s]

let getSpeedtests =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            return! json sss next ctx
        }

let postSpeedtests =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! speedtest = ctx.BindJson<Speedtest>()
            return! json speedtest next ctx
        }

let webApp : HttpHandler =
    choose [
        route "/speedtests" >=>
            choose [
                GET >=> getSpeedtests
                POST >=> postSpeedtests]
        setStatusCode 404 >=> text "Not Found"
    ]