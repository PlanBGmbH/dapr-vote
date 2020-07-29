module Shared.Extensions

open Dapr.Client
open Dapr.Actors.Runtime
open System.Collections.Generic
open Microsoft.FSharp.Control

type DaprClient with

    /// <summary>
    /// Helper function to handle the non-existence of the state in the data store in a functional way, by returning
    /// `None` for null and `Some` for every valid state.
    /// </summary>
    /// <param name="storeName">The name of state store to read from.</param>
    /// <param name="key">The state key.</param>
    /// <returns>Some 'T if the key exists in store, None otherwise.</returns>
    member this.GetStateAsyncO<'T>(storeName: string, key: string): Async<Option<'T>> =
        async {
            let! state = this.GetStateAsync<'T>(storeName, key).AsTask() |> Async.AwaitTask

            return if isNull (box state) then None else Some(state)
        }

    /// <summary>
    /// Helper function to handle the non-existence of the state in the data store in a functional way, by returning
    /// `defaultValue` for null and `Some` for every valid state.
    /// </summary>
    /// <param name="storeName">The name of state store to read from.</param>
    /// <param name="key">The state key.</param>
    /// <param name="defaultValue">The value to return if the result was `None`.</param>
    /// <returns>'T if the key exists in store, `defaultValue` otherwise.</returns>
    member this.GetStateAsyncOr<'T>(storeName: string, key: string, defaultValue: 'T): Async<'T> =
        async {
            let! state = this.GetStateAsyncO(storeName, key)

            return state |> Option.defaultValue(defaultValue)
        }

type IActorStateManager with

    /// <summary>
    /// Helper function to handle the non-existence of the state in the data store in a functional way, by returning
    /// `None` for null and `Some` for every valid state.
    ///
    /// The state manager throws an exception if the `stateName` doesn't exists in the store. We catch that exception
    /// and return `None` in this case. If we get the result, we return `Some`.
    /// </summary>
    /// <param name="stateName">Name of the actor state to get.</param>
    /// <returns>Some 'T if the key exists in store, None otherwise.</returns>
    member this.GetStateAsyncO<'T>(stateName: string): Async<Option<'T>> =
        async {
            let! maybeState = this.GetStateAsync<'T>(stateName) |> Async.AwaitTask |> Async.Catch
            match maybeState with
            | Choice1Of2 state -> return if isNull (box state) then None else Some(state)
            | Choice2Of2 error ->
                match error with
                | :? System.Collections.Generic.KeyNotFoundException -> return None
                | :? System.AggregateException ->
                    match error.InnerException with
                    | :? System.Collections.Generic.KeyNotFoundException -> return None
                    | _ -> return raise error
                | _ -> return raise error
        }

    /// <summary>
    /// Helper function to handle the non-existence of the state in the data store in a functional way, by returning
    /// `defaultValue` for null and `Some` for every valid state.
    /// </summary>
    /// <param name="stateName">Name of the actor state to get.</param>
    /// <param name="defaultValue">The value to return if the result was `None`.</param>
    /// <returns>'T if the key exists in store, `defaultValue` otherwise.</returns>
    member this.GetStateAsyncOr<'T>(stateName: string, defaultValue: 'T): Async<'T> =
        async {
            let! state = this.GetStateAsyncO(stateName)

            return state |> Option.defaultValue(defaultValue)
        }

type Dictionary<'A, 'B> with

    /// <summary>
    /// Helper function to handle the non-existence of the key in the dictionary in a functional way, by returning
    /// `None` for null and `Some` for the existing key.
    /// </summary>
    /// <param name="key">The state key.</param>
    /// <returns>Some 'T if the key exists iun store, None otherwise.</returns>
    member this.GetValueO(key: 'A): Option<'B> =
        let value = this.GetValueOrDefault<'A, 'B>(key)

        if isNull (box value) then None else Some(value)
