module Shared.Extensions

open Dapr.Client

type DaprClient with

    /// <summary>
    /// Helper function to handle the non-existence of the votes in the data store in a functional way, by returning
    /// the `Option` type.
    ///
    /// The state manager throws an exception if the key "votes" doesn't exists in the store. We catch that exception
    /// and return `None` in this case. If we get the result, we return `Some`.
    /// </summary>
    /// <param name="storeName">The name of state store to read from.</param>
    /// <param name="key">The state key.</param>
    /// <returns>Some 'T if the key exists iun store, None otherwise.</returns>
    member this.GetStateAsyncF<'T>(storeName: string, key: string): Async<Option<'T>> =
        async {
            try
                let! votes = this.GetStateAsync<'T>(storeName, key).AsTask() |> Async.AwaitTask

                return Some(votes)
            with :? System.Collections.Generic.KeyNotFoundException -> return None
        }
