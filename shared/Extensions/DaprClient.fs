module Shared.Extensions

open Dapr.Client

type DaprClient with

    /// <summary>
    /// Helper function to handle the non-existence of the votes in the data store in a functional way, by returning
    /// `None` for null and `Some` for every valid state.
    /// </summary>
    /// <param name="storeName">The name of state store to read from.</param>
    /// <param name="key">The state key.</param>
    /// <returns>Some 'T if the key exists iun store, None otherwise.</returns>
    member this.GetStateAsyncF<'T>(storeName: string, key: string): Async<Option<'T>> =
        async {
            let! state = this.GetStateAsync<'T>(storeName, key).AsTask() |> Async.AwaitTask

            return if isNull (box state) then None else Some(state)
        }
