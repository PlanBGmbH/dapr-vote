# [WIP] A voting application based on dapr

Note: This example may not follow best practices. It's a playground for me, to get comfortable with the .NET platform
and dapr. This may change in the feature. Please don't hesitate to share your suggestions via GitHub issues.

An example application that uses different [dapr] concepts like [Actors], [Service-to-Service Invocation],
[State Management] and [Publish and Subscribe].

The application consists of four services:

| Service           | Description                     | Language   |
| ----------------- | ------------------------------- | ---------- |
| `votes`           | Handles the voting              | F#         |
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

## Run the votes service

```
dapr run --app-id votes --app-port 3000 -- dotnet run --project "./votes/votes.fsproj"
```

## Run the notifications service

```
dapr run --app-id notifications --app-port 3001 --protocol grpc -- dotnet run --project "./notifications/notifications.csproj"
```
