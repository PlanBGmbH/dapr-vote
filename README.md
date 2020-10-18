# [WIP] A voting application based on dapr

Note: This example may not follow best practices. It's a playground for me, to get comfortable with the .NET platform
and dapr. This may change in the feature. Please don't hesitate to share your suggestions via GitHub issues.

An example application that uses different [dapr] concepts like [Actors], [Service-to-Service Invocation],
[State Management] and [Publish and Subscribe].

The application consists of three services:

| Service           | Description                     | Language   |
| ----------------- | ------------------------------- | ---------- |
| `votes`           | Handles the voting              | F#         |
| `subscriptions`   | Handles the subscriptions       | F#         |
| `notifications`   | Allows to subscribe for updates | C#         |
| `frontend`        | The react frontend application  | TypeScript |

[dapr]: https://dapr.io/
[Actors]: https://github.com/dapr/docs/blob/master/concepts/actors/README.md
[Service-to-Service Invocation]: https://github.com/dapr/docs/blob/master/concepts/service-invocation/README.md
[State Management]: https://github.com/dapr/docs/blob/master/concepts/state-management/README.md
[Publish and Subscribe]: https://github.com/dapr/docs/blob/master/concepts/publish-subscribe-messaging/README.md

## Architectural decisions

### Using actors to manipulate state

The update process with Dapr's state store, isn't an atomic operation. We must first retrieve the data from
the store, manipulate the data and store it again with the updated data. To not lose data because of race 
conditions, we use actors to handle the state manipulation. Due to it's nature an actor processes it's mail 
box sequentially. Therefore we don't need to implement locking or synchronization in our services.

## Implement a gRPC API and call it from another service

The Dapr.NET SDK documentation is a bit vague when it comes to describing the implementation of a gRPC API and how 
they will be called from another service. After some googling, I found out that an application must implement the 
`AppCallback` service based on the [appcallback.proto]. The `AppCallback` service defines an `OnInvoke` method which 
must be implemented. This method can then be called with the `DaprClient.InvokeMethodAsync` method. In our case, we 
have implemented this method as a proxy that calls our gRPC API. The concrete implementation can be found in the 
`notifications/Services/DaprService` file.

All proto files are located in a separate `proto` project, so that it can be referenced by the other projects.

[appcallback.proto]: https://github.com/dapr/dapr/blob/master/dapr/proto/runtime/v1/appcallback.proto

## Run the votes service

```
dapr run --app-id votes --app-port 3000 -- dotnet run --project "./votes/votes.fsproj"
```

## Run the subscriptions service

```
dapr run --app-id subscriptions --app-port 3001 -- dotnet run --project "./subscriptions/subscriptions.fsproj"
```

## Run the notifications service

The notification service uses a gRPC instead of a REST API. This must be activated by the `--protocol grpc` argument.

```
dapr run --app-id notifications --app-port 3002 --app-protocol grpc -- dotnet run --project "./notifications/notifications.csproj"
```

## Run a local mail server

The notification service sends emails to subscribed users. To run a local email server for development, the [maildev] package can be used.

```
npm install -g maildev
maildev --smtp 1025 --web 8080
```

This starts a local mail server on port 1025. The incoming mails can be viewed on http://localhost:8080

[maildev]: https://github.com/maildev/maildev
