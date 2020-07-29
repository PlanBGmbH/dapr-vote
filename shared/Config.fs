module Shared.Config

open Dapr.Client
open System.Text.Json
open System.Text.Json.Serialization

// Use https://github.com/Tarmil/FSharp.SystemTextJson to provide F# interoperability for JSON serialization
let jsonConverter = JsonFSharpConverter(JsonUnionEncoding.InternalTag
    ||| JsonUnionEncoding.UnwrapFieldlessTags
    ||| JsonUnionEncoding.UnwrapOption)
let jsonSerializerOptions = JsonSerializerOptions()
jsonSerializerOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
jsonSerializerOptions.PropertyNameCaseInsensitive <- true
jsonSerializerOptions.Converters.Add(jsonConverter)

module Apps =
    let notifications = "notifications"
    let subscriptions = "subscriptions"
    let votes = "votes"

module StateStore =
    let name = "statestore"

module Dapr =
    let client = DaprClientBuilder().UseJsonSerializationOptions(jsonSerializerOptions).Build()
